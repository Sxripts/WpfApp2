using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using System;
using System.Management;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulatePCHealthInfo();
            StartOpacityAnimation();

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

        private void PopulatePCHealthInfo()
        {
            // Fetch and display PC health information
            cpuNameTextBlock.Text = GetCpuName();
            gpuNameTextBlock.Text = GetGpuName();
            ramNameTextBlock.Text = GetTotalRamSize();
            motherboardNameTextBlock.Text = GetMotherboardInfo();
            hddStorageTextBlock.Text = GetHddInfo();
            sddStorageTextBlock.Text = GetSddInfo();
        }

        private string GetCpuName()
        {
            string cpuName = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    cpuName = obj["Name"].ToString();
                    break; // Retrieve only the first CPU
                }
            }
            return cpuName;
        }

        private string GetGpuName()
        {
            string gpuName = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    gpuName = obj["Caption"].ToString();
                    break; // Retrieve only the first GPU
                }
            }
            return gpuName;
        }

        private string GetTotalRamSize()
        {
            long totalRamBytes = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalRamBytes = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    break;
                }
            }
            double totalRamGB = totalRamBytes / (1024 * 1024 * 1024.0); // Convert to GB
            return $"{totalRamGB:F2} GB";
        }

        private string GetMotherboardInfo()
        {
            string motherboardInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    motherboardInfo = obj["Product"].ToString();
                    break; // Retrieve only the first motherboard
                }
            }
            return motherboardInfo;
        }

        private string GetHddInfo()
        {
            string hddInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    double sizeBytes = Convert.ToDouble(obj["Size"]);
                    double sizeGB = sizeBytes / (1024 * 1024 * 1024.0); // Convert to GB
                    hddInfo = $"{sizeGB:F2} GB";
                    break; // Retrieve only the first HDD
                }
            }
            return hddInfo;
        }


        private string GetSddInfo()
        {
            string sddInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE MediaType='SSD'"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    sddInfo = $"{obj["Size"]} bytes";
                    break; // Retrieve only the first SSD
                }
            }
            return sddInfo;
        }
        private bool IsWindowsDefenderEnabled()
        {
            try
            {
                using (var defenderKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection"))
                {
                    if (defenderKey != null)
                    {
                        var value = defenderKey.GetValue("DisableRealtimeMonitoring");
                        if (value != null && (int)value == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
            }
            return false;
        }
    }
}
