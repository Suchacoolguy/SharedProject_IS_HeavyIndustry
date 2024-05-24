using System;
using System.IO;

namespace SharedProject_IS_HeavyIndustry.Models;

public class CallDragon
{
    private string dragon = " _______ \n< Hello >\n ------- \n      \\                    / \\  //\\\n       \\    |\\___/|      /   \\//  \\\\\n            /0  0  \\__  /    //  | \\ \\    \n           /     /  \\/_/    //   |  \\  \\  \n           @_^_@'/   \\/_   //    |   \\   \\ \n           //_^_/     \\/_ //     |    \\    \\\n        ( //) |        \\///      |     \\     \\\n      ( / /) _|_ /   )  //       |      \\     _\\\n    ( // /) '/,_ _ _/  ( ; -.    |    _ _\\.-~        .-~~~^-.\n  (( / / )) ,-{        _      `-.|.-~-.           .~         `.\n (( // / ))  '/\\      /                 ~-. _ .-~      .-~^-.  \\\n (( /// ))      `.   {            }                   /      \\  \\\n  (( / ))     .----~-.\\        \\-'                 .~         \\  `. \\^-.\n             ///.----..>        \\             _ -~             `.  ^-`  ^-_\n               ///-._ _ _ _ _ _ _}^ - - - - ~                     ~-- ,.-~\n                                                                  /.-~\n";

    private string cow =
        " _______ \n< Hello >\n ------- \n        \\   ^__^\n         \\  (oo)\\_______\n            (__)\\       )\\/\\\n                ||----w |\n                ||     ||\n";

    public void GoDragon()
    {
        Console.Write(dragon);
    }

    public void GoCow()
    {
        Console.Write(cow);
    }
}