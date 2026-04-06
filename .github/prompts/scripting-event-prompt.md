# Scripting Event Implementation
Your goal is to implement one or more of MTA's Scripting events in this codebase. A (list of) event name(s) will be provided to you.

Alternatively, it's possible you are requested to implement a set of events without being provided the event names. In that case use the MTA MCP wiki `getEventList` tool to get the server side function list, and search through there for the events to implement.

## Where
Event definitions are defined in the SlipeServer.Scripting project. Currently SlipeServer.Lua and SlipeServer.Luau are two runners that actually use these event definitions to execute code.

Both events and functions can be defined in the Scripting project.

## How
You can use the MTA Wiki MCP to get information about the event in question. It is then your goal to make a signature compatible implementation of the event in C#. You can use the existing event definition as a reference for how to implement the event. 

## Testing
Once the event is written, you can testing it by writing a test in the SlipeServer.Scripting.Lua.Tests project, that runs a Lua script and verifies the implementation.
Executing the test will yield any script execution errors and you can use that to debug your implementation. 

## Iteration
Your iteration should be as follows:
- Get relevant event information from the MTA Wiki MCP
- Write the event implementation in C# in the Scripting project
- Write a test in the Scripting.Lua.Tests project that verifies the implementation
- Execute the test and debug any issues that arise

## Naming
Sometimes the event parameter names aren't that great. You can rename the parameters to be more descriptive, but make sure to keep the event name the same as the MTA function name. This is important for consistency and for users who are familiar with the MTA functions.

Please also if  there are any arguments that have a `the` prefix (like `thePed`), remove the `the` prefix and rename the argument to be more descriptive (like `ped`).
