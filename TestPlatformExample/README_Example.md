# Test Platform Example

This example demonstrates a minimal implementation of a plugin-based Test Platform.
It uses .NET 8 and Windows Forms for the UI. (Note: .NET 7 was originally requested, but SDK availability led to using .NET 8).
The platform now supports dynamic loading of plugins from a designated `Plugins` folder and C# scripting.

## Project Structure

- **CorePlatform**: A class library containing the `IPlugin` interface, `IScriptablePlugin` interface, `PluginManager`, `ScriptEngine`, and `ScriptingHost`.
- **SamplePlugin**: A class library (`SamplePlugin.dll`) that implements `IScriptablePlugin`. This serves as an example of a plugin that can be dynamically loaded and scripted.
- **WinFormsUI**: A Windows Forms application that acts as the host. It uses `PluginManager` to load plugins and `ScriptEngine` to execute C# scripts. It includes a main form with a basic script input area and a more advanced pop-up Script Editor.

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

- The main window has a "Load Plugins" button, a "Run Plugin Tests" button, a "Run Script" button (for the main form's quick script area), an "Open Script Editor" button, and a script input area.
- **Load Plugins**: Triggers `PluginManager` to scan the `Plugins` folder for DLLs, load them, and instantiate `IPlugin` implementations.
- **Run Plugin Tests**: Executes `RunTest` on all loaded plugins.
- **Script Input & Execution**:
    - Users can type C# scripts into the main form's text area and click its "Run Script" button.
    - The "Open Script Editor" button launches a separate window with advanced editing features (syntax highlighting, file operations, syntax checking, script settings). Scripts run from this editor use its current text and settings.
- Log messages from all operations are displayed in the main list box.

This example now focuses on dynamic plugin discovery, loading, and C# scripting with enhanced editor and environment customization features.

## C# Scripting

This example platform now includes a C# scripting capability, allowing you to write and execute C# scripts to interact with loaded plugins. Scripts now have more common .NET features available by default (e.g., from `System.IO`, `System.Text`, `System.Windows.Forms` like `MessageBox`).

For a comprehensive guide on using the scripting platform, including detailed API explanations, more examples, and troubleshooting tips, please see the [C# Scripting Platform Guide](./SCRIPTING_PLATFORM_GUIDE.md).

**UI Elements:**
- **MainForm Script Area**: A multi-line text box for quick scripts, with its own "Run Script" button.
- **"Open Script Editor" Button**: Launches the `ScriptEditorForm`.
- **ScriptEditorForm**: Provides a `FastColoredTextBox` for advanced script editing, along with menus for:
    - File operations (New, Open, Save, Save As).
    - Script execution ("Run Script", "Check Syntax").
    - **Script Settings**: Allows customization of namespaces and assembly references for the script.
    - Help (opens the "Scripting Help & Examples" window).
- **Log Display (MainForm)**: All script output, return values, and errors are displayed here.
- **Help Window**: Launched from the Script Editor's Help menu, it displays the detailed scripting guide and copyable example scripts.

**Scripting API (`Host` Object):**
Within your C# scripts, a global object named `Host` (of type `CorePlatform.ScriptingHost`) is available. It provides the following methods:
- `void Log(string message)`: Logs a message to the main UI log.
- `void print(object? message)`: An alias for `Log`, converting the object to a string.
- `string[] ListPluginNames()`: Returns an array of names of all currently loaded plugins.
- `string? ExecutePluginCommand(string pluginName, string commandName, string parameters)`: Executes a command on a specified plugin (must implement `IScriptablePlugin`).

**Making Plugins Scriptable:**
For a plugin to be controllable by scripts using `Host.ExecutePluginCommand`, it must implement `CorePlatform.IScriptablePlugin`. See the "Plugin Development Requirements" section and the linked guides for details. The `SamplePlugin` is an example.

**Example Script (can be run from MainForm or ScriptEditorForm):**
```csharp
// Ensure "Load Plugins" has been clicked first to load SamplePlugin
Host.print("Script: Listing loaded plugins..."); // Using print
string[] plugins = Host.ListPluginNames();
if (plugins.Contains("Sample Test Plugin")) {
    Host.print("Script: Found Sample Test Plugin.");
    string status = Host.ExecutePluginCommand("Sample Test Plugin", "GetStatus", "");
    Host.print("Script: SamplePlugin GetStatus result: " + status);

    string echo = Host.ExecutePluginCommand("Sample Test Plugin", "Echo", "Hello from Script!");
    Host.print("Script: SamplePlugin Echo result: " + echo);

    string sum = Host.ExecutePluginCommand("Sample Test Plugin", "Add", "25,17");
    Host.print("Script: SamplePlugin Add result: " + sum);
} else {
    Host.print("Script: Sample Test Plugin not found. Ensure it's loaded.");
}
```

## Plugin Development Requirements

To create your own plugin compatible with this example platform:

1.  **Create a .NET Class Library Project**: Recommended: .NET 8.
2.  **Reference `CorePlatform.csproj`**: Needed for `IPlugin` and `IScriptablePlugin`.
    ```xml
    <ItemGroup>
      <ProjectReference Include="..\CorePlatform\CorePlatform.csproj" />
    </ItemGroup>
    ```
3.  **Implement `CorePlatform.IPlugin` (and `IScriptablePlugin` for scriptability)**.
4.  **Build your Plugin DLL**.
5.  **Deployment**:
    *   Place your plugin DLL and `CorePlatform.dll` in the `Plugins` folder of `WinFormsUI`'s output directory.

When "Load Plugins" is clicked, your plugin will be loaded. If scriptable, `Host.ExecutePluginCommand` can call it. You can customize the script environment (add namespaces/assemblies) via "Script -> Script Settings..." in the Script Editor.

For more details:
- [C# Scripting Platform Guide](./SCRIPTING_PLATFORM_GUIDE.md) (English)
- [插件开发规范与要求 (Plugin Development Guide CN)](./PLUGIN_DEVELOPMENT_GUIDE_CN.md) (Chinese)

---
*The solution and project files were created targeting .NET 8 due to SDK availability in the execution environment. The WinForms project (`WinFormsUI`) is included as specified but will only build and run on a Windows system with the appropriate .NET Desktop Runtime.*
