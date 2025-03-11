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
