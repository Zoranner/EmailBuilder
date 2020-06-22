using Microsoft.Win32;

namespace Zoranner.Upgrader
{
    public enum StartupType
    {
        Install,
        Upgrade,
        Uninstall
    }

    public class Function
    {
        //public const string ASSEMBLY_URL = "assembly.txt";
        public const string SERVER_URL = "http://116.62.68.138:111/Compile/";
        private static Function _Instance;

        /// <summary>
        /// 安装目录
        /// </summary>
        private string _InstallPath;

        /// <summary>
        /// </summary>
        private RegistryKey _MainRegistry;

        public StartupType StartupType;

        private Function()
        {
        }

        public static Function Instance => _Instance ?? (_Instance = new Function());

        public RegistryKey MainRegistry =>
            _MainRegistry ?? (_MainRegistry =
                Registry.CurrentUser.OpenSubKey("Software\\KimoTech\\Hephaistos", true) ??
                Registry.CurrentUser.CreateSubKey("Software\\KimoTech\\Hephaistos"));

        public string InstallPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_InstallPath))
                {
                    return _InstallPath.Replace("/", @"\");
                }

                if (MainRegistry.GetValue("LocalPath") != null)
                {
                    _InstallPath = MainRegistry.GetValue("LocalPath").ToString();
                }
                else
                {
                    var localMachine = Registry.LocalMachine;
                    var currentVersion = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion", false);
                    var programFilesDir = currentVersion.GetValue("ProgramFilesDir").ToString();
                    _InstallPath = $@"{programFilesDir}\KimoTech\Hephaistos";
                }

                return _InstallPath.Replace("/", @"\");
            }
            set
            {
                MainRegistry.SetValue("LocalPath", value);
                _InstallPath = value;
            }
        }
    }
}