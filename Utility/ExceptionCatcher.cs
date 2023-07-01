
using System;
using System.Linq;
using Godot;

public class ExceptionCatcher
{
    public static bool Try(Action action, Action<DisplayableException> display)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception e)
        {
            if (e is DisplayableException i)
            {
                display(i);
                GD.Print(AbbrStackTrace(i.StackTrace));
            }
            else
            {
                throw;
            }

            return false;
        }
    }

    private static string AbbrStackTrace(string stackTrace)
    {
        var res = "";
        var s = stackTrace.Split("C:");
        for (var i = 0; i < s.Length; i++)
        {
            var sub = s[i].Split("\\");
            res += sub.Last() + " \n";
        }

        return res;
    }
}
