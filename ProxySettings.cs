namespace SocksToVpn
{
    public class ProxySettings
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public ProxySettings(string ipAddress, int port, string? username = null, string? password = null)
        {
            IpAddress = ipAddress;
            Port = port;
            Username = username;
            Password = password;
        }

        public static ProxySettings GetFromUserInput()
        {
            Console.WriteLine("Please enter proxy settings (or use format domain:port:username:password):");
            
            string input = Console.ReadLine() ?? string.Empty;
            
            // Check if input follows the format domain:port:username:password
            if (input.Contains(':'))
            {
                string[] parts = input.Split(':');
                
                // Parse domain:port:username:password format
                if (parts.Length >= 2)
                {
                    string ipAddress = parts[0];
                    
                    // Try to parse port
                    if (!int.TryParse(parts[1], out int port))
                    {
                        port = 1080; // Default SOCKS port
                        Console.WriteLine($"Invalid port number. Using default: {port}");
                    }
                    
                    // Handle username and password if provided
                    string? username = null;
                    string? password = null;
                    
                    if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                    {
                        username = parts[2];
                    }
                    
                    if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3]))
                    {
                        password = parts[3];
                    }
                    
                    return new ProxySettings(ipAddress, port, username, password);
                }
            }
            
            // If we get here, use the interactive input method
            Console.Write("Proxy IP Address: ");
            string interactiveIpAddress = Console.ReadLine() ?? string.Empty;
            
            Console.Write("Proxy Port: ");
            int interactivePort = 1080; // Default value
            if (!int.TryParse(Console.ReadLine(), out interactivePort))
            {
                Console.WriteLine($"Invalid port number. Using default: {interactivePort}");
            }
            
            Console.Write("Proxy Username (leave empty if not required): ");
            string? interactiveUsername = Console.ReadLine();
            interactiveUsername = string.IsNullOrWhiteSpace(interactiveUsername) ? null : interactiveUsername;
            
            string? interactivePassword = null;
            if (interactiveUsername != null)
            {
                Console.Write("Proxy Password: ");
                interactivePassword = Console.ReadLine();
            }
            
            return new ProxySettings(interactiveIpAddress, interactivePort, interactiveUsername, interactivePassword);
        }
    }
}
