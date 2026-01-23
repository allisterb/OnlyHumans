namespace OnlyHumans.Automation;

using System;

using Jumbee.Console;
using static Jumbee.Console.Style;

internal class Program
{
    static void Main(string[] args)
    {
        var p = new TextPrompt(">")        
            .WithRoundedBorder(Purple)
            .WithTitle("Foo");
        var tree = new Tree("tree", TreeGuide.Line, Green | Dim) { Width = 30, Height = 40 };           
        tree.AddNodes("Y".WithStyle(Red | Dim), "Z".WithStyle(Blue | Underline)).WithTitle("ff");
        p.IsFocused = true;
        var d = new DockPanel(DockedControlPlacement.Left, tree, p);
        //var g = new Grid([10], [100, 100], [p, tree.WithRoundedBorder(Blue)]);
        var t = UI.Start(d);    
        t.Wait();
    }
}
