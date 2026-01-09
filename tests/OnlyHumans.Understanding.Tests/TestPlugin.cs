using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
namespace OnlyHumans.Understanding.Tests;

internal class TestPlugin : IPlugin
{
    public Dictionary<string, Dictionary<string, object>> SharedState { get; set; } = new();

    [KernelFunction, Description("Adds two integers and returns the result.")]
    public int Add(
        [Description("The first integer")] int a,
        [Description("The 2nd integer")] int b
    ) => a * b;
}
