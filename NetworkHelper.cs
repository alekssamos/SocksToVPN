using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SocksToVpn
{
    public static class NetworkHelper
    {
        public static string GetPrimaryInterfaceName()
        {
            string interfaceName = string.Empty;
            NetworkInterface? bestInterface = null;
            int highestSpeed = -1;
            bool foundWifi = false;

            try
            {
                // Used for finding the best route to a public IP
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
                
                // Connect to a public DNS (doesn't actually connect, just sets up routing info)
                socket.Connect("8.8.8.8", 53);
                var localEndPoint = socket.LocalEndPoint as IPEndPoint;
                var localIp = localEndPoint?.Address.ToString();
                
                Console.WriteLine($"Local IP connecting to internet appears to be: {localIp}");
                
                // Get all network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                // First, try to find exact match for the local IP that's routing to the internet
                if (!string.IsNullOrEmpty(localIp))
                {
                    foreach (NetworkInterface adapter in interfaces)
                    {
                        if (adapter.OperationalStatus == OperationalStatus.Up &&
                            adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        {
                            IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                            
                            foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork && 
                                    ip.Address.ToString() == localIp)
                                {
                                    // Found the exact interface being used!
                                    Console.WriteLine($"Found exact interface match: {adapter.Name} ({adapter.Description}) with IP {localIp}");
                                    return adapter.Name;
                                }
                            }
                        }
                    }
                }
                
                // If we get here, we couldn't find an exact match, so try common interface types
                
                // First preference: Find wireless LAN adapter that's up
                foreach (NetworkInterface adapter in interfaces)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        // Check for wireless/wifi interfaces - they're typically used for internet
                        if (adapter.Description.ToLower().Contains("wireless") || 
                             adapter.Description.ToLower().Contains("wifi") || 
                             adapter.Description.ToLower().Contains("wi-fi") ||
                             adapter.Name.ToLower().Contains("wireless") || 
                             adapter.Name.ToLower().Contains("wifi") || 
                             adapter.Name.ToLower().Contains("wi-fi"))
                        {
                            IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                            if (adapterProperties.UnicastAddresses.Any(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork) &&
                                adapterProperties.GatewayAddresses.Count > 0)
                            {
                                Console.WriteLine($"Found wireless interface: {adapter.Name} ({adapter.Description})");
                                foundWifi = true;
                                
                                // If adapter is WiFi and has higher speed, use it
                                if (adapter.Speed > highestSpeed)
                                {
                                    highestSpeed = (int)adapter.Speed;
                                    bestInterface = adapter;
                                }
                            }
                        }
                    }
                }
                
                // If we found WiFi interface(s), use the fastest one
                if (foundWifi && bestInterface != null)
                {
                    interfaceName = bestInterface.Name;
                    Console.WriteLine($"Using wireless interface with highest speed: {interfaceName} ({bestInterface.Description})");
                    return interfaceName;
                }
                
                // Second preference: Ethernet/LAN connections that are up with gateway and IPv4
                highestSpeed = -1;
                bestInterface = null;
                
                foreach (NetworkInterface adapter in interfaces)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up &&
                        adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        adapter.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                        
                        if (adapterProperties.GatewayAddresses.Count > 0 &&
                            adapterProperties.UnicastAddresses.Any(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork))
                        {
                            // Another good candidate - find highest speed
                            if (adapter.Speed > highestSpeed)
                            {
                                highestSpeed = (int)adapter.Speed;
                                bestInterface = adapter;
                            }
                        }
                    }
                }
                
                // Use the fastest interface with IPv4 and gateway
                if (bestInterface != null)
                {
                    interfaceName = bestInterface.Name;
                    Console.WriteLine($"Using interface with highest speed and gateway: {interfaceName} ({bestInterface.Description})");
                    return interfaceName;
                }
                
                // Last resort: Try any interface that's up with IPv4
                foreach (NetworkInterface adapter in interfaces)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up &&
                        adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                        
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                interfaceName = adapter.Name;
                                Console.WriteLine($"Found usable interface: {interfaceName} ({adapter.Description})");
                                return interfaceName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting network interface: {ex.Message}");
            }

            // If we couldn't find anything, return a default based on OS
            if (string.IsNullOrEmpty(interfaceName))
            {
                interfaceName = OsInfo.GetOperatingSystem() switch
                {
                    OsInfo.OsType.Windows => "Ethernet",
                    OsInfo.OsType.MacOS => "en0",
                    OsInfo.OsType.Linux => "eth0",
                    _ => "eth0"
                };
                
                Console.WriteLine($"Could not detect network interface, using default: {interfaceName}");
            }
            
            return interfaceName;
        }

        public static string GetPrimaryGateway()
        {
            try
            {
                // First find the primary interface
                string primaryInterfaceName = GetPrimaryInterfaceName();
                
                // Get all network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                // Look for the interface we identified as primary
                foreach (NetworkInterface adapter in interfaces)
                {
                    if (adapter.Name == primaryInterfaceName)
                    {
                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                        
                        if (adapterProperties.GatewayAddresses.Count > 0)
                        {
                            foreach (GatewayIPAddressInformation gateway in adapterProperties.GatewayAddresses)
                            {
                                if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    string gatewayAddress = gateway.Address.ToString();
                                    Console.WriteLine($"Found gateway: {gatewayAddress} on interface {adapter.Name}");
                                    return gatewayAddress;
                                }
                            }
                        }
                        
                        // If we found the interface but it has no gateway, break to fallback
                        break;
                    }
                }
                
                // Fallback: search all interfaces for any gateway
                foreach (NetworkInterface adapter in interfaces)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up &&
                        adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                        
                        if (adapterProperties.GatewayAddresses.Count > 0)
                        {
                            foreach (GatewayIPAddressInformation gateway in adapterProperties.GatewayAddresses)
                            {
                                if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    string gatewayAddress = gateway.Address.ToString();
                                    Console.WriteLine($"Found gateway: {gatewayAddress} on interface {adapter.Name} (fallback)");
                                    return gatewayAddress;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting gateway: {ex.Message}");
            }

            // Default gateway if we couldn't detect one
            string defaultGateway = "192.168.1.1";
            Console.WriteLine($"Could not detect gateway, using default: {defaultGateway}");
            return defaultGateway;
        }
    }
}
