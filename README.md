# Tobii EyeX Json TCP Server
TCP Server proxying Tobii's EyeX eye tracking device on Windows. 

## Getting Started
1. Install EyeX SDK on Windows
2. Add the following references to the Visual C# Project
⋅⋅* lib/EyeXFramework.dll
⋅⋅* lib/Tobii.EyeX.Client.Net20.dll
⋅⋅* lib/Newtonsoft.Json.dll
3. Copy the following to Debug/bin directory
⋅⋅* lib/Tobii.EyeX.Client.dll
4. Build and Run the Project. Make sure port 6555 is not allocated or use by other programs.

## Supported Devices
Tobii EyeX

## Protocol Document
To be written

## Available Clients
⋅⋅* Chrome App / Chrome Extensions https://github.com/5un/eyex-chrome