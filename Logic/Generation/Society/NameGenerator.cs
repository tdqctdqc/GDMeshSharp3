using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RandomFriendlyNameGenerator;

public class NameGenerator
{
    public static string GetName()
    {
        return RandomFriendlyNameGenerator.NameGenerator.PersonNames.Get().Split(' ')[0];
    }
}