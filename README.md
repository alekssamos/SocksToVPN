# SocksToVPN 

A .NET Core console application that automatically configures tun2socks with proxy settings for transparent proxying across different operating systems.

## Features

- **Multi-Architecture Support**:
  - Supports x86, x64, ARM, ARM64, and RISC-V architectures
  - Automatic CPU architecture detection 
  - Specific support for Linux musl-based distributions (like Alpine Linux)

- **OS Detection**:
  - Windows (x86, x64, ARM64)
  - macOS (Intel x64, Apple Silicon ARM64)
  - Linux (standard glibc-based distributions)
  - Linux musl distributions

- **Intelligent Network Detection**:
  - Automatically detects the active network interface being used for internet
  - Multiple fallback strategies to ensure proper detection
  - Finds correct gateway for the detected network interface

- **Automated Setup**:
  - Downloads tun2socks binary for the current platform from GitHub
  - On Windows, downloads the appropriate Wintun driver for the current architecture
  - Configures OS-specific routing and network settings

## Usage
### From Release

1. Download SocksToVPN

First, you need to download the SocksToVPN application from GitHub. Select the version that matches your operating system and CPU architecture from the following link:

[SocksToVPN GitHub Releases](https://github.com/localtonet/SocksToVPN/releases/tag/latest)

Download and extract the appropriate file for your system.
2. Run the Application

After downloading, launch SocksToVPN. Use the following command in the terminal or command prompt:

`./SocksToVPN`

For Windows users, simply run the SocksToVPN.exe file.
3. Enter Your Proxy Information

SocksToVPN will prompt you to enter your SOCKS5 proxy credentials in the following format:

`ip:port:user:pass`

Example:

`192.168.1.100:1080:myuser:mypassword`

Once you enter this information, SocksToVPN will automatically establish the connection.
4. Establishing the Connection

SocksToVPN will automatically download and configure additional components like tun2socks and Wintun as needed, depending on your system requirements. It will then route your internet traffic through the SOCKS5 proxy.
5. Test Your Connection

To verify that your connection is working, use the following command:

`curl ifconfig.me`

This command will display your external IP address. If you are successfully connected through the SOCKS5 proxy, your IP address should match that of the proxy server.
### Basic Execution

```bash
dotnet run
```

The application will:
1. Detect your OS and CPU architecture
2. Prompt for proxy IP, port, username, and password
3. Download tun2socks and necessary drivers
4. Configure routing for transparent proxying

### Cross-Platform Building

Build for multiple platforms at once:

**Windows**:
```
build.bat
```

**Linux/macOS**:
```bash
chmod +x build.sh
./build.sh
```

These scripts will generate binaries for all supported platforms in the `publish` directory.

## Supported Runtime Identifiers

- Windows: `win-x86`, `win-x64`, `win-arm64`  
- macOS: `osx-x64`, `osx-arm64`
- Linux (glibc): `linux-x64`, `linux-arm`, `linux-arm64`, `linux-musl-x64`, `linux-musl-arm64`

## Requirements

- .NET 7.0 SDK to build the project
- Administrator/root privileges to run (for network configuration)

## Implementation Details

The application implements configuration according to the tun2socks wiki examples for each OS:

- **Windows**: Uses Wintun driver, configures wintun interface and DNS, sets up routing
- **macOS**: Creates utun interface, configures routing tables and proxy settings
- **Linux**: Sets up TUN interface, handles both glibc and musl variants, configures routing and rp_filter settings

## Cross-Compiling for musl-based Linux

The application is designed to work on Alpine Linux and other musl-based distributions, which require specific runtime identifiers (`linux-musl-x64`, `linux-musl-arm64`). The build scripts automatically generate these variants.
