using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Zoranner.Upgrader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public const string PROGRAM_PATH = "Resource";
        public const string FILEHASH_PATH = "filehash.txt";
        private string _FileHashGroup = "";

        public Downqueue Downqueue;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DownName.Text = "正在初始化...";
            DownPercent.Text = "0%";

            SigninDowner();
        }

        public void SigninDowner()
        {
            TryGetFileHash();

            if (Downqueue != null)
            {
                SignoutDowner();
            }

            var fileHashArray = _FileHashGroup.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            //if (fileHashArray.Length <= 0)
            //{
            //    return;
            //}

            Downqueue = new Downqueue();
            Downqueue.StateChangedEvent += Downqueue_StateChangedEvent;

            foreach (var fileHash in fileHashArray)
            {
                var httpFileInfo = new HttpFileInfo(fileHash);
                if (!httpFileInfo.Proper)
                {
                    continue;
                }

                var httpPath = httpFileInfo.Hash == ""
                    ? ""
                    : $"{Function.SERVER_URL}/{PROGRAM_PATH}/{httpFileInfo.Hash}";
                var localPath = $@"{Function.Instance.InstallPath}\{httpFileInfo.Path}";
                Downqueue.Add(httpPath, httpFileInfo.Hash, localPath.Replace(@"\\", @"\"));
            }

            Downqueue.Start();
        }

        private void Downqueue_StateChangedEvent(TaskState state, object message)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    switch (state)
                    {
                        case TaskState.Prepared:
                            DownProgress.Minimum = 0;
                            DownProgress.Maximum = 1;
                            break;
                        case TaskState.Loading:
                            DownName.Text = $"正在升级：{Downqueue.Caption}";
                            Console.WriteLine(Downqueue.Progress);
                            DownProgress.Value = Math.Round(Downqueue.Progress, 2);
                            DownPercent.Text = $"{(int) (Downqueue.Progress * 100)}%";
                            break;
                        case TaskState.Paused:
                            break;
                        case TaskState.Broken:
                            Console.WriteLine(((Exception) message).Message);
                            MessageBox.Show(((Exception)message).Message, "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            Close();
                            break;
                        case TaskState.Finished:
                            if (Downqueue != null)
                            {
                                SignoutDowner();
                            }

                            DownName.Text = "升级完成";
                            DownProgress.Value = 1;
                            DownPercent.Text = "100%";
                            //var programPath = $@"{Function.Instance.InstallPath}\Launcher.exe";
                            //if (File.Exists(programPath))
                            //{
                            //    Process.Start(programPath);
                            //}

                            Close();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(state), state, null);
                    }
                }
                catch
                {
                    // ignored
                }
            });
        }

        private void SignoutDowner()
        {
            Downqueue.StateChangedEvent -= Downqueue_StateChangedEvent;
            Downqueue.Clear();
        }

        private void TryGetFileHash()
        {
            try
            {
                var fixWebClient = new WebClient();
                var webStream = fixWebClient.OpenRead($"{Function.SERVER_URL}{FILEHASH_PATH}");
                var webStreamReader = new StreamReader(webStream, Encoding.UTF8);
                _FileHashGroup = webStreamReader.ReadToEnd();
                webStreamReader.Close();
                webStream.Close();
            }
            catch (Exception exception)
            {
                //bool? dialogResult = false;
                //m_MessageDialog = new MessageForm(Properties.Resources.WMW_VERSION_LOSING, MessageFormType.Error,
                //    () =>
                //    {
                //        // Retry
                //        m_MessageDialog.DialogResult = false;
                //    },
                //    () =>
                //    {
                //        // Quit
                //        m_MessageDialog.DialogResult = true;
                //        Application.Current.Shutdown();
                //    }
                //);
                //dialogResult = m_MessageDialog.ShowDialog();
                //if (!(dialogResult.HasValue && dialogResult.Value))
                //{
                //    TryGetFileHash();
                //}
                Console.WriteLine(exception.Message);
                MessageBox.Show(exception.Message, "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class HttpFileInfo
    {
        public string Hash;
        public string Path;
        public bool Proper;

        public HttpFileInfo(string fileHash)
        {
            var fileHashArray = fileHash.Split('|');
            if (fileHashArray.Length == 2)
            {
                Path = fileHashArray[0];
                Hash = fileHashArray[1];
                Proper = true;
            }
            else
            {
                Proper = false;
            }
        }
    }
}