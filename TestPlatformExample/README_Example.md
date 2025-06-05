# Test Platform Example

This example demonstrates a minimal implementation of the Test Platform concepts described in the main README.md.
It uses .NET Core 8 and Windows Forms for the UI. (Note: .NET 7 was originally requested, but SDK availability led to using .NET 8)

## Project Structure

- **CorePlatform**: A class library containing the basic plugin interface (`IPlugin`) and `PluginManager`.
- **SamplePlugin**: A class library containing a simple implementation of `IPlugin` (`MyPlugin`).
- **WinFormsUI**: A Windows Forms application that acts as the host, loads the plugin, and allows running its test method.

## How to Build and Run

1.  **Prerequisites**:
    *   .NET 8 SDK installed.
    *   An IDE like Visual Studio 2022 or VS Code with C# support.
    *   For running `WinFormsUI`, a Windows environment is required.

2.  **Open the Solution**:
    *   Open `TestPlatformExample/TestPlatformExample.sln` in Visual Studio.
    *   Or, navigate to the `TestPlatformExample` folder in your terminal.

3.  **Build the Solution**:
    *   In Visual Studio: Build -> Build Solution.
    *   Using .NET CLI: `dotnet build TestPlatformExample/TestPlatformExample.sln` (Building `WinFormsUI` will fail on non-Windows environments).

4.  **Run the Application** (WinFormsUI on Windows):
    *   In Visual Studio: Set `WinFormsUI` as the startup project and click Start (or F5).
    *   Using .NET CLI: `dotnet run --project TestPlatformExample/WinFormsUI/WinFormsUI.csproj`

## Functionality

- The main window has a "Load Sample Plugin" button and a "Run Plugin Tests" button.
- **Load Sample Plugin**: Manually instantiates `MyPlugin` and adds it to the `PluginManager`. In a real application, plugins would be discovered dynamically from a specific folder.
- **Run Plugin Tests**: Iterates through loaded plugins (just the `MyPlugin` in this case) and executes their `RunTest` method.
- Log messages from the UI, `PluginManager`, and the plugin are displayed in the list box.

This example focuses on the basic interaction between a host application and a plugin, reflecting the modular design principle.
The solution and project files were created targeting .NET 8 due to SDK availability in the execution environment. The WinForms project (`WinFormsUI`) is included as specified but will only build and run on a Windows system with the appropriate .NET Desktop Runtime.
