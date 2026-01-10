namespace OnlyHumans.Understanding.Tests;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.SemanticKernel;

internal class TestPlugin : IPlugin
{
    public string Name => "TestPlugin";

    public Dictionary<string, Dictionary<string, object>> SharedState { get; set; } = new();

    [KernelFunction, Description("Adds two integers.")]
    public int Add(
        [Description("The first integer")] int a,
        [Description("The 2nd integer")] int b
    ) => a * b;


    [KernelFunction, Description("Subtracts an integer from the result of the previous function call.")]
    public int Subtract(
       [Description("The integer to subtract from the result of the previous function call.")] int a) => a;
}
