using System;
using System.Collections.Generic;
using System.Text;

namespace SorteFederation.Shared.Helpers;
 
public class AppLiceseService
{
    public static string Get()
    {
        string uuid = "unknown";
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"cat /sys/class/dmi/id/product_uuid\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    using (var reader = process.StandardOutput)
                    {
                        uuid = reader.ReadToEnd().Trim();
                    }
                }
            }

            Console.WriteLine($"VM UUID: {uuid}");
            return uuid;
        }
        catch
        {
            return uuid;
        }
    }
}