using CefSharp;
using HandyControl.Controls;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Zoranner.EmailBuilder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private string _TemplatePath;
        private string _TemporaryPath;
        private string _TemplateContent;
        private string _TemporaryContent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            htmlBrowser.MenuHandler = new MenuHandler();

            var templatesPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Templates";
            var temporariesPath = $"{templatesPath}\\Temporary";
            Console.WriteLine(templatesPath);

            if (!Directory.Exists(templatesPath))
            {
                Directory.CreateDirectory(templatesPath);
            }
            if (!Directory.Exists(temporariesPath))
            {
                Directory.CreateDirectory(temporariesPath);
            }

            _TemplatePath = $"{templatesPath}\\Template.html";
            _TemporaryPath = $"{temporariesPath}\\Temporary.html";
            Console.WriteLine(_TemplatePath);

            if (!File.Exists(_TemplatePath))
            {
                htmlBrowser.Visibility = Visibility.Hidden;
                errorText.Text = "模板文件不存在，请检查后重试！";
            }
            else
            {
                _TemplateContent = File.ReadAllText(_TemplatePath, Encoding.UTF8);
                RefreshHtml(true);
            }
        }

        private void MobileText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[0-9]");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void MobileText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!mobileText.Text.Contains(" "))
            {
                return;
            }

            var cursorIndex = mobileText.SelectionStart;
            mobileText.Text = mobileText.Text.Replace(" ", "");
            mobileText.SelectionStart = cursorIndex - 1;
        }

        private void EmailText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!emailText.Text.Contains(" "))
            {
                return;
            }

            var cursorIndex = emailText.SelectionStart;
            emailText.Text = emailText.Text.Replace(" ", "");
            emailText.SelectionStart = cursorIndex - 1;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!htmlBrowser.IsBrowserInitialized)
            {
                return;
            }

            htmlBrowser.GetBrowser().Reload();
        }

        private void HtmlBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var initialized = (bool) e.NewValue;
            headerText.IsEnabled = initialized;
            contentText.IsEnabled = initialized;
            senderText.IsEnabled = initialized;
            mobileText.IsEnabled = initialized;
            emailText.IsEnabled = initialized;
            copyButton.IsEnabled = initialized;
            buildButton.IsEnabled = initialized;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var clipboardThread = new Thread(ClipboardThread);
            clipboardThread.SetApartmentState(ApartmentState.STA);
            clipboardThread.IsBackground = false;
            clipboardThread.Start();
            Growl.Info("源代码已复制到剪贴板，请打开邮箱进行粘贴。", "MainMessage");
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshHtml(true);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (!htmlBrowser.IsBrowserInitialized)
            {
                return;
            }

            htmlBrowser.GetBrowser().CloseBrowser(true);
            htmlBrowser.Dispose();
        }

        private void ClipboardThread()
        {
            try
            {
                Clipboard.SetText(_TemporaryContent);
            }
            catch { }
        }

        private void RefreshHtml(bool force = false)
        {
            if (!File.Exists(_TemporaryPath) || force)
            {
                //errorText.Text = "缓存文件损坏，请重新启动程序！";
                //return;
                BuildHtml();
            }

            if (!htmlBrowser.IsBrowserInitialized)
            {
                htmlBrowser.Address = _TemporaryPath;
            }
            else
            {
                htmlBrowser.GetBrowser().Reload();
            }
        }

        private void BuildHtml()
        {
            _TemporaryContent = _TemplateContent;

            if (!string.IsNullOrEmpty(headerText.Text))
            {
                _TemporaryContent = _TemporaryContent.Replace("[PH:HEADER]", headerText.Text);
            }

            if (!string.IsNullOrEmpty(contentText.Text))
            {
                _TemporaryContent = _TemporaryContent.Replace("[PH:CONTENT]", contentText.Text);
            }

            if (!string.IsNullOrEmpty(senderText.Text))
            {
                _TemporaryContent = _TemporaryContent.Replace("[PH:SENDER]", senderText.Text);
            }

            if (!string.IsNullOrEmpty(mobileText.Text))
            {
                _TemporaryContent = _TemporaryContent.Replace("[PH:MOBILE]", mobileText.Text);
            }

            if (!string.IsNullOrEmpty(emailText.Text))
            {
                _TemporaryContent = _TemporaryContent.Replace("[PH:E-MAIL]", emailText.Text);
            }
            
            File.WriteAllText(_TemporaryPath, _TemporaryContent, Encoding.UTF8);
        }
    }

    internal class MenuHandler : IContextMenuHandler
    {
        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            Console.WriteLine(@"OnContextMenuCommand");
            return false;
        }
        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            Console.WriteLine(@"OnContextMenuDismissed");
        }
        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            Console.WriteLine(@"RunContextMenu");
            return false;
        }
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            Console.WriteLine(@"OnBeforeContextMenu");
            model.Clear();
        }
    }
}
