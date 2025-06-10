# C# Scripting Platform Guide for TestPlatformExample

This guide provides an overview of the C# scripting capabilities within the `TestPlatformExample` project, how to use the scripting UI, and how to develop scriptable plugins.

## 1. Introduction to C# Scripting

The platform includes a C# scripting engine (`Microsoft.CodeAnalysis.CSharp.Scripting`) that allows you to execute C# code snippets dynamically at runtime. This is useful for:

- Automating interactions with one or more plugins.
- Performing custom test sequences.
- Querying plugin statuses or data.
- Prototyping plugin commands or logic quickly.

## 2. Using the Scripting UI (`WinFormsUI`)

The main `WinFormsUI` application provides the following elements for scripting:

- **Script Input Area**: A multi-line text box (using `FastColoredTextBox` for syntax highlighting) on the main form where you can write or paste short C# scripts.
- **"Run Script" Button (on MainForm)**: Executes the script currently in the main form's input area.
- **"Open Script Editor" Button**: Opens a separate, more advanced `ScriptEditorForm` window.
- **Log Display**: The main ListBox in the UI will show:
    - Messages logged by your script via `Host.Log()` or `Host.print()`.
    - Return values from your script.
    - Notifications of script success or failure.
    - Detailed compilation errors if your script fails to compile.
    - Runtime error messages if your script encounters an exception during execution.

The **`ScriptEditorForm`** provides:
- A larger `FastColoredTextBox` for script editing with syntax highlighting, line numbers, etc.
- **File Menu**: For New, Open, Save, Save As operations on script files.
- **Script Menu**:
    - **Run Script**: Executes the current script in the editor.
    - **Check Syntax**: Compiles the script without running it and reports any errors or warnings.
    - **Script Settings...**: Opens a dialog to customize the scripting environment for the current editor session by adding extra namespace imports or assembly references.
- **Help Menu**:
    - **Scripting Guide...**: Opens the "Scripting Help & Examples" window (this guide and examples).
- **Status Bar**: Displays status messages like "Ready", "Executing script...", "Syntax check: OK.", etc.

## 3. The `Host` Scripting API

When you write a C# script (either in `MainForm` or `ScriptEditorForm`), a global object named `Host` is automatically available. This `Host` object is an instance of `CorePlatform.ScriptingHost` and provides the bridge between your script and the plugin platform.

The `Host` object exposes the following methods:

-   **`void Log(string message)`**
    *   **Description**: Logs a custom message to the main UI log display. All messages are automatically prefixed with "Script> ".
    *   **Example**: `Host.Log("This is a message from my script.");`

-   **`void print(object? message)`**
    *   **Description**: A convenient alias for `Host.Log()`. It converts the given object to a string and logs it.
    *   **Example**: `Host.print($"The value is: {myVariable}");`

-   **`string[] ListPluginNames()`**
    *   **Description**: Retrieves an array of strings containing the `Name` property of all currently loaded and active plugins.
    *   **Example**:
        ```csharp
        string[] names = Host.ListPluginNames();
        if (names.Length > 0) {
            Host.print("Loaded plugins: " + string.Join(", ", names));
        } else {
            Host.print("No plugins are currently loaded.");
        }
        ```

-   **`string? ExecutePluginCommand(string pluginName, string commandName, string parameters)`**
    *   **Description**: Executes a specific command on a named plugin. The target plugin must implement the `CorePlatform.IScriptablePlugin` interface.
    *   **Parameters**:
        *   `pluginName` (string): The name of the plugin (must match the `Name` property of the plugin).
        *   `commandName` (string): A string identifying the command to be executed by the plugin.
        *   `parameters` (string): A string containing any parameters the command might need. The plugin's `ExecuteScriptCommand` method will be responsible for parsing this string.
    *   **Returns**: `string?`. The plugin's `ExecuteScriptCommand` method can return a string result. If it returns `null` or void (implicitly null for string-returning methods), this method also returns `null`. The returned string can indicate success, data, or an error message from the plugin.
    *   **Example**:
        ```csharp
        // Assuming "Sample Test Plugin" is loaded and implements IScriptablePlugin
        string status = Host.ExecutePluginCommand("Sample Test Plugin", "GetStatus", "");
        Host.print("Status of Sample Test Plugin: " + (status ?? "N/A"));

        string echoResult = Host.ExecutePluginCommand("Sample Test Plugin", "Echo", "Hello Plugin!");
        Host.print("Echo result: " + echoResult);
        ```

## 4. Default Scripting Environment

By default, your C# scripts have access to a range of common .NET functionalities. The following namespaces are automatically imported:

*   `System`
*   `System.IO` (for file operations)
*   `System.Text` (for `StringBuilder`, etc.)
*   `System.Text.RegularExpressions` (for `Regex`)
*   `System.Linq` (for LINQ queries)
*   `System.Collections.Generic` (for `List<T>`, `Dictionary<TKey, TValue>`, etc.)
*   `System.Net.Http` (for `HttpClient`)
*   `System.Threading.Tasks` (for `Task`)
*   `System.Xml.Linq` (for LINQ to XML)
*   `System.Diagnostics` (for `Stopwatch`, `Debug`)
*   `CorePlatform` (for `Host`, `IPlugin`, `IScriptablePlugin`)
*   `System.Windows.Forms` (if the platform is running as a Windows Forms application, allowing use of types like `MessageBox`)

This means you can often use types from these namespaces directly without needing `using` directives in your script (e.g., `List<int> numbers = new List<int>();`, `MessageBox.Show("Hello");`). Corresponding assemblies are also referenced.

## 5. Customizing Script Environment (Script Settings)

If your script needs access to additional .NET namespaces or assemblies not included by default, you can configure these through the Script Editor.

1.  In the `ScriptEditorForm` window (opened via "Open Script Editor" on the main form), go to the **Script** menu.
2.  Select **Script Settings...**. This will open the "Script Settings" dialog.

**In the "Script Settings" dialog:**

*   **Additional Using Namespaces (one per line):**
    Enter any extra namespaces your script requires (e.g., `System.Numerics`). These will be effectively added as `using Namespace;` directives for your script.
*   **Additional Assembly References (one per line; full path or GAC name):**
    If your script uses types from assemblies that are not referenced by default (e.g., a custom utility DLL you wrote, or a specific .NET assembly not in the default list), you can add them here.
    *   Provide the full path to the DLL (e.g., `C:\MyLibs\MyCustomLibrary.dll`).
    *   Or, for assemblies in the Global Assembly Cache (GAC), you can use the assembly's strong name (e.g., `System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089`). Note: GAC usage is less common in modern .NET applications. Relative paths for DLLs might be resolved from the application's execution directory.

Click "OK" to save these settings. These settings will be used by the `ScriptEngine` the next time you "Run Script" or "Check Syntax" *from the ScriptEditorForm that holds these settings*.

## 6. Using the Help Window

The Script Editor provides a help window to assist you:

1.  In the `ScriptEditorForm` window, go to the **Help** menu.
2.  Select **Scripting Guide...**. This will open the "Scripting Help & Examples" window.

**The Help Window features:**
*   **This Guide**: The top panel displays this scripting guide document for easy reference.
*   **Example Scripts**:
    *   A list of example script titles is shown in a tree view in the bottom-left panel.
    *   Clicking on an example title will display its C# code in the viewer panel (bottom-right).
    *   This code is read-only but can be studied and copied.
*   **"Copy Script to Clipboard" Button**: Below the example script viewer, this button copies the currently displayed example script's code to your clipboard, so you can easily paste it into the Script Editor.

## 7. Writing Scripts - Best Practices

-   **Check for Loaded Plugins**: Before attempting to execute commands on a plugin, ensure it's loaded. Use `Host.ListPluginNames()` and check if your target plugin's name is in the list.
-   **Error Handling in Scripts**: While the host logs errors, your script can also use `try-catch` blocks if you anticipate specific issues or want to handle plugin command errors gracefully.
-   **Parameter Passing**: For `ExecutePluginCommand`, the `parameters` argument is a single string. If you need to pass multiple values or complex data:
    -   Use a delimited string (e.g., `"param1,param2,param3"`) and parse it within your plugin.
    -   Consider using JSON strings for more structured data, and then deserialize the JSON within your plugin's `ExecuteScriptCommand` method (this would require adding a JSON library reference to your plugin project).
-   **Return Values**: Scripts can return values. The last expression evaluated in a script is often its return value. For example, `1 + 1` at the end of a script will result in the script returning `2`. This return value will be logged by the UI.
-   **Logging**: Use `Host.Log()` or `Host.print()` liberally to trace your script's execution path and intermediate values. This is very helpful for debugging.

## 8. Example Script (using `SamplePlugin`)

This script assumes `SamplePlugin` (which implements `IScriptablePlugin`) has been loaded.

```csharp
// 1. Log a startup message using print
Host.print("Starting comprehensive script for SamplePlugin...");

// 2. List all loaded plugins
string[] pluginNames = Host.ListPluginNames();
Host.print("Currently loaded plugins: " + string.Join(", ", pluginNames));

// 3. Define the target plugin name
string targetPlugin = "Sample Test Plugin"; // Matches SamplePlugin.Name

// 4. Check if the target plugin is loaded
if (pluginNames.Contains(targetPlugin))
{
    Host.print($"Plugin '{targetPlugin}' is loaded. Proceeding with commands.");

    // 5. Execute 'GetStatus' command
    string? status = Host.ExecutePluginCommand(targetPlugin, "GetStatus", ""); // Parameters can be null or empty string if not used
    Host.print($"'{targetPlugin}' GetStatus result: '{status ?? "null"}'");

    // 6. Execute 'Echo' command
    string? echoMessage = "Hello from a C# script!";
    string? echoResult = Host.ExecutePluginCommand(targetPlugin, "Echo", echoMessage);
    Host.print($"'{targetPlugin}' Echo command with '{echoMessage}' result: '{echoResult ?? "null"}'");

    // 7. Execute 'Add' command with valid parameters
    string? addParams1 = "10,20.5";
    string? addResult1 = Host.ExecutePluginCommand(targetPlugin, "Add", addParams1);
    Host.print($"'{targetPlugin}' Add command with '{addParams1}' result: '{addResult1 ?? "null"}'");

    // 8. Execute 'Add' command with invalid parameters
    string? addParams2 = "hello,world";
    string? addResult2 = Host.ExecutePluginCommand(targetPlugin, "Add", addParams2);
    Host.print($"'{targetPlugin}' Add command with '{addParams2}' result: '{addResult2 ?? "null"}' (expected error)");

    // 9. Execute an unknown command
    string? unknownCmdResult = Host.ExecutePluginCommand(targetPlugin, "NonExistentCommand", "some_params");
    Host.print($"'{targetPlugin}' NonExistentCommand result: '{unknownCmdResult ?? "null"}' (expected error)");

    // 10. Example of using MessageBox if WinForms is available
    // Host.ExecutePluginCommand(targetPlugin, "ShowMessageBox", "Hello from script via plugin!");
    // (Assuming SamplePlugin had a "ShowMessageBox" command that internally calls MessageBox.Show)
    // OR directly:
    // MessageBox.Show("Direct message from script!", "Script Direct Call");
}
else
{
    Host.print($"Error: Plugin '{targetPlugin}' is not loaded. Cannot execute commands.");
}

Host.print("Script finished.");
// Optional: Return a summary value
"Script execution summary: All planned steps attempted.";
```

## 9. Developing Scriptable Plugins

For a plugin to be targeted by `Host.ExecutePluginCommand`, it must:
1.  Implement the `CorePlatform.IPlugin` interface.
2.  Implement the `CorePlatform.IScriptablePlugin` interface, which adds the method:
    `string? ExecuteScriptCommand(string commandName, string parameters);`
3.  Be built as a DLL and placed in the `Plugins` folder of the `WinFormsUI` application, along with `CorePlatform.dll`.

Refer to the `README_Example.md` and `PLUGIN_DEVELOPMENT_GUIDE_CN.md` for detailed steps on creating and deploying plugins. The `SamplePlugin` included in this example project is a reference for a scriptable plugin.
