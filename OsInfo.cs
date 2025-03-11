using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SocksToVpn
{
    public static class OsInfo
    {
        public enum OsType
        {
            Windows,
            MacOS,
            Linux,
            LinuxMusl,
            Unknown
        }

        public enum ArchitectureType
        {
            X86,
            X64,
            X64V3, // For newer CPUs with AVX2 support
            Arm,
            Arm64,
            Riscv64,
            Mips,
            MipsLE,
            Mips64,
            Mips64LE,
            PPC64,
            PPC64LE,
            S390X,
            Unknown
        }

        public static OsType GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OsType.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OsType.MacOS;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Check if this is a musl-based Linux
                if (IsMuslBased())
                    return OsType.LinuxMusl;
                else
                    return OsType.Linux;
            }
            else
                return OsType.Unknown;
        }

        public static ArchitectureType GetArchitecture()
        {
            // Get the basic architecture first
            Architecture arch = RuntimeInformation.ProcessArchitecture;
            
            ArchitectureType baseArch = arch switch
            {
                Architecture.X86 => ArchitectureType.X86,
                Architecture.X64 => ArchitectureType.X64,
                Architecture.Arm => ArchitectureType.Arm,
                Architecture.Arm64 => ArchitectureType.Arm64,
                _ => ArchitectureType.Unknown
            };

            // On Linux, check for special architectures not directly supported by .NET's RuntimeInformation
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && baseArch == ArchitectureType.Unknown)
            {
                // Try to detect via uname on Linux
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "uname",
                        Arguments = "-m",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = System.Diagnostics.Process.Start(startInfo);
                    if (process != null)
                    {
                        string machineType = process.StandardOutput.ReadToEnd().Trim();
                        process.WaitForExit();

                        Console.WriteLine($"Detected machine type via uname: {machineType}");
                        
                        // Map the machine type to our architecture enum
                        return machineType.ToLowerInvariant() switch
                        {
                            "x86_64" => CheckForAVX2() ? ArchitectureType.X64V3 : ArchitectureType.X64,
                            "i386" or "i686" => ArchitectureType.X86,
                            "aarch64" => ArchitectureType.Arm64,
                            "armv7l" or "armv6l" or "arm" => ArchitectureType.Arm,
                            "mips" => ArchitectureType.Mips,
                            "mipsel" => ArchitectureType.MipsLE,
                            "mips64" => ArchitectureType.Mips64,
                            "mips64el" => ArchitectureType.Mips64LE,
                            "ppc64" => ArchitectureType.PPC64,
                            "ppc64le" => ArchitectureType.PPC64LE,
                            "s390x" => ArchitectureType.S390X,
                            "riscv64" => ArchitectureType.Riscv64,
                            _ => ArchitectureType.Unknown
                        };
                    }
                }
                catch
                {
                    // Fall back to the base architecture if uname fails
                    Console.WriteLine("Failed to detect detailed architecture via uname");
                }
            }
            
            // For x64 on any platform, check if it's a newer CPU with AVX2 support
            if (baseArch == ArchitectureType.X64 && CheckForAVX2())
            {
                Console.WriteLine("Detected modern CPU with AVX2 support, using X64V3");
                return ArchitectureType.X64V3;
            }

            return baseArch;
        }
        
        private static bool CheckForAVX2()
        {
            try
            {
                // On Windows, we can check CPU features via Registry
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Simple check for newer processor - this is a crude estimation
                    // A better solution would use proper CPU feature detection
                    return Environment.ProcessorCount > 2 && 
                           Environment.ProcessorCount * 2 >= Environment.SystemPageSize; // Just a rough heuristic
                }
                
                // On Linux, check through /proc/cpuinfo
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists("/proc/cpuinfo"))
                {
                    string cpuInfo = File.ReadAllText("/proc/cpuinfo");
                    return cpuInfo.Contains("avx2", StringComparison.OrdinalIgnoreCase);
                }
                
                // On macOS, we could use sysctl, but for simplicity we'll just assume newer Macs have AVX2
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Again, this is a crude estimation without proper feature detection
                    return true;
                }
            }
            catch
            {
                // If feature detection fails, assume no AVX2
            }
            
            return false;
        }

        private static bool IsMuslBased()
        {
            try
            {
                // Alpine Linux and some other distributions use musl
                if (File.Exists("/etc/os-release"))
                {
                    string osRelease = File.ReadAllText("/etc/os-release");
                    if (osRelease.Contains("Alpine", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                // Check if we have ldd which can tell us if it's musl
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ldd",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process != null)
                {
                    string output = process.StandardError.ReadToEnd(); // musl's ldd outputs to stderr
                    process.WaitForExit();
                    
                    if (output.Contains("musl", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                // Another check could be to look for the musl library
                if (File.Exists("/lib/ld-musl-x86_64.so.1") || 
                    File.Exists("/lib/ld-musl-aarch64.so.1"))
                    return true;
            }
            catch
            {
                // If we encounter any exceptions during detection, assume it's not musl
            }
            
            return false;
        }

        public static string GetTun2SocksExecutableName()
        {
            OsType os = GetOperatingSystem();
            
            return os switch
            {
                OsType.Windows => "tun2socks.exe",
                _ => "tun2socks"
            };
        }

        public static string GetTun2SocksDownloadUrl()
        {
            OsType os = GetOperatingSystem();
            ArchitectureType arch = GetArchitecture();

            string baseUrl = "https://github.com/xjasonlyu/tun2socks/releases/latest/download/";
            
            return (os, arch) switch
            {
                // Windows builds
                (OsType.Windows, ArchitectureType.X86) => $"{baseUrl}tun2socks-windows-386.zip",
                (OsType.Windows, ArchitectureType.X64) => $"{baseUrl}tun2socks-windows-amd64.zip",
                (OsType.Windows, ArchitectureType.X64V3) => $"{baseUrl}tun2socks-windows-amd64-v3.zip",
                (OsType.Windows, ArchitectureType.Arm) => $"{baseUrl}tun2socks-windows-arm32v7.zip",
                (OsType.Windows, ArchitectureType.Arm64) => $"{baseUrl}tun2socks-windows-arm64.zip",
                
                // macOS builds
                (OsType.MacOS, ArchitectureType.X64) => $"{baseUrl}tun2socks-darwin-amd64.zip",
                (OsType.MacOS, ArchitectureType.X64V3) => $"{baseUrl}tun2socks-darwin-amd64-v3.zip",
                (OsType.MacOS, ArchitectureType.Arm64) => $"{baseUrl}tun2socks-darwin-arm64.zip",
                
                // Linux builds (standard/glibc)
                (OsType.Linux, ArchitectureType.X86) => $"{baseUrl}tun2socks-linux-386.zip",
                (OsType.Linux, ArchitectureType.X64) => $"{baseUrl}tun2socks-linux-amd64.zip",
                (OsType.Linux, ArchitectureType.X64V3) => $"{baseUrl}tun2socks-linux-amd64-v3.zip",
                (OsType.Linux, ArchitectureType.Arm64) => $"{baseUrl}tun2socks-linux-arm64.zip",
                
                // Linux ARM builds (handled separately for different versions)
                (OsType.Linux, ArchitectureType.Arm) => GetLinuxArmVersion(),
                
                // MIPS builds
                (OsType.Linux, ArchitectureType.Mips) => $"{baseUrl}tun2socks-linux-mips-hardfloat.zip",
                (OsType.Linux, ArchitectureType.MipsLE) => $"{baseUrl}tun2socks-linux-mipsle-hardfloat.zip",
                (OsType.Linux, ArchitectureType.Mips64) => $"{baseUrl}tun2socks-linux-mips64.zip",
                (OsType.Linux, ArchitectureType.Mips64LE) => $"{baseUrl}tun2socks-linux-mips64le.zip",

                // PowerPC builds
                (OsType.Linux, ArchitectureType.PPC64) => $"{baseUrl}tun2socks-linux-ppc64.zip",
                (OsType.Linux, ArchitectureType.PPC64LE) => $"{baseUrl}tun2socks-linux-ppc64le.zip",
                
                // S390X architecture (IBM System z)
                (OsType.Linux, ArchitectureType.S390X) => $"{baseUrl}tun2socks-linux-s390x.zip",
                
                // RISC-V architecture
                (OsType.Linux, ArchitectureType.Riscv64) => $"{baseUrl}tun2socks-linux-riscv64.zip",
                
                // FreeBSD builds are also available but not explicitly supported in this app yet
                
                // Default fallback - choose a reasonable default
                _ => $"{baseUrl}tun2socks-linux-amd64.zip"
            };
        }

        private static string GetLinuxArmVersion()
        {
            // Determine ARM version for Linux
            string baseUrl = "https://github.com/xjasonlyu/tun2socks/releases/latest/download/";
            
            try 
            {
                // Try to detect ARM version through CPU info
                if (File.Exists("/proc/cpuinfo"))
                {
                    string cpuInfo = File.ReadAllText("/proc/cpuinfo");
                    
                    if (cpuInfo.Contains("ARMv7") || cpuInfo.Contains("model name\t: ARMv7"))
                    {
                        return $"{baseUrl}tun2socks-linux-armv7.zip";
                    }
                    else if (cpuInfo.Contains("ARMv6") || cpuInfo.Contains("model name\t: ARMv6"))
                    {
                        return $"{baseUrl}tun2socks-linux-armv6.zip";
                    }
                    else if (cpuInfo.Contains("ARMv5") || cpuInfo.Contains("model name\t: ARMv5"))
                    {
                        return $"{baseUrl}tun2socks-linux-armv5.zip";
                    }
                }
            }
            catch
            {
                // Fall back to ARM v7 if detection fails
            }
            
            // Default to ARMv7 as it's most common
            return $"{baseUrl}tun2socks-linux-armv7.zip";
        }

        public static string GetWintunDllPath(string extractPath)
        {
            ArchitectureType arch = GetArchitecture();
            
            return arch switch
            {
                ArchitectureType.X86 => Path.Combine(extractPath, "wintun", "bin", "x86", "wintun.dll"),
                ArchitectureType.X64 => Path.Combine(extractPath, "wintun", "bin", "amd64", "wintun.dll"),
                ArchitectureType.Arm64 => Path.Combine(extractPath, "wintun", "bin", "arm64", "wintun.dll"),
                _ => Path.Combine(extractPath, "wintun", "bin", "amd64", "wintun.dll") // Default to x64 if unknown
            };
        }

        public static string GetCpuArchitectureDescription()
        {
            ArchitectureType arch = GetArchitecture();
            OsType os = GetOperatingSystem();
            
            string muslSuffix = os == OsType.LinuxMusl ? " (musl)" : "";
            
            return arch switch
            {
                ArchitectureType.X86 => $"32-bit x86 (i386/i686){muslSuffix}",
                ArchitectureType.X64 => $"64-bit x86_64 (AMD64){muslSuffix}",
                ArchitectureType.X64V3 => $"64-bit x86_64 with AVX2 (modern CPUs){muslSuffix}",
                ArchitectureType.Arm => GetDetailedArmDescription() + muslSuffix,
                ArchitectureType.Arm64 => $"64-bit ARM64 (AArch64){muslSuffix}",
                ArchitectureType.Riscv64 => $"64-bit RISC-V{muslSuffix}",
                ArchitectureType.Mips => $"32-bit MIPS (Big Endian, Hardware Float){muslSuffix}",
                ArchitectureType.MipsLE => $"32-bit MIPS (Little Endian, Hardware Float){muslSuffix}",
                ArchitectureType.Mips64 => $"64-bit MIPS64 (Big Endian){muslSuffix}",
                ArchitectureType.Mips64LE => $"64-bit MIPS64 (Little Endian){muslSuffix}",
                ArchitectureType.PPC64 => $"64-bit PowerPC (Big Endian){muslSuffix}",
                ArchitectureType.PPC64LE => $"64-bit PowerPC (Little Endian){muslSuffix}",
                ArchitectureType.S390X => $"64-bit IBM System z (S390x){muslSuffix}",
                _ => $"Unknown architecture{muslSuffix}"
            };
        }
        
        private static string GetDetailedArmDescription()
        {
            try
            {
                if (File.Exists("/proc/cpuinfo"))
                {
                    string cpuInfo = File.ReadAllText("/proc/cpuinfo");
                    
                    if (cpuInfo.Contains("ARMv7") || cpuInfo.Contains("model name\t: ARMv7"))
                    {
                        return "32-bit ARMv7";
                    }
                    else if (cpuInfo.Contains("ARMv6") || cpuInfo.Contains("model name\t: ARMv6"))
                    {
                        return "32-bit ARMv6";
                    }
                    else if (cpuInfo.Contains("ARMv5") || cpuInfo.Contains("model name\t: ARMv5"))
                    {
                        return "32-bit ARMv5";
                    }
                }
            }
            catch
            {
                // Fall back to generic description if detection fails
            }
            
            return "32-bit ARM";
        }
    }
}
