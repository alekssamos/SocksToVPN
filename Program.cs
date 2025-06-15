using SocksToVpn;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=============================================");
        Console.WriteLine("  tun2socks Proxy Setup Utility");
        Console.WriteLine("=============================================");
        
        // Get operating system information
        OsInfo.OsType osType = OsInfo.GetOperatingSystem();
        OsInfo.ArchitectureType archType = OsInfo.GetArchitecture();
        
        Console.WriteLine($"Detected OS: {osType}");
        Console.WriteLine($"Detected Architecture: {OsInfo.GetCpuArchitectureDescription()}");
        Console.WriteLine();
        
        // Step 1: Get proxy settings from environment variables or from user
        ProxySettings? proxySettings = ProxySettings.GetFromEnvironmentVariables();
        
        // Check if settings were successfully retrieved from environment variables
        if (proxySettings == null)
        {
            Console.WriteLine("Failed to get proxy settings from environment variables. Falling back to user input...");
            proxySettings = ProxySettings.GetFromUserInput();
        }
        
        try
        {
            // Step 2: Download required files
            Downloader downloader = new Downloader();
            
            // For Windows, download Wintun driver
            if (osType == OsInfo.OsType.Windows)
            {
                Console.WriteLine("Downloading Wintun driver for Windows...");
                await downloader.DownloadAndExtractWintunAsync();
            }
            
            // Download tun2socks for the current OS
            Console.WriteLine("Downloading tun2socks...");
            string tun2SocksPath = await downloader.DownloadTun2SocksAsync();
            
            // Step 3: Run tun2socks
            Console.WriteLine("Setting up tun2socks...");
            
            var tun2SocksRunner = new Tun2SocksRunner(tun2SocksPath, proxySettings);
            await tun2SocksRunner.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
