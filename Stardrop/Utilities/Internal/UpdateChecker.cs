using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Semver;
using Stardrop.Utilities;

namespace Stardrop.Utilities.Internal
{
    /// <summary>
    /// �汾���¼������֧�����ȱȽ� FileVersion(�Ķ�����)��ʧ���ٻ��˵� SemVer��
    /// </summary>
    internal static class UpdateChecker
    {
        private static readonly HttpClient _hc = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private const string CheckUrl = "https://api.hs2049.cn/tools/StardewModManager";

        public static async Task CheckForUpdateAsync(Window owner)
        {
            try
            {
                var resp = await _hc.GetStringAsync(CheckUrl).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(resp);
                var root = doc.RootElement;

                // ���ȶ�ȡԶ�� FileVersion��û�����˻ص� Version
                var remoteStr = (root.TryGetProperty("FileVersion", out var fvEl) ? fvEl.GetString() : null)
                                ?? (root.TryGetProperty("Version", out var vEl) ? vEl.GetString() : null)
                                ?? string.Empty;
                var downloadUrl = root.TryGetProperty("DownloadUrl", out var dEl) ? dEl.GetString() : null;

                // ���أ�����ʹ�� AssemblyFileVersion���� <FileVersion> ���ƣ�
                var localFileStr = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? string.Empty;

                // ���ã���Ϣ�汾���� <Version>/<AssemblyInformationalVersion> ���ƣ�
                var localInfoStr = typeof(Program).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

                bool updateAvailable = false;
                string displayLocal = string.Empty, displayRemote = string.Empty;

                // ����1���Ķ����ְ汾��FileVersion��
                if (VersionTryParse(remoteStr, out var remoteFileVer) && VersionTryParse(localFileStr, out var localFileVer))
                {
                    updateAvailable = remoteFileVer > localFileVer;
                    displayLocal = localFileVer.ToString();
                    displayRemote = remoteFileVer.ToString();
                    Program.helper.Log($"[UpdateCheck] FileVersion compare: local={displayLocal}, remote={displayRemote}");
                }
                // ����2��SemVer������ 2.2.4 �� v2.2.4��
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

                var msg = $"��⵽�°汾��{displayRemote}\n��ǰ�汾��{displayLocal}\n�Ƿ�ǰ�����أ�";
                var dialog = new Stardrop.Views.MessageWindow(msg, "ȷ��", "ȡ��");
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
                Program.helper.Log($"������ʧ��: {ex}", Helper.Status.Warning);
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