using System;
using Microsoft.Win32;
using System.Management;
using System.Linq;
using System.Collections.Generic;

namespace WpfApp2
{
    class SystemInfoService
    {
        // Declare a delegate for the logging event
        public delegate void LogEventHandler(string message);
        // Declare the event using the delegate
        public event LogEventHandler? LogEvent;

        public string? GetCpuName()
        {
            try
            {
                string cpuName = "Unknown";
                using (ManagementObjectSearcher searcher = new("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                    {
                        cpuName = obj["Name"].ToString();
                        break; // Retrieve only the first CPU
                    }
                }
                return cpuName;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"An error occurred in SomeMethod: {ex.Message}");
                return "Error fetching CPU name";
            }
        }

        public static string? GetGpuName()
        {
            string gpuName = "Unknown";
            using (ManagementObjectSearcher searcher = new("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    gpuName = obj["Caption"].ToString();
                    break; // Retrieve only the first GPU
                }
            }
            return gpuName;
        }

        public string? GetTotalRamSize()
        {
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_PhysicalMemory");
            long totalRamBytes = 0;
            string manufacturer = "Unknown";
            string model = "Unknown";
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                totalRamBytes = Convert.ToInt64(obj["Capacity"]);
                manufacturer = obj["Manufacturer"].ToString();
                model = obj["PartNumber"].ToString(); // Keeping PartNumber as model for now
                break;
            }
            double totalRamGB = totalRamBytes / (1024.0 * 1024.0 * 1024.0);
            return $"{manufacturer} {model} - {totalRamGB:F2} GB";
        }

        public static string? GetMotherboardInfo()
        {
            string motherboardInfo = "Unknown";
            using (ManagementObjectSearcher searcher = new("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    motherboardInfo = obj["Product"].ToString();
                    break; // Retrieve only the first motherboard
                }
            }
            return motherboardInfo;
        }

        public static string? GetHddInfo()
        {
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'");
            string hddInfo = "Unknown";
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                // Print all properties for diagnostic purposes
                foreach (PropertyData prop in obj.Properties)
                {
                    Console.WriteLine($"{prop.Name}: {prop.Value}");
                }

                // Just for testing, let's fetch only the Model property
                object? model = obj.Properties["Model"]?.Value;
                if (model != null)
                {
                    hddInfo = model.ToString();
                    break;
                }
            }
            return hddInfo;
        }

        public static string? GetSddInfo()
        {
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_DiskDrive");
            List<string> sddInfos = new();
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                bool hasMediaType = obj.Properties["MediaType"] != null;
                bool hasSpindleSpeed = obj.Properties["SpindleSpeed"] != null;
                bool hasSize = obj.Properties["Size"] != null;
                bool hasManufacturer = obj.Properties["Manufacturer"] != null;
                bool hasModel = obj.Properties["Model"] != null;

                object? mediaType = hasMediaType ? obj.Properties["MediaType"].Value : null;
                object? spindleSpeed = hasSpindleSpeed ? obj.Properties["SpindleSpeed"].Value : null;
                object? size = hasSize ? obj.Properties["Size"].Value : null;
                object? manufacturer = hasManufacturer ? obj.Properties["Manufacturer"].Value : null;
                object? model = hasModel ? obj.Properties["Model"].Value : null;

                if (mediaType != null && mediaType.ToString() == "Solid state drive" && size != null)
                {
                    double sizeBytes = Convert.ToDouble(size);
                    double sizeGB = sizeBytes / (1024.0 * 1024.0 * 1024.0);
                    sddInfos.Add($"{manufacturer ?? "Unknown"} {model ?? "Unknown"} - {sizeGB:F2} GB");
                }
                else if (spindleSpeed != null && Convert.ToInt32(spindleSpeed) == 0 && size != null)
                {
                    double sizeBytes = Convert.ToDouble(size);
                    double sizeGB = sizeBytes / (1024.0 * 1024.0 * 1024.0);
                    sddInfos.Add($"{manufacturer ?? "Unknown"} {model ?? "Unknown"} - {sizeGB:F2} GB");
                }
            }
            return string.Join(", ", sddInfos);
        }



        public static bool IsWindowsDefenderEnabled()
        {
            const string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Real-Time Protection";
            const string valueName = "DisableRealtimeMonitoring";

            int value = (int)Registry.GetValue(keyPath, valueName, -1);
            return value != 1;
        }
    }
}