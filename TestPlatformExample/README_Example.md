# Test Platform Example

This example demonstrates a minimal implementation of a plugin-based Test Platform.
It uses .NET 8 and Windows Forms for the UI. (Note: .NET 7 was originally requested, but SDK availability led to using .NET 8).
The platform now supports dynamic loading of plugins from a designated `Plugins` folder and C# scripting.

## Project Structure

- **CorePlatform**: A class library containing the `IPlugin` interface, `IScriptablePlugin` interface (with `ExecuteScriptCommand` and `GetAvailableScriptCommands`), `PluginManager`, `ScriptEngine`, and `ScriptingHost`.
- **SamplePlugin**: A class library (`SamplePlugin.dll`) that implements `IScriptablePlugin`. This serves as an example of a plugin that can be dynamically loaded and scripted.
- **WinFormsUI**: A Windows Forms application that acts as the host. It uses `PluginManager` to load plugins and `ScriptEngine` to execute C# scripts. It includes a main form with a basic script input area, a more advanced pop-up Script Editor, and a Plugin Information viewer.

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
    *   Create a subdirectory named `Plugins` if it doesn't already exist.
    *   Copy `SamplePlugin.dll` from its output directory (e.g., `TestPlatformExample/SamplePlugin/bin/Debug/net8.0/SamplePlugin.dll`) into the `Plugins` folder.
    *   Also, copy `CorePlatform.dll` into the same `Plugins` folder.

5.  **Run the `WinFormsUI` Application (on Windows)**:
    *   In Visual Studio: Set `WinFormsUI` as the startup project and click Start (or F5).
    *   Using .NET CLI: `dotnet run --project TestPlatformExample/WinFormsUI/WinFormsUI.csproj`.

## Functionality

- The main window provides buttons for "Load Plugins", "Run Plugin Tests", "Run Script" (for the main form's quick script area), "Open Script Editor", and "Plugin Info".
- **Load Plugins**: Scans the `Plugins` folder, loads DLLs, and instantiates `IPlugin` implementations.
- **Run Plugin Tests**: Executes `RunTest` on all loaded plugins.
- **Script Input & Execution**:
    - Quick scripts can be run from the main form.
    - A full **Script Editor** (`ScriptEditorForm`) offers advanced features: syntax highlighting, file operations, syntax checking, and customizable script settings (namespaces, assembly references).
- **Plugin Information**: The "Plugin Info" button opens a window (`PluginInfoForm`) listing loaded plugins and their details, including available script commands for `IScriptablePlugin` implementers.
- **Help System**: The Script Editor's Help menu opens a `HelpForm` displaying a detailed scripting guide and copyable example scripts.
- Log messages from all operations are displayed in the main list box and saved to `TestPlatformExample.log`.

This example demonstrates dynamic plugin loading, script execution with a customizable environment, and UI tools for managing and understanding these features.

## C# Scripting

The platform includes a C# scripting capability to interact with plugins. Scripts have access to many common .NET features by default (e.g., `System.IO`, `System.Text`, `System.Windows.Forms.MessageBox`).

For a comprehensive guide, see the [C# Scripting Platform Guide](./SCRIPTING_PLATFORM_GUIDE.md).

**UI Elements Supporting Scripting:**
- **MainForm Script Area**: For quick tests.
- **"Open Script Editor" Button**: Launches `ScriptEditorForm`.
- **`ScriptEditorForm`**: Advanced editor with File menu, Script menu ("Run Script", "Check Syntax", "Script Settings..."), and Help menu ("Scripting Guide...").
- **`PluginInfoForm`**: Launched from MainForm's "Plugin Info" button, it lists plugins and, for scriptable ones, their available commands (obtained via `GetAvailableScriptCommands()`).
- **Help Window (`HelpForm`)**: Shows this guide and example scripts.

**Scripting API (`Host` Object):**
Available in scripts:
- `void Log(string message)` / `void print(object? message)`: Log to UI.
- `string[] ListPluginNames()`: List loaded plugins.
- `string? ExecutePluginCommand(string pluginName, string commandName, string parameters)`: Execute commands on `IScriptablePlugin`s.

**Making Plugins Scriptable:**
Implement `CorePlatform.IScriptablePlugin`, which requires `ExecuteScriptCommand(...)` and `GetAvailableScriptCommands()`. The latter helps users discover commands via the Plugin Info window. See guides for details. `SamplePlugin` is an example.

**Example Script:**
```csharp
Host.print("Script: Listing loaded plugins...");
string[] plugins = Host.ListPluginNames();
if (plugins.Contains("Sample Test Plugin")) {
    Host.print("Script: Found Sample Test Plugin.");
    // Use Plugin Info window to see commands like "GetStatus", "Echo", "Add"
    string status = Host.ExecutePluginCommand("Sample Test Plugin", "GetStatus", "");
    Host.print("Script: SamplePlugin GetStatus result: " + status);
} else {
    Host.print("Script: Sample Test Plugin not found.");
}
```

## Plugin Development Requirements

1.  Create a .NET 8 Class Library.
2.  Reference `CorePlatform.csproj` (for `IPlugin`, `IScriptablePlugin`).
3.  Implement `IPlugin`. For scriptability, also implement `IScriptablePlugin` (including `ExecuteScriptCommand` and `GetAvailableScriptCommands`).
4.  Build your DLL.
5.  Deploy your plugin DLL and `CorePlatform.dll` to the `Plugins` folder in `WinFormsUI`'s output directory.

Scripts can be customized via "Script Settings..." in the Script Editor. Discover plugin commands using the "Plugin Info" window.

For more details:
- [C# Scripting Platform Guide](./SCRIPTING_PLATFORM_GUIDE.md) (English)
- [插件开发规范与要求 (Plugin Development Guide CN)](./PLUGIN_DEVELOPMENT_GUIDE_CN.md) (Chinese)

---
*The WinFormsUI project requires a Windows environment. CorePlatform and SamplePlugin can be built on any platform with .NET 8 SDK.*
