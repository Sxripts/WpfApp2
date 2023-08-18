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

        public string? GetGpuName()
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



        public string? GetMotherboardInfo()
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

        private string ManufacturerName(string verboseName)
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



        public List<string> GetAllStorageInfo()
        {
            List<string> storageInfos = new List<string>();
            string primaryDiskName = "";

            // Fetch the primary disk (where Windows is installed)
            ManagementObjectSearcher osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject osObj in osSearcher.Get().Cast<ManagementObject>())
            {
                string primaryDriveLetter = osObj["SystemDrive"].ToString();
                ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_DiskDrive WHERE DeviceID IN (ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{primaryDriveLetter}'}})");
                foreach (ManagementObject driveObj in driveSearcher.Get().Cast<ManagementObject>())
                {
                    primaryDiskName = driveObj["Model"].ToString();
                }
            }

            // Fetch HDD and SSD Info
            ManagementObjectSearcher storageSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media' OR MediaType='Solid State Drive'");
            foreach (ManagementObject obj in storageSearcher.Get().Cast<ManagementObject>())
            {
                object? model = obj.Properties["Model"]?.Value;
                if (model != null)
                {
                    if (model.ToString() == primaryDiskName)
                    {
                        storageInfos.Insert(0, model.ToString()); // Add primary disk to the beginning of the list
                    }
                    else
                    {
                        storageInfos.Add(model.ToString());
                    }
                }
            }

            return storageInfos;
        }

    }
}
