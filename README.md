# TreeToString

The purpose of this class is to provide a tool that can parse Binary Expression Trees from a string to a list of all potential iterations that could be produced by the tree.

Example :

**Input:**

`"(param1 || (param2 && param3 && param4 && (param5 || param6 || param7)))"`

**Output:**

    param1, param2, param3, param4
    param1, param2, param3, param5
    param1, param2, param3, param6

Use [scripts\TestFunctionality.ps1](\scripts\TestFunctionality.ps1) to see it in action.
