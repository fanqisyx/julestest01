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

- **Script Input Area**: A multi-line text box (using `FastColoredTextBox` for syntax highlighting) where you can write or paste your C# scripts.
- **"Run Script" Button**: Executes the script currently in the input area.
- **Log Display**: The main ListBox in the UI will show:
    - Messages logged by your script via `Host.Log()`.
    - Return values from your script.
    - Notifications of script success or failure.
    - Detailed compilation errors if your script fails to compile.
    - Runtime error messages if your script encounters an exception during execution.

## 3. The `Host` Scripting API

When you write a C# script, a global object named `Host` is automatically available to your script. This `Host` object is an instance of `CorePlatform.ScriptingHost` and provides the bridge between your script and the plugin platform.

The `Host` object exposes the following methods:

-   **`void Log(string message)`**
    *   **Description**: Logs a custom message to the main UI log display.
    *   **Example**: `Host.Log("This is a message from my script.");`

-   **`string[] ListPluginNames()`**
    *   **Description**: Retrieves an array of strings containing the `Name` property of all currently loaded and active plugins.
    *   **Example**:
        ```csharp
        string[] names = Host.ListPluginNames();
        if (names.Length > 0) {
            Host.Log("Loaded plugins: " + string.Join(", ", names));
        } else {
            Host.Log("No plugins are currently loaded.");
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
        Host.Log("Status of Sample Test Plugin: " + (status ?? "N/A"));

        string echoResult = Host.ExecutePluginCommand("Sample Test Plugin", "Echo", "Hello Plugin!");
        Host.Log("Echo result: " + echoResult);
        ```

## 4. Writing Scripts - Best Practices

-   **Check for Loaded Plugins**: Before attempting to execute commands on a plugin, ensure it's loaded. Use `Host.ListPluginNames()` and check if your target plugin's name is in the list.
-   **Error Handling in Scripts**: While the host logs errors, your script can also use `try-catch` blocks if you anticipate specific issues or want to handle plugin command errors gracefully.
-   **Parameter Passing**: For `ExecutePluginCommand`, the `parameters` argument is a single string. If you need to pass multiple values or complex data:
    -   Use a delimited string (e.g., `"param1,param2,param3"`) and parse it within your plugin.
    -   Consider using JSON strings for more structured data, and then deserialize the JSON within your plugin's `ExecuteScriptCommand` method (this would require adding a JSON library reference to your plugin project).
-   **Return Values**: Scripts can return values. The last expression evaluated in a script is often its return value. For example, `1 + 1` at the end of a script will result in the script returning `2`. This return value will be logged by the UI.
-   **Logging**: Use `Host.Log()` liberally to trace your script's execution path and intermediate values. This is very helpful for debugging.

## 5. Example Script (using `SamplePlugin`)

This script assumes `SamplePlugin` (which implements `IScriptablePlugin`) has been loaded.

```csharp
// 1. Log a startup message
Host.Log("Starting comprehensive script for SamplePlugin...");

// 2. List all loaded plugins
string[] pluginNames = Host.ListPluginNames();
Host.Log("Currently loaded plugins: " + string.Join(", ", pluginNames));

// 3. Define the target plugin name
string targetPlugin = "Sample Test Plugin"; // Matches SamplePlugin.Name

// 4. Check if the target plugin is loaded
if (pluginNames.Contains(targetPlugin))
{
    Host.Log($"Plugin '{targetPlugin}' is loaded. Proceeding with commands.");

    // 5. Execute 'GetStatus' command
    string? status = Host.ExecutePluginCommand(targetPlugin, "GetStatus", "");
    Host.Log($"'{targetPlugin}' GetStatus result: '{status ?? "null"}'");

    // 6. Execute 'Echo' command
    string? echoMessage = "Hello from a C# script!";
    string? echoResult = Host.ExecutePluginCommand(targetPlugin, "Echo", echoMessage);
    Host.Log($"'{targetPlugin}' Echo command with '{echoMessage}' result: '{echoResult ?? "null"}'");

    // 7. Execute 'Add' command with valid parameters
    string? addParams1 = "10,20.5";
    string? addResult1 = Host.ExecutePluginCommand(targetPlugin, "Add", addParams1);
    Host.Log($"'{targetPlugin}' Add command with '{addParams1}' result: '{addResult1 ?? "null"}'");

    // 8. Execute 'Add' command with invalid parameters
    string? addParams2 = "hello,world";
    string? addResult2 = Host.ExecutePluginCommand(targetPlugin, "Add", addParams2);
    Host.Log($"'{targetPlugin}' Add command with '{addParams2}' result: '{addResult2 ?? "null"}' (expected error)");

    // 9. Execute an unknown command
    string? unknownCmdResult = Host.ExecutePluginCommand(targetPlugin, "NonExistentCommand", "some_params");
    Host.Log($"'{targetPlugin}' NonExistentCommand result: '{unknownCmdResult ?? "null"}' (expected error)");
}
else
{
    Host.Log($"Error: Plugin '{targetPlugin}' is not loaded. Cannot execute commands.");
}

Host.Log("Script finished.");
// Optional: Return a summary value
"Script execution summary: All planned steps attempted.";
```

## 6. Developing Scriptable Plugins

For a plugin to be targeted by `Host.ExecutePluginCommand`, it must:
1.  Implement the `CorePlatform.IPlugin` interface.
2.  Implement the `CorePlatform.IScriptablePlugin` interface, which adds the method:
    `string? ExecuteScriptCommand(string commandName, string parameters);`
3.  Be built as a DLL and placed in the `Plugins` folder of the `WinFormsUI` application, along with `CorePlatform.dll`.

Refer to the `README_Example.md` and `PLUGIN_DEVELOPMENT_GUIDE_CN.md` for detailed steps on creating and deploying plugins. The `SamplePlugin` included in this example project is a reference for a scriptable plugin.
