using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace WpfApp2
{
    internal class HardwareInfoService
    {
        // Declare a delegate for the logging event
        public delegate void LogEventHandler(string message);
        // Declare the event using the delegate
        public event LogEventHandler? LogEvent;

        private static ManagementObjectSearcher CreateSearcher(string wmiQuery)
        {
            return new ManagementObjectSearcher(wmiQuery);
        }

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
                LogEvent?.Invoke($"An error occurred in GetCpuName: {ex.Message}");
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
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                totalRamBytes += Convert.ToInt64(obj["Capacity"]);
                manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
            }
            double totalRamGB = totalRamBytes / (1024.0 * 1024.0 * 1024.0);
            return $"{manufacturer} {totalRamGB:F2} GB";
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
                manufacturer = ManufacturerName(manufacturer);

                motherboardInfo = $"{manufacturer} {product}";
                break;
            }
            return motherboardInfo;
        }

        private static string ManufacturerName(string verboseName)
        {
            Dictionary<string, string> manufacturerMappings = new()
            {
                { "ASUSTeK COMPUTER INC.", "Asus" },
                { "Micro-Star International", "MSI" },
                { "Acer Inc.", "Acer" },
                { "Hewlett-Packard", "HP" },
                { "Gigabyte Technology Co., Ltd.", "Gigabyte" },
                { "Dell Inc.", "Dell" },
                { "Sony Corporation", "Sony" },
                { "Samsung Electronics", "Samsung" },
                { "LG Electronics", "LG" },
                { "Intel Corporation", "Intel" },
                { "American Megatrends Inc.", "AMI" },
                { "Biostar Group", "Biostar" },
                { "EVGA Corp.", "EVGA" },
                { "Panasonic Corporation", "Panasonic" },
            };

            return manufacturerMappings.TryGetValue(verboseName, out var simplifiedName) ? simplifiedName : verboseName;
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
            ManagementObjectSearcher searcher = CreateSearcher("SELECT * FROM Win32_DiskDrive WHERE MediaType='Solid State Drive'");
            List<string> sddInfos = new();

            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                object? model = null;
                try
                {
                    model = obj.Properties["Model"]?.Value;
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke($"Error accessing Model property: {ex.Message}");
                }
                if (model != null)
                {
                    sddInfos.Add(model.ToString());
                }
            }
            return string.Join(", ", sddInfos);
        }

    }
}
