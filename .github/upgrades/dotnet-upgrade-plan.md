# .NET 10.0 (Preview) Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.

3. Upgrade `SlipeServer.Packets\SlipeServer.Packets.csproj`
4. Upgrade `SlipeServer.Net\SlipeServer.Net.csproj`
5. Upgrade `SlipeServer.Server\SlipeServer.Server.csproj`
6. Upgrade `SlipeServer.Scripting\SlipeServer.Scripting.csproj`
7. Upgrade `SlipeServer.Lua\SlipeServer.Lua.csproj`
8. Upgrade `SlipeServer.LuaControllers\SlipeServer.LuaControllers.csproj`
9. Upgrade `SlipeServer.ConfigurationProviders\SlipeServer.ConfigurationProviders.csproj`
10. Upgrade `SlipeServer.Luau\SlipeServer.Luau.csproj`
11. Upgrade `SlipeServer.Physics\SlipeServer.Physics.csproj`
12. Upgrade `SlipeServer.Example\SlipeServer.Example.csproj`
13. Upgrade `SlipeServer.Hosting\SlipeServer.Hosting.csproj`
14. Upgrade `SlipeServer.Console\SlipeServer.Console.csproj`
15. Upgrade `SlipeServer.Server.TestTools\SlipeServer.Server.TestTools.csproj`
16. Upgrade `SlipeServer.WebHostBuilderExample\SlipeServer.WebHostBuilderExample.csproj`
17. Upgrade `SlipeServer.HostBuilderExample\SlipeServer.HostBuilderExample.csproj`
18. Upgrade `SlipeServer.SlipeLuaIntegration\SlipeServer.SlipeLuaIntegration.csproj`
19. Upgrade `SlipeServer.Server.Tests\SlipeServer.Server.Tests.csproj`
20. Upgrade `SlipeServer.Packets.Tests\SlipeServer.Packets.Tests.csproj`
21. Convert `NetModuleWrapper\NetModuleWrapper.vcxproj` to SDK-style project (if applicable) and then upgrade.

## Settings

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|


### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                        | Current Version         | New Version | Description                                   |
|:---------------------------------------------------|:----------------------:|:-----------:|:----------------------------------------------|
| DotNetZip                                           | 1.16.0                 |             | Deprecated and contains security vulnerability; remove or replace. |
| Microsoft.AspNetCore.OpenApi                        | 8.0.4                  | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Configuration.UserSecrets     | 8.0.0                  | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.DependencyInjection           | 6.0.0                  | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.0               | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Hosting                       | 8.0.0                  | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Hosting.Abstractions          | 8.0.0                  | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Http                          | 6.0.0                  | 10.0.0      | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Logging.Abstractions          | 6.0.0;8.0.0            | 10.0.0      | Multiple referenced versions; unify and upgrade to 10.0.0 |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.10.14             |             | No supported version found for .NET 10.0; consider removing or replacing. |
| Microsoft.Win32.Registry                           | 6.0.0-preview.5.21301.5 |             | Functionality included in framework; remove package reference. |
| Newtonsoft.Json                                    | 13.0.1                 | 13.0.4      | Patch upgrade to address potential issues/security fixes. |
| System.Drawing.Common                              | 5.0.3                  | 10.0.0      | Deprecated; replace or remove and use supported alternatives. |

### Project upgrade details

#### SlipeServer.Packets modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - (No package changes recommended specifically for this project in analysis.)

Other changes:
  - Verify any API breaking changes after retargeting and run tests.

#### SlipeServer.Net modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - Verify runtime and API compatibility.

#### SlipeServer.Server modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.DependencyInjection: 6.0.0 -> 10.0.0
  - Microsoft.Extensions.Http: 6.0.0 -> 10.0.0
  - Microsoft.Extensions.Logging.Abstractions: 6.0.0 -> 10.0.0
  - Newtonsoft.Json: 13.0.1 -> 13.0.4
  - DotNetZip: remove or replace due to security/deprecation

Other changes:
  - Address any API changes in Microsoft.Extensions namespaces and verify behavior.

#### SlipeServer.Scripting modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.Lua modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.LuaControllers modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.ConfigurationProviders modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Newtonsoft.Json: 13.0.1 -> 13.0.4

#### SlipeServer.Luau modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.DependencyInjection.Abstractions: 8.0.0 -> 10.0.0

#### SlipeServer.Physics modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.Example modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.Hosting modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting.Abstractions: 8.0.0 -> 10.0.0
  - Microsoft.Extensions.Logging.Abstractions: 8.0.0 -> 10.0.0

#### SlipeServer.Console modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Configuration.UserSecrets: 8.0.0 -> 10.0.0
  - System.Drawing.Common: remove or replace; recommended to upgrade to supported approach for graphics on cross-platform.
  - Microsoft.VisualStudio.Azure.Containers.Tools.Targets: no supported version for .NET 10.0; consider removing or keeping if only used for Docker tooling in IDE.
  - Microsoft.Win32.Registry: remove and use framework reference if available.

#### SlipeServer.Server.TestTools modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting: 8.0.0 -> 10.0.0
  - Microsoft.Extensions.Hosting.Abstractions: 8.0.0 -> 10.0.0

#### SlipeServer.WebHostBuilderExample modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Configuration.UserSecrets: 8.0.0 -> 10.0.0
  - Microsoft.AspNetCore.OpenApi: 8.0.4 -> 10.0.0

#### SlipeServer.HostBuilderExample modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Configuration.UserSecrets: 8.0.0 -> 10.0.0
  - Microsoft.Extensions.Hosting: 8.0.0 -> 10.0.0

#### SlipeServer.SlipeLuaIntegration modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.Server.Tests modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### SlipeServer.Packets.Tests modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

#### NetModuleWrapper modifications

Project file changes:
  - Convert `NetModuleWrapper\NetModuleWrapper.vcxproj` to SDK-style project if feasible, and then retarget to `net10.0`. If the project is native C++ or cannot be converted, exclude from automatic .NET upgrade and handle separately.


