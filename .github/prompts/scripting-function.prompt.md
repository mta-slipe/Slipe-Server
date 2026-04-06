# Scripting Function Implementation
Your goal is to implement one or more of MTA's Scripting functions in this codebase. A (list of) function name(s) will be provided to you.

Alternatively, it's possible you are requested to implement a set of functions without being provided the function names. In that case use the MTA MCP wiki `getFunctionList` tool to get the server side function list, and search through there for the functions to implement.

## Where
Function definitions are defined in the SlipeServer.Scripting project. Currently SlipeServer.Lua and SlipeServer.Luau are two runners that actually use these function definitions to execute code.

Both events and functions can be defined in the Scripting project.

## How
You can use the MTA Wiki MCP to get information about the function in question. It is then your goal to make a syntax compatible implementation of the function in C#. You can use the existing functions as a reference for how to implement the function. 

## Testing
Once the function is written, you can testing it by writing a test in the SlipeServer.Scripting.Lua.Tests project, that runs a Lua script and verifies the implementation.
Executing the test will yield any script execution errors and you can use that to debug your implementation. 

## Iteration
Your iteration should be as follows:
- Get relevant function information from the MTA Wiki MCP
- Write the function implementation in C# in the Scripting project
- Write a test in the Scripting.Lua.Tests project that verifies the implementation
- Execute the test and debug any issues that arise

## Oddities
MTA's scripting functions have some odd implementations. They tend to return false when a function fails, which is not entirely compatible with C# where we have the actual return type of the function.
Instead of handling this, have the return type be the appropraite return type for the successful case of the function. Exceptions can then be handled separately.