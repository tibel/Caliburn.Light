# Build the Code

For building Caliburn.Light yourself you need:

- [Git](https://git-scm.com/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+) with:
  - .NET desktop development workload
  - Windows application development workload (for WinUI)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building

1. Clone the repository to your computer:
   ```
   git clone https://github.com/tibel/Caliburn.Light.git
   ```

2. Navigate to the folder where you cloned the repository

3. Open `Caliburn.Light.slnx` in Visual Studio 2022

4. Press `F6` (or use the Build menu) to build the solution

## Solution Structure

- `src/` - Framework source code
  - `Caliburn.Light.Core` - Core library (platform-independent)
  - `Caliburn.Light.WPF` - WPF platform support
  - `Caliburn.Light.WinUI` - WinUI 3 platform support
  - `Caliburn.Light.Avalonia` - Avalonia platform support

- `samples/` - Sample applications
  - `Caliburn.Light.Gallery.WPF` - WPF gallery demo
  - `Caliburn.Light.Gallery.WinUI` - WinUI gallery demo
  - `Caliburn.Light.Gallery.Avalonia` - Avalonia gallery demo

## Running the Samples

Set one of the gallery projects as the startup project and press F5 to run.
