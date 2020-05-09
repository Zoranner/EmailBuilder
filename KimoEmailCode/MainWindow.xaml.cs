using CefSharp.Wpf;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = HandyControl.Controls.MessageBox;

namespace KimoEmailCode
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private string _TemplateContent;
        private string _TemporaryContent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

            var templatePath = $"{templatesPath}\\Template.html";
            var temporaryPath = $"{temporariesPath}\\Temporary.html";
            Console.WriteLine(templatePath);

            if (!File.Exists(templatePath))
            {
                htmlBrowser.Visibility = Visibility.Hidden;
                errorText.Text = "模板文件不存在，请检查后重试！";
            }
            else
            {
                _TemplateContent = File.ReadAllText(templatePath, Encoding.UTF8);
                File.WriteAllText(temporaryPath, _TemplateContent, Encoding.UTF8);
                htmlBrowser.Address = temporaryPath;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void 
    }
}
