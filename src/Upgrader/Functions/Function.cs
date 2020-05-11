using System;
using System.Text;
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
        private Function() { }
        private static Function _Instance;
        public static Function Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Function();
                }
                return _Instance;
            }
        }

        public StartupType StartupType;

        /// <summary>
        /// 
        /// </summary>
        private RegistryKey _MainRegistry;
        public RegistryKey MainRegistry
        {
            get
            {
                if(_MainRegistry == null)
                {
                    _MainRegistry = Registry.CurrentUser.OpenSubKey("Software\\KimoTech\\Hephaistos", true);
                    if (_MainRegistry == null)
                    {
                        _MainRegistry = Registry.CurrentUser.CreateSubKey("Software\\KimoTech\\Hephaistos");
                    }
                }
                return _MainRegistry;
            }
        }

        /// <summary>
        /// 安装目录
        /// </summary>
        private string _InstallPath;
        public string InstallPath
        {
            get
            {
                if (string.IsNullOrEmpty(_InstallPath))
                {
                    if (MainRegistry.GetValue("LocalPath") != null)
                    {
                        _InstallPath = MainRegistry.GetValue("LocalPath").ToString();
                    }
                    else
                    {
                        RegistryKey localMachine = Registry.LocalMachine;
                        RegistryKey currentVersion = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion", false);
                        string programFilesDir = currentVersion.GetValue("ProgramFilesDir").ToString();
                        _InstallPath = string.Format(@"{0}\KimoTech\Hephaistos", programFilesDir);
                    }
                }
                return _InstallPath.Replace("/", @"\");
            }
            set
            {
                MainRegistry.SetValue("LocalPath", value);
                _InstallPath = value;
            }
        }

        //public const string ASSEMBLY_URL = "assembly.txt";
        public const string SERVER_URL = "http://116.62.68.138:111/Compile/";
    }
}
