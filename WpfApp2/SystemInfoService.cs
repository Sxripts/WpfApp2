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
                model = obj["PartNumber"].ToString();
                break;
            }
            double totalRamGB = totalRamBytes / (1024 * 1024 * 1024.0);
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
                double sizeBytes = Convert.ToDouble(obj["Size"]);
                double sizeGB = sizeBytes / (1024 * 1024 * 1024.0);
                string manufacturer = obj["Manufacturer"].ToString();
                string model = obj["Model"].ToString();
                hddInfo = $"{manufacturer} {model} - {sizeGB:F2} GB";
                break;
            }
            return hddInfo;
        }

        public static string? GetSddInfo()
        {
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_DiskDrive WHERE MediaType='Solid state drive'");
            List<string> sddInfos = new();
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                double sizeBytes = Convert.ToDouble(obj["Size"]);
                double sizeGB = sizeBytes / (1024 * 1024 * 1024.0);
                string manufacturer = obj["Manufacturer"].ToString();
                string model = obj["Model"].ToString();
                sddInfos.Add($"{manufacturer} {model} - {sizeGB:F2} GB");
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