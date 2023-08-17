using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {

        private readonly SystemInfoService _systemInfoService = new SystemInfoService();

        public MainWindow()
        {
            InitializeComponent();
            cpuNameTextBlock.Text = _systemInfoService.GetCpuName();
            gpuNameTextBlock.Text = _systemInfoService.GetGpuName();
            ramNameTextBlock.Text = _systemInfoService.GetTotalRamSize();
            motherboardNameTextBlock.Text = _systemInfoService.GetMotherboardInfo();
            hddStorageTextBlock.Text = _systemInfoService.GetHddInfo();
            sddStorageTextBlock.Text = _systemInfoService.GetSddInfo();
            StartOpacityAnimation();

            _systemInfoService.LogEvent += AddLog;

            // Get system information
            string systemName = Environment.MachineName;
            string userName = Environment.UserName;
            string osName = Environment.OSVersion.VersionString;
            string osVersion = Environment.OSVersion.Version.ToString();
            string resolution = SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight;
            string hertz = GetPrimaryScreenRefreshRate() + " Hz"; // Use custom method

            // Update TextBlocks with system information
            systemNameTextBlock.Text = systemName;
            userTextBlock.Text = userName;
            osTextBlock.Text = osName;
            osVerTextBlock.Text = osVersion;
            resolutionTextBlock.Text = resolution;
            hertzTextBlock.Text = hertz;

            bool isDefenderEnabled = _systemInfoService.IsWindowsDefenderEnabled();
            windowsDefenderTextBlock.Text = isDefenderEnabled ? "Enabled" : "Disabled";
        }

        private void StartOpacityAnimation()
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = true,  // Enables reversing the animation
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTargetName(opacityAnimation, "iconHome");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
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
            return 60; // Default value
        }

        // Property to hold log messages
        public string LogMessages
        {
            get { return LogTextBlock.Text; }
            set { LogTextBlock.Text = value; }
        }

        // Sample method to add a log message
        public void AddLog(string message)
        {
            LogMessages += $"{DateTime.Now}: {message}\n";
        }
    }
}
