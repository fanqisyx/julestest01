# Test Platform Example

This example demonstrates a minimal implementation of a plugin-based Test Platform.
It uses .NET 8 and Windows Forms for the UI. (Note: .NET 7 was originally requested, but SDK availability led to using .NET 8).
The platform now supports dynamic loading of plugins from a designated `Plugins` folder.

## Project Structure

- **CorePlatform**: A class library containing the `IPlugin` interface and the `PluginManager` responsible for discovering and managing plugins.
- **SamplePlugin**: A class library (`SamplePlugin.dll`) that implements `IPlugin`. This serves as an example of a plugin that can be dynamically loaded.
- **WinFormsUI**: A Windows Forms application that acts as the host. It uses `PluginManager` to load plugins from a `Plugins` subdirectory (located in its own output folder, e.g., `bin/Debug/net8.0/Plugins/`) and allows running their test methods.

## How to Build and Run

1.  **Prerequisites**:
    *   .NET 8 SDK installed.
    *   An IDE like Visual Studio 2022 or VS Code with C# support.
    *   For building and running `WinFormsUI`, a Windows environment is required.

2.  **Open the Solution**:
    *   Open `TestPlatformExample/TestPlatformExample.sln` in Visual Studio.
    *   Or, navigate to the `TestPlatformExample` folder in your terminal.

3.  **Build the Solution**:
    *   In Visual Studio: Build -> Build Solution. This will build `CorePlatform`, `SamplePlugin`, and attempt to build `WinFormsUI`.
    *   Using .NET CLI: `dotnet build TestPlatformExample/TestPlatformExample.sln`. (Building `WinFormsUI` will fail on non-Windows environments, but `CorePlatform.dll` and `SamplePlugin.dll` will still be built).

4.  **Prepare Plugins for `WinFormsUI` (on Windows)**:
    *   After building the solution, navigate to the output directory of `WinFormsUI` (e.g., `TestPlatformExample/WinFormsUI/bin/Debug/net8.0/`).
    *   Create a subdirectory named `Plugins` if it doesn't already exist. (The application will attempt to create this folder on first run if missing, but it will be empty).
    *   Copy `SamplePlugin.dll` from its output directory (e.g., `TestPlatformExample/SamplePlugin/bin/Debug/net8.0/SamplePlugin.dll`) into the `Plugins` folder you just created/verified.
    *   Also, copy `CorePlatform.dll` into the same `Plugins` folder. `SamplePlugin.dll` depends on `CorePlatform.dll`. The `SamplePlugin` project is configured to output `CorePlatform.dll` alongside `SamplePlugin.dll` by default due to the project reference, so you can typically copy both from `TestPlatformExample/SamplePlugin/bin/Debug/net8.0/`.

5.  **Run the `WinFormsUI` Application (on Windows)**:
    *   In Visual Studio: Set `WinFormsUI` as the startup project and click Start (or F5).
    *   Using .NET CLI: `dotnet run --project TestPlatformExample/WinFormsUI/WinFormsUI.csproj`.

## Functionality

- The main window has a "Load Plugins" button (previously "Load Sample Plugin") and a "Run Plugin Tests" button.
- **Load Plugins**: Clicking this button triggers the `PluginManager` to scan the `Plugins` subdirectory (within the application's execution directory) for DLLs. It attempts to load assemblies, find types implementing `IPlugin`, instantiate them, and call their `Load()` method.
- **Run Plugin Tests**: Iterates through successfully loaded plugins and executes their `RunTest` method.
- Log messages from the UI, `PluginManager`, and the plugins are displayed in the list box.

This example now focuses on dynamic plugin discovery and loading, a core aspect of modular and extensible platforms.

## Plugin Development Requirements

To create your own plugin compatible with this example platform:

1.  **Create a .NET Class Library Project**: It's recommended to use .NET 8, consistent with this example.
2.  **Add a Project Reference to `CorePlatform.csproj`**: Your plugin project will need to reference the `CorePlatform` project (or its output `CorePlatform.dll`) to access the `IPlugin` interface.
    ```xml
    <!-- Example for your .csproj -->
    <ItemGroup>
      <ProjectReference Include="..\CorePlatform\CorePlatform.csproj" />
      <!-- Adjust path as necessary if your plugin is outside the TestPlatformExample solution structure -->
    </ItemGroup>
    ```
    Alternatively, you can distribute `CorePlatform.dll` and reference it directly as a DLL. If you do this, ensure `CorePlatform.dll` is available for your plugin at runtime (see Deployment step).
3.  **Implement `CorePlatform.IPlugin`**: Create a public class in your library that implements the `IPlugin` interface.
4.  **Build your Plugin**: Compile your project to produce a DLL (e.g., `YourPlugin.dll`).
5.  **Deployment**:
    *   Locate the output directory of the `WinFormsUI` application (e.g., `WinFormsUI/bin/Debug/net8.0/` or wherever `WinFormsUI.exe` is).
    *   Inside this directory, create a folder named `Plugins`.
    *   Copy your plugin's DLL (e.g., `YourPlugin.dll`) into this `Plugins` folder.
    *   **Crucially**, also ensure `CorePlatform.dll` is present in this `Plugins` folder alongside your plugin's DLL. If your plugin project references `CorePlatform` as a project reference, `CorePlatform.dll` is typically copied to your plugin's output directory automatically, so you can just copy both `YourPlugin.dll` and `CorePlatform.dll` from your plugin's output into the `WinFormsUI/Plugins` folder.

When the "Load Plugins" button is clicked in the WinForms UI, it will scan this `Plugins` folder, load compatible DLLs, and instantiate your plugin.

For a more detailed guide on plugin development in Chinese, please see [插件开发规范与要求 (Plugin Development Guide CN)](./PLUGIN_DEVELOPMENT_GUIDE_CN.md).

---
*The solution and project files were created targeting .NET 8 due to SDK availability in the execution environment. The WinForms project (`WinFormsUI`) is included as specified but will only build and run on a Windows system with the appropriate .NET Desktop Runtime.*
