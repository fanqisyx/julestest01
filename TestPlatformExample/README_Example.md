# Test Platform Example

This example demonstrates a minimal implementation of a plugin-based Test Platform.
It uses .NET 8 and Windows Forms for the UI. (Note: .NET 7 was originally requested, but SDK availability led to using .NET 8).
The platform now supports dynamic loading of plugins from a designated `Plugins` folder and C# scripting.

## Project Structure

- **CorePlatform**: A class library containing the `IPlugin` interface, `IScriptablePlugin` interface, `PluginManager`, `ScriptEngine`, and `ScriptingHost`.
- **SamplePlugin**: A class library (`SamplePlugin.dll`) that implements `IScriptablePlugin`. This serves as an example of a plugin that can be dynamically loaded and scripted.
- **WinFormsUI**: A Windows Forms application that acts as the host. It uses `PluginManager` to load plugins and `ScriptEngine` to execute C# scripts.

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

- The main window has a "Load Plugins" button, a "Run Plugin Tests" button, a "Run Script" button, and a script input area.
- **Load Plugins**: Clicking this button triggers the `PluginManager` to scan the `Plugins` subdirectory (within the application's execution directory) for DLLs. It attempts to load assemblies, find types implementing `IPlugin`, instantiate them, and call their `Load()` method.
- **Run Plugin Tests**: Iterates through successfully loaded plugins and executes their `RunTest` method.
- **Script Input & Execution**: Users can type C# script code into the text area and click "Run Script". The `ScriptEngine` executes this script, providing a `Host` object for interaction.
- Log messages from the UI, `PluginManager`, plugins, and scripts are displayed in the list box.

This example now focuses on dynamic plugin discovery, loading, and C# scripting, core aspects of modular and extensible platforms.

## C# Scripting

This example platform now includes a basic C# scripting capability, allowing you to write and execute C# scripts to interact with loaded plugins.

**UI Elements:**
- A multi-line **Script Input TextBox** is provided below the log display for writing or pasting your C# scripts.
- A **"Run Script" Button** will execute the script entered in the TextBox.
- Script output, return values, and errors will be displayed in the main log ListBox.

**Scripting API (`Host` Object):**
Within your C# scripts, a global object named `Host` (of type `CorePlatform.ScriptingHost`) is available. It provides the following methods:
- `void Log(string message)`: Logs a message to the main UI log.
- `string[] ListPluginNames()`: Returns an array of names of all currently loaded plugins.
- `string? ExecutePluginCommand(string pluginName, string commandName, string parameters)`: Executes a command on a specified plugin. The plugin must implement the `IScriptablePlugin` interface. `commandName` is a string identifying the action, and `parameters` is a string containing any data for the command.

**Making Plugins Scriptable:**
For a plugin to be controllable by scripts using `Host.ExecutePluginCommand`, it must implement the `CorePlatform.IScriptablePlugin` interface and its `ExecuteScriptCommand` method. See the "Plugin Development Requirements" section below (and the detailed Chinese guide) for more information. The `SamplePlugin` has been updated to be scriptable.

**Example Script:**
```csharp
// Ensure "Load Plugins" has been clicked first to load SamplePlugin
Host.Log("Script: Listing loaded plugins...");
string[] plugins = Host.ListPluginNames();
if (plugins.Contains("Sample Test Plugin")) {
    Host.Log("Script: Found Sample Test Plugin.");
    string status = Host.ExecutePluginCommand("Sample Test Plugin", "GetStatus", "");
    Host.Log("Script: SamplePlugin GetStatus result: " + status);

    string echo = Host.ExecutePluginCommand("Sample Test Plugin", "Echo", "Hello from Script!");
    Host.Log("Script: SamplePlugin Echo result: " + echo);

    string sum = Host.ExecutePluginCommand("Sample Test Plugin", "Add", "25,17");
    Host.Log("Script: SamplePlugin Add result: " + sum);
} else {
    Host.Log("Script: Sample Test Plugin not found. Ensure it's loaded.");
}
```

## Plugin Development Requirements

To create your own plugin compatible with this example platform:

1.  **Create a .NET Class Library Project**: It's recommended to use .NET 8, consistent with this example.
2.  **Add a Project Reference to `CorePlatform.csproj`**: Your plugin project will need to reference the `CorePlatform` project (or its output `CorePlatform.dll`) to access the `IPlugin` interface and `IScriptablePlugin` if you want scripting support.
    ```xml
    <!-- Example for your .csproj -->
    <ItemGroup>
      <ProjectReference Include="..\CorePlatform\CorePlatform.csproj" />
      <!-- Adjust path as necessary if your plugin is outside the TestPlatformExample solution structure -->
    </ItemGroup>
    ```
    Alternatively, you can distribute `CorePlatform.dll` and reference it directly as a DLL. If you do this, ensure `CorePlatform.dll` is available for your plugin at runtime (see Deployment step).
3.  **Implement `CorePlatform.IPlugin` (and optionally `IScriptablePlugin`)**: Create a public class in your library that implements these interfaces.
4.  **Build your Plugin**: Compile your project to produce a DLL (e.g., `YourPlugin.dll`).
5.  **Deployment**:
    *   Locate the output directory of the `WinFormsUI` application (e.g., `WinFormsUI/bin/Debug/net8.0/` or wherever `WinFormsUI.exe` is).
    *   Inside this directory, create a folder named `Plugins`.
    *   Copy your plugin's DLL (e.g., `YourPlugin.dll`) into this `Plugins` folder.
    *   **Crucially**, also ensure `CorePlatform.dll` is present in this `Plugins` folder alongside your plugin's DLL. If your plugin project references `CorePlatform` as a project reference, `CorePlatform.dll` is typically copied to your plugin's output directory automatically, so you can just copy both `YourPlugin.dll` and `CorePlatform.dll` from your plugin's output into the `WinFormsUI/Plugins` folder.

When the "Load Plugins" button is clicked in the WinForms UI, it will scan this `Plugins` folder, load compatible DLLs, and instantiate your plugin. If your plugin implements `IScriptablePlugin`, its `ExecuteScriptCommand` method can be called from scripts.

For a more detailed guide on plugin development in Chinese, please see [插件开发规范与要求 (Plugin Development Guide CN)](./PLUGIN_DEVELOPMENT_GUIDE_CN.md).

---
*The solution and project files were created targeting .NET 8 due to SDK availability in the execution environment. The WinForms project (`WinFormsUI`) is included as specified but will only build and run on a Windows system with the appropriate .NET Desktop Runtime.*
