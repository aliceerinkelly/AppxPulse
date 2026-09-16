# AppxPulse (Pulse)

A lightweight, non-profit system configuration utility designed for standard users to manage Windows AppX packages simply and efficiently. 

## About The Project

AppxPulse provides a simple, interactive graphical interface that allows users to clean up their operating system environment by completely removing unwanted packages or quickly reinstalling native system components. It operates as an open-source tool with a focus on ease of use, speed, and safety.

### Key Features
*   **Visual Checkbox Selection:** Easily toggle target apps and packages on or off from a populated system state list.
*   **Simple Package Management:** Streamlined package removal and installation methods inspired by the lightweight functionality of utilities like GeekUninstaller.
*   **Low Memory Footprint:** Built optimized to execute with minimal system resource utilization, using significantly less memory during processing than traditional, heavy system deployment tools like NTLite.
*   **Completely Portable:** Compiled as a single, self-contained executable wrapper. All required managed assemblies, configurations, and dynamic runtime libraries are bundled natively into the binary.

## Interface Design
*   **Theme:** Clean, modern Grey, White, and Green high-DPI configuration layout.
*   **Target Users:** Designed for standard, non-technical users who need an uncomplicated dashboard to configure their Windows environment.

## Requirements & Environment
*   **Target Framework:** .NET 8.0 (Windows Desktop Runtime)
*   **Target OS Platform:** Windows 10 (Build 19041 or higher) / Windows 11
*   **Architecture:** 64-bit (win-x64)

## How to Run AppxPulse

Because this application is distributed as a **Self-Contained Single File**, standard users do not need to install the .NET SDK or any background development frameworks to run it.

1. Navigate to the [Releases](https://github.com) tab on the right side of this repository page.
2. Download the latest compiled **`AppXPulse.exe`** binary.
3. Move the executable to any folder on your machine (e.g., your Desktop).
4. Double-click the file to launch the dashboard and begin managing your system packages instantly!

## License

Distributed under the MIT License. See `LICENSE` for more information.
