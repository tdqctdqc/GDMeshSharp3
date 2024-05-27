using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools;
using Google.OrTools.Graph;

public static class OrToolsExt
{

    public static Dictionary<TWorker, TTask> 
        GetLinearSumAssignment<TWorker, TTask>(
            IReadOnlyList<TWorker> workers,
            IReadOnlyList<TTask> tasks,
            Func<TWorker, TTask, int> getCost)
    {
        var assignment = new LinearSumAssignment();
        for (var i = 0; i < workers.Count; i++)
        {
            var worker = workers[i];
            for (var j = 0; j < tasks.Count; j++)
            {
                var task = tasks[j];
                var cost = getCost(worker, task);
                assignment.AddArcWithCost(i, j, cost);

            }
        }

        var solution = assignment.Solve();
        var res = new Dictionary<TWorker, TTask>();
        if (solution == LinearSumAssignment.Status.OPTIMAL)
        {
            for (var i = 0; i < workers.Count; i++)
            {
                var taskIndex = assignment.RightMate(i);
                var task = tasks[taskIndex];
                res.Add(workers[i], task);
            }
        }
        else
        {
            throw new Exception();
        }

        return res;
    }
    
    public static Dictionary<TWorker, TTask> 
        RepeatLinearSumAssignment<TWorker, TTask>(
            IReadOnlyList<TWorker> workersSource,
            IReadOnlyList<TTask> tasks,
            Func<TWorker, TTask, int> getCost)
    {
        var res = new Dictionary<TWorker, TTask>();
        var workersToTake = workersSource.ToHashSet();
        var maxIter = workersSource.Count / (float)tasks.Count
                      + 2;
        var iter = 0;
        while (workersToTake.Count > 0)
        {
            iter++;
            if (iter > maxIter)
            {
                throw new Exception();
            }
            var workers = workersToTake.ToList();
            var assignment = new LinearSumAssignment();

            for (var i = 0; i < workers.Count; i++)
            {
                var worker = workers[i];
                for (var j = 0; j < tasks.Count; j++)
                {
                    var task = tasks[j];
                    var cost = getCost(worker, task);
                    if (cost == 0) cost = 1;
                    if (cost < 0 || float.IsNaN(cost))
                    {
                        throw new Exception();
                    }
                    assignment.AddArcWithCost(i, j, cost);
                }
            }

            var solution = assignment.Solve();
            if (solution == LinearSumAssignment.Status.OPTIMAL)
            {
                for (var i = 0; i < workers.Count; i++)
                {
                    var taskIndex = assignment.RightMate(i);
                    if (taskIndex == -1)
                    {
                        GD.Print("skipping");
                        continue;
                    }
                    var task = tasks[taskIndex];
                    var worker = workers[i];
                    res.Add(worker, task);
                    workersToTake.Remove(worker);
                }
            }
            else
            {
                throw new Exception($"iter {iter} workers {workers.Count} jobs {tasks.Count}");
            }
        }

        return res;
    }

    public static Dictionary<TWorker, TTask>
        MinCostFlowAssignment<TWorker, TTask>(
            IReadOnlyList<TWorker> workers,
            IReadOnlyList<TTask> tasks,
            Func<TWorker, TTask, int> getCost)
    {
        var flow = workers.Count;
        int source = workers.Count + tasks.Count;
        int sink = source + 1;
        var solver = new MinCostFlow();
        for (var i = 0; i < workers.Count; i++)
        {
            solver.AddArcWithCapacityAndUnitCost(source,
                i, 1, 0);
        }
        for (var i = 0; i < tasks.Count; i++)
        {
            solver.AddArcWithCapacityAndUnitCost(
                i + workers.Count,
                sink, flow, 0);
        }
        for (var i = 0; i < workers.Count; i++)
        {
            var worker = workers[i];
            for (var j = 0; j < tasks.Count; j++)
            {
                var task = tasks[j];
                var taskNodeIndex = j + workers.Count;
                var cost = getCost(worker, task);
                solver.AddArcWithCapacityAndUnitCost(i, taskNodeIndex,
                    1, cost);
            }
        }
        
        solver.SetNodeSupply(source, flow);
        solver.SetNodeSupply(sink, -flow);
    
        var solution = solver.Solve();
        var res = new Dictionary<TWorker, TTask>();
        for (var i = 0; i < solver.NumArcs(); i++)
        {
            if (solver.Tail(i) == source || solver.Head(i) == sink)
            {
                continue;
            }

            if (solver.Flow(i) > 0)
            {
                var worker = solver.Tail(i);
                var task = solver.Head(i) - workers.Count;
                res.Add(workers[worker], tasks[task]);
            }
        }

        return res;
    }
}
