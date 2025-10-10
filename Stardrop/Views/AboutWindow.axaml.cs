using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Stardrop.Utilities;
using System.Diagnostics;

namespace Stardrop.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // Set the description text with translations when window is opening
            this.Opened += AboutWindow_Opened;
        }

        private void AboutWindow_Opened(object? sender, System.EventArgs e)
        {
            // 设置关于页面的描述文本
            this.FindControl<TextBlock>("descriptionTextBlock").Text = Program.translation.Get("ui.about_window.description");
        }

        /// <summary>
        /// 使用教程按钮点击事件处理方法
        /// 点击后在默认浏览器中打开使用教程网页
        /// </summary>
        private void TutorialButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://hs2049.cn") { UseShellExecute = true });
        }

        /// <summary>
        /// GitHub源码按钮点击事件处理方法
        /// 点击后在默认浏览器中打开GitHub项目页面
        /// </summary>
        private void GitHubButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/huaisha1224/StardewValleyMODManager") { UseShellExecute = true });
        }

        /// <summary>
        /// Stardrop源码按钮点击事件处理方法
        /// 点击后在默认浏览器中打开Stardrop源码页面
        /// </summary>
        private void StardropButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/Floogen/Stardrop") { UseShellExecute = true });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}