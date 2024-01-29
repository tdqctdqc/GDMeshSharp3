using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

namespace GDMeshSharp3.Exception;

public static class TimeOuter
{
    public static void Try(Action action, int milliseconds,
        string msg)
    {
        var sw = new Stopwatch();
        sw.Start();
        var task = Task.Run(action);
        while (task.IsCompleted == false)
        {
            if (sw.Elapsed.TotalMilliseconds > milliseconds)
            {
                GD.Print(msg + " timeout");
                throw new System.Exception();
            }
        }
        // GD.Print(msg + " failed to timeout");
    }
}