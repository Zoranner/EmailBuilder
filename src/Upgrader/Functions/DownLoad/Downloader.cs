using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Zoranner.Upgrader
{
    public enum TaskState
    {
        Prepared,
        Loading,
        Paused,
        Broken,
        Finished
    }

    public class Downloader
    {
        /// <summary>
        /// 每次读取的字节数
        /// </summary>
        private const int READ_BYTES = 1000 * 1024;

        public string Name { get; private set; }

        //public bool IsLoading { get; private set; }

        //public bool Completed { get; private set; }

        public string HttpPath { get; private set; }

        public string HttpHash { get; }

        public string LocalPath { get; private set; }

        public TaskState State { get; private set; }

        /// <summary>
        /// 已下载的字节数
        /// </summary>
        public long LoadedBytes { get; private set; }

        /// <summary>
        /// 要下载的字节总数
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// 当前进度
        /// </summary>
        public float Progress => TotalBytes == 0 ? 1 : (float) LoadedBytes / TotalBytes;

        ///// <summary>
        ///// 即时速度
        ///// </summary>
        //public float Speed { get; set; }

        //public event DownloadStartedHandler StartedEvent;
        //public event DownloadPausedHandler PausedEvent;
        //public event DownloadProgressChangedHandler ProgressChangedEvent;
        //public event DownloadErrorBrokenHandler ErrorBrokenEvent;
        //public event DownloadCompletedHandler CompletedEvent;
        public event DownloadStateChangedHandler StateChangedEvent;

        public Downloader(string httpPath, string httpHash, string localPath)
        {
            //IsLoading = false;
            //Completed = false;
            SetState(TaskState.Prepared);
            var pathParts = localPath.Split('\\');
            Name = $@"{pathParts[0]}\..\{pathParts[pathParts.Length - 2]}\{pathParts[pathParts.Length - 1]}";
            HttpPath = httpPath;
            HttpHash = httpHash;
            LocalPath = localPath;
        }

        public void Start()
        {
            new Thread(() =>
            {
                if (State != TaskState.Prepared)
                {
                    return;
                }

                Console.WriteLine(LocalPath);
                Console.WriteLine(HttpPath);

                if (string.IsNullOrEmpty(LocalPath))
                {
                    return;
                }

                if (string.IsNullOrEmpty(HttpPath))
                {
                    if (File.Exists(LocalPath))
                    {
                        File.Delete(LocalPath);
                    }
                    else if (Directory.Exists(LocalPath))
                    {
                        Directory.Delete(LocalPath, true);
                    }

                    //IsLoading = false;
                    //Completed = true;
                    //CompletedEvent?.Invoke(this);
                    SetState(TaskState.Finished);
                    return;
                }


                try
                {
                    if (!HttpFileExist(HttpPath))
                    {
                        return;
                    }

                    //IsLoading = true;
                    SetState(TaskState.Loading);
                    var directoryName = Path.GetDirectoryName(LocalPath);

                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName ?? string.Empty);
                    }

                    if (File.Exists(LocalPath) && LocalFileSHA1(LocalPath) == HttpHash)
                    {
                        //StartedEvent?.Invoke(this);
                        TotalBytes = new FileInfo(LocalPath).Length;
                        LoadedBytes = TotalBytes;
                        //IsLoading = false;
                        //Completed = true;
                        //CompletedEvent?.Invoke(this);
                        SetState(TaskState.Finished);
                        return;
                    }

                    var request = (HttpWebRequest) WebRequest.Create(HttpPath);
                    if (LoadedBytes > 0)
                    {
                        request.AddRange(LoadedBytes);
                    }

                    request.ServicePoint.ConnectionLimit = int.MaxValue;
                    var response = request.GetResponse();

                    if (TotalBytes == 0)
                    {
                        TotalBytes = response.ContentLength;
                        //StartedEvent?.Invoke(this);
                    }

                    using (var writer = new FileStream(LocalPath, FileMode.OpenOrCreate))
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            while (State == TaskState.Loading)
                            {
                                var data = new byte[READ_BYTES];
                                var readNumber = stream.Read(data, 0, data.Length);
                                if (readNumber > 0)
                                {
                                    writer.Write(data, 0, readNumber);
                                    LoadedBytes += readNumber;
                                    //ProgressChangedEvent?.Invoke(this);
                                    SetState(TaskState.Loading, Progress);
                                }

                                if (LoadedBytes != TotalBytes)
                                {
                                    continue;
                                }

                                //IsLoading = false;
                                //Completed = true;
                                //CompletedEvent?.Invoke(this);
                                SetState(TaskState.Finished);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    SetState(TaskState.Broken, exception);
                }
            }).Start();
        }

        private void SetState(TaskState state, object message = null)
        {
            State = state;
            StateChangedEvent?.Invoke(State, message);
        }

        public void Pause()
        {
            //if (!IsLoading || Completed)
            //{
            //    return;
            //}
            if (State != TaskState.Loading)
            {
                return;
            }

            SetState(TaskState.Paused);
            //PausedEvent?.Invoke(this);
        }

        public void Delete()
        {
            Pause();
            Name = "";
            HttpPath = "";
            LocalPath = "";
            LoadedBytes = 0;
            TotalBytes = 0;
        }

        /// <summary>
        /// 判断远程文件是否存在
        /// </summary>
        /// <returns>存在-true，不存在-false</returns>
        private bool HttpFileExist(string httpPath)
        {
            WebResponse response = null;
            var result = false;
            try
            {
                response = WebRequest.Create(httpPath).GetResponse();
                result = true;
            }
            catch (Exception exception)
            {
                SetState(TaskState.Broken, exception);
            }
            finally
            {
                response?.Close();
            }

            return result;
        }

        /// <summary>
        /// 计算指定文件的SHA1值
        /// </summary>
        /// <param name="fileName">指定文件的完全限定名称</param>
        /// <returns>返回值的字符串形式</returns>
        public string LocalFileSHA1(string fileName)
        {
            var hashSHA1 = string.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            if (!File.Exists(fileName))
            {
                return hashSHA1;
            }

            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                //计算文件的SHA1值
                var calculator = SHA1.Create();
                var buffers = calculator.ComputeHash(fileStream);
                calculator.Clear();
                //将字节数组转换成十六进制的字符串形式
                var stringBuilder = new StringBuilder();
                foreach (var buffer in buffers)
                {
                    stringBuilder.Append(buffer.ToString("x2"));
                }

                hashSHA1 = stringBuilder.ToString();
            }

            return hashSHA1;
        }
    }

    //public delegate void DownloadStartedHandler(object sender);

    //public delegate void DownloadPausedHandler(object sender);

    //public delegate void DownloadProgressChangedHandler(object sender);

    //public delegate void DownloadErrorBrokenHandler(object sender, Exception ex);

    //public delegate void DownloadCompletedHandler(object sender);

    public delegate void DownloadStateChangedHandler(TaskState state, object message = null);
}