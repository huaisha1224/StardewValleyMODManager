using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Semver;
using Stardrop.Utilities;

namespace Stardrop.Utilities.Internal
{
    /// <summary>
    /// 版本更新检查器，支持比较 FileVersion(读取程序集)与失败回退到 SemVer格式
    /// </summary>
    internal static class UpdateChecker
    {
        private static readonly HttpClient _hc = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private const string CheckUrl = "https://api.hs2049.cn/tools/StardewModManager";

        public static async Task CheckForUpdateAsync(Window owner)
        {
            try
            {
                // 设置HttpClient使用UTF-8编码
                _hc.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                var resp = await _hc.GetStringAsync(CheckUrl).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(resp);
                var root = doc.RootElement;

                // 首先读取远程 FileVersion，没有则回退到 Version
                var remoteStr = (root.TryGetProperty("FileVersion", out var fvEl) ? fvEl.GetString() : null)
                                ?? (root.TryGetProperty("Version", out var vEl) ? vEl.GetString() : null)
                                ?? string.Empty;
                var downloadUrl = root.TryGetProperty("DownloadUrl", out var dEl) ? dEl.GetString() : null;

                // 本地：使用 AssemblyFileVersion作为 <FileVersion> 名称，
                var localFileStr = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? string.Empty;

                // 本地：信息版本作为 <Version>/<AssemblyInformationalVersion> 名称，
                var localInfoStr = typeof(Program).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

                bool updateAvailable = false;
                string displayLocal = string.Empty, displayRemote = string.Empty;

                // 比较1：使用FileVersion版本
                if (VersionTryParse(remoteStr, out var remoteFileVer) && VersionTryParse(localFileStr, out var localFileVer))
                {
                    updateAvailable = remoteFileVer > localFileVer;
                    displayLocal = localFileVer.ToString();
                    displayRemote = remoteFileVer.ToString();
                    Program.helper.Log($"[UpdateCheck] FileVersion compare: local={displayLocal}, remote={displayRemote}");
                }
                // 比较2：SemVer格式比较 2.2.4 或 v2.2.4格式
                else if (SemVersion.TryParse(remoteStr?.TrimStart('v', 'V'), SemVersionStyles.Any, out var remoteSem)
                      && SemVersion.TryParse(localInfoStr?.TrimStart('v', 'V'), SemVersionStyles.Any, out var localSem))
                {
                    updateAvailable = remoteSem > localSem;
                    displayLocal = localSem.ToString();
                    displayRemote = remoteSem.ToString();
                    Program.helper.Log($"[UpdateCheck] SemVer compare: local={displayLocal}, remote={displayRemote}");
                }
                else
                {
                    Program.helper.Log($"[UpdateCheck] Unable to parse versions. remote='{remoteStr}', localFile='{localFileStr}', localInfo='{localInfoStr}'", Helper.Status.Warning);
                    return;
                }

                if (!updateAvailable)
                {
                    Program.helper.Log("[UpdateCheck] Already up to date.");
                    return;
                }

                var msg = $"检测到新版本：{displayRemote}\n当前版本：{displayLocal}\n是否前往下载？";
                var dialog = new Stardrop.Views.MessageWindow(msg, "确认", "取消");
                var result = owner is null
                    ? await dialog.ShowDialog<bool?>(null)
                    : await dialog.ShowDialog<bool?>(owner);

                if (result == true && !string.IsNullOrWhiteSpace(downloadUrl))
                {
                    Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Program.helper.Log($"检查更新失败: {ex}", Helper.Status.Warning);
            }
        }

        private static bool VersionTryParse(string? s, out Version v)
        {
            v = new Version(0, 0, 0, 0);
            try
            {
                if (string.IsNullOrWhiteSpace(s)) return false;
                v = new Version(s);
                return true;
            }
            catch { return false; }
        }
    }
}