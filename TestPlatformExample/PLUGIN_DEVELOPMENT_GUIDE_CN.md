# 插件开发指南 (PLUGIN_DEVELOPMENT_GUIDE_CN.md)

本指南旨在帮助开发者为 `TestPlatformExample` 项目创建新的插件。

## 1. 基本要求

- **.NET 版本**: 推荐使用 .NET 8，与主示例项目保持一致。
- **开发工具**: Visual Studio 2022 或其他兼容的 .NET IDE (如 VS Code)。

## 2. 创建插件项目

1.  **新建类库项目**:
    *   启动 Visual Studio 或使用 .NET CLI。
    *   创建一个新的 .NET 类库 (Class Library) 项目。例如，命名为 `MyCustomPlugin`。
    *   确保项目目标框架为 `net8.0` (或与 `CorePlatform.dll` 兼容的版本)。

2.  **添加对 `CorePlatform` 的引用**:
    *   您的插件需要实现 `CorePlatform.IPlugin` 接口。为此，您必须引用 `CorePlatform` 项目或其编译后的 `CorePlatform.dll`。
    *   **方式一：项目引用 (推荐，如果插件与主解决方案一起开发)**:
        编辑您的插件项目文件 (`.csproj`)，添加如下项目引用。请根据您的目录结构调整相对路径。
        ```xml
        <ItemGroup>
          <ProjectReference Include="..\CorePlatform\CorePlatform.csproj" />
          <!-- 假设 MyCustomPlugin 文件夹与 CorePlatform 文件夹同级 -->
        </ItemGroup>
        ```
    *   **方式二：DLL引用**:
        如果您将 `CorePlatform.dll` 作为单独的文件分发，可以在项目中添加对该 DLL 的引用。
        ```xml
        <ItemGroup>
          <Reference Include="CorePlatform.dll">
            <HintPath>..\Path\To\CorePlatform.dll</HintPath> <!-- 提供 CorePlatform.dll 的实际路径 -->
          </Reference>
        </ItemGroup>
        ```
        **注意**: 如果采用 DLL 引用，请确保在部署插件时，`CorePlatform.dll` 也被正确地复制到 `WinFormsUI` 的 `Plugins` 目录中 (详见部署步骤)。

## 3. 实现 `IPlugin` 接口

在您的插件项目中，创建一个公共类，并实现 `CorePlatform.IPlugin` 接口。

```csharp
using System;
using CorePlatform; // 确保 using CorePlatform 命名空间

namespace MyCustomPlugin
{
    public class MyPlugin : IPlugin
    {
        private Action<string>? _hostLogCallback; // 用于接收宿主程序的日志回调

        public string Name => "我的自定义插件"; // 插件名称

        public string Description => "这是一个自定义插件的简单示例。"; // 插件描述

        // 插件加载时调用
        public void Load()
        {
            // 初始化插件资源
            Console.WriteLine($"{Name} 已加载。"); // 也可以使用 _hostLogCallback 记录日志，但此时可能尚未设置
        }

        // 插件卸载时调用
        public void Unload()
        {
            // 释放插件资源
            _hostLogCallback?.Invoke($"{Name} 已卸载。");
            Console.WriteLine($"{Name} 已卸载。");
        }

        // 执行插件定义的测试（或主要功能）
        public void RunTest(Action<string> logCallback)
        {
            _hostLogCallback = logCallback; // 保存宿主提供的日志回调实例

            _hostLogCallback?.Invoke($"插件 '{Name}': 开始执行测试...");

            // 模拟测试步骤
            try
            {
                System.Threading.Thread.Sleep(500); // 模拟工作
                _hostLogCallback?.Invoke($"插件 '{Name}': 步骤 1 完成。");
                System.Threading.Thread.Sleep(1000); // 模拟更多工作
                _hostLogCallback?.Invoke($"插件 '{Name}': 步骤 2 完成。");
                _hostLogCallback?.Invoke($"插件 '{Name}': 测试成功结束。");
            }
            catch (Exception ex)
            {
                _hostLogCallback?.Invoke($"插件 '{Name}': 测试执行期间发生错误: {ex.Message}");
            }
        }
    }
}
```

## 4. 构建插件

编译您的插件项目，生成 DLL 文件 (例如，`MyCustomPlugin.dll`)。
通常，此 DLL 会在项目的 `bin/Debug/net8.0/` 或 `bin/Release/net8.0/` 目录下。

## 5. 部署插件到 `WinFormsUI`

1.  **找到 `WinFormsUI` 的运行目录**:
    这是 `WinFormsUI.exe` 所在的目录。如果您从 Visual Studio 运行，它通常是 `TestPlatformExample/WinFormsUI/bin/Debug/net8.0/` 或 `TestPlatformExample/WinFormsUI/bin/Release/net8.0/`。

2.  **创建 `Plugins` 文件夹**:
    在 `WinFormsUI` 的运行目录下，创建一个名为 `Plugins` 的子文件夹 (如果尚不存在)。

3.  **复制插件文件**:
    *   将您的插件 DLL (例如 `MyCustomPlugin.dll`) 复制到上述 `Plugins` 文件夹中。
    *   **重要**: 同时需要将 `CorePlatform.dll` 复制到此 `Plugins` 文件夹中。
        *   如果您的插件项目通过**项目引用**链接到 `CorePlatform`，则 `CorePlatform.dll` 通常会自动复制到您插件的输出目录中。您可以将您插件输出目录中的 `MyCustomPlugin.dll` 和 `CorePlatform.dll` 一起复制到 `WinFormsUI/Plugins/` 目录下。
        *   如果您的插件项目通过**DLL引用**链接到 `CorePlatform.dll`，则需要手动确保 `CorePlatform.dll` 与您的插件 DLL 一起位于 `Plugins` 文件夹中。

4.  **复制其他依赖项 (如果需要)**:
    如果您的插件依赖于任何其他第三方库 (并且这些库不是 .NET 运行时的一部分，也不是 `WinFormsUI` 主程序本身提供的)，您也需要将这些依赖 DLL 复制到 `Plugins` 文件夹或 `WinFormsUI` 的主运行目录中，以便 .NET 运行时能够找到它们。

## 6. 运行和测试

启动 `WinFormsUI` 应用程序。点击 "Load Plugins" (或类似) 按钮。应用程序的日志区域应显示您的插件被发现和加载的消息。随后，您可以通过 "Run Plugin Tests" 按钮来执行插件的 `RunTest` 方法。

---
遵循这些步骤，您应该能够成功开发并集成新的插件到 `TestPlatformExample` 平台中。
