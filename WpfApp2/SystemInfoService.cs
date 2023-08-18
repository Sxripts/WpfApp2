﻿using System;
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
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_BaseBoard");
            string motherboardInfo = "Unknown";
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                string manufacturer = obj.Properties["Manufacturer"]?.Value?.ToString() ?? "Unknown";
                string product = obj.Properties["Product"]?.Value?.ToString() ?? "Unknown";

                // Simplify known verbose manufacturer names
                if (manufacturer.Equals("ASUSTeK COMPUTER INC.", StringComparison.OrdinalIgnoreCase))
                {
                    manufacturer = "Asus";
                }

                motherboardInfo = $"{manufacturer} {product}";
                break;
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
                // Print all properties for diagnostic purposes
                foreach (PropertyData prop in obj.Properties)
                {
                    try
                    {
                        Console.WriteLine($"{prop.Name}: {prop.Value}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing property {prop.Name}: {ex.Message}");
                    }
                }

                // Just for testing, let's fetch only the Model property
                object? model = null;
                try
                {
                    model = obj.Properties["Model"]?.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing Model property: {ex.Message}");
                }

                if (model != null)
                {
                    sddInfos.Add(model.ToString());
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