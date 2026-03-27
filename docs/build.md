# Build the Code

For building Caliburn.Light yourself you need:

- [Git](https://git-scm.com/)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) with:
  - .NET desktop development workload
  - Windows application development workload (for WinUI)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Building

1. Clone the repository to your computer:
   ```
   git clone https://github.com/tibel/Caliburn.Light.git
   ```

2. Navigate to the folder where you cloned the repository

3. Open `Caliburn.Light.slnx` in Visual Studio 2026

4. Press `F6` (or use the Build menu) to build the solution

## Solution Structure

- `src/` - Framework source code
  - `Caliburn.Light.Core` - Core library (platform-independent)
  - `Caliburn.Light.WPF` - WPF platform support
  - `Caliburn.Light.WinUI` - WinUI 3 platform support
  - `Caliburn.Light.Avalonia` - Avalonia platform support

- `tests/` - Test projects ([TUnit](https://github.com/thomhurst/TUnit))
  - `Caliburn.Light.Core.Tests` - Core library tests
  - `Caliburn.Light.WPF.Tests` - WPF platform tests
  - `Caliburn.Light.WinUI.Tests` - WinUI platform tests
  - `Caliburn.Light.Avalonia.Tests` - Avalonia platform tests

- `samples/` - Sample applications
  - `Caliburn.Light.Gallery.WPF` - WPF gallery demo
  - `Caliburn.Light.Gallery.WinUI` - WinUI gallery demo
  - `Caliburn.Light.Gallery.Avalonia` - Avalonia gallery demo

## Running Tests

```
dotnet test
```

WinUI tests require a runtime identifier:
```
dotnet test --project tests/Caliburn.Light.WinUI.Tests -r win-x64
```

## Running the Samples

Set one of the gallery projects as the startup project and press F5 to run.
