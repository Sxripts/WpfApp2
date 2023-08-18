using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {

        private readonly SystemInfoService _systemInfoService = new();
        private readonly HardwareInfoService _HardwareInfoService = new();

        public MainWindow()
        {
            InitializeComponent();
            cpuNameTextBlock.Text = _HardwareInfoService.GetCpuName();
            gpuNameTextBlock.Text = _HardwareInfoService.GetGpuName();
            ramNameTextBlock.Text = _HardwareInfoService.GetTotalRamSize();
            var storageInfos = _HardwareInfoService.GetAllStorageInfo();
            if (storageInfos.Count > 0)
            {
                hddsddStorageTextBlock.Text = storageInfos[0]; // Display the primary storage by default
                foreach (var info in storageInfos)
                {
                    storageComboBox.Items.Add(info);
                }
            }
            motherboardNameTextBlock.Text = _HardwareInfoService.GetMotherboardInfo();
            StartOpacityAnimation();

            _systemInfoService.LogEvent += AddLog;
            _HardwareInfoService.LogEvent += AddLog;

            // Get system information
            string systemName = Environment.MachineName;
            string userName = Environment.UserName;
            string osName = Environment.OSVersion.VersionString;
            string osVersion = Environment.OSVersion.Version.ToString();
            string resolution = SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight;
            string hertz = GetPrimaryScreenRefreshRate() + " Hz";

            systemNameTextBlock.Text = systemName;
            userTextBlock.Text = userName;
            osTextBlock.Text = osName;
            osVerTextBlock.Text = osVersion;
            resolutionTextBlock.Text = resolution;
            hertzTextBlock.Text = hertz;

            bool isDefenderEnabled = SystemInfoService.IsWindowsDefenderEnabled();
            windowsDefenderTextBlock.Text = isDefenderEnabled ? "Enabled" : "Disabled";
        }

        private void StartOpacityAnimation()
        {
            DoubleAnimation opacityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = true,  // Enables reversing the animation
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTargetName(opacityAnimation, "iconHome");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new();
            storyboard.Children.Add(opacityAnimation);

            iconHome.BeginStoryboard(storyboard);
        }

        // Custom method to get primary screen refresh rate
        private int GetPrimaryScreenRefreshRate()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null && source.CompositionTarget != null)
            {
                return (int)(source.CompositionTarget.TransformToDevice.M22 * 60);
            }
            return 60;
        }

        public string LogMessages
        {
            get { return LoggingConsole.Text; }
            set { LoggingConsole.Text = value; }
        }

        public void AddLog(string message)
        {
            string currentTime = DateTime.Now.ToString("HH:mm:ss");
            LoggingConsole.Text += $"{currentTime} {message}\n";
        }
    }
}
