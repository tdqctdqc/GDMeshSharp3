
using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using Godot;

public class Assigner
{
    public static void Assign<TPicker, TPicked>(IEnumerable<TPicker> pickers,
        Func<TPicker, float> getPriority,
        Func<TPicker, IEnumerable<TPicked>> getExisting,
        Func<TPicked, float> getPrice, 
        HashSet<TPicked> toPick,
        Action<TPicker, TPicked> assign,
        Func<TPicker, TPicked, float> ranker)
    {
        if (pickers.Count() == 0) return;
        
        var totalPriority = pickers.Sum(getPriority);
        var priorities = pickers.ToDictionary(
            p => p,
            p => new Vector2(getExisting(p).Sum(getPrice), 
                getPriority(p) / totalPriority)
        );
        while (toPick.Count > 0)
        {
            var picker = priorities
                .MinBy(kvp =>
                {
                    var v2 = kvp.Value;
                    return v2.X / v2.Y;
                }).Key;
            var preferred = toPick.MaxBy(pick => ranker(picker, pick));
            assign(picker, preferred);
            var value = priorities[picker];
            priorities[picker] = new Vector2(value.X + getPrice(preferred), value.Y);
            toPick.Remove(preferred);
        }
    }

    public static void AssignAlongLine<TPoint, TUnit>(
        List<TPoint> points,
        List<TUnit> units,
        Func<TUnit, float> getStrength,
        Func<TPoint, TPoint, float> getSegCost,
        Func<TPoint, Vector2> getPos,
        Func<Vector2, Vector2, Vector2> getOffset,
        Action<TUnit, List<Vector2>> assign
    )
    {
        if (points.Count < 2) return;
        var totalCost = 0f;
        for (var i = 0; i < points.Count - 1; i++)
        {
            totalCost += getSegCost(points[i], points[i + 1]);
        }

        if (totalCost == 0f) throw new Exception();
        if (float.IsNaN(totalCost)) throw new Exception();


        if (totalCost == 0f) throw new Exception();
        var totalStrength = units.Sum(getStrength);
        if (totalStrength == 0f) throw new Exception();
        var unitProportions = new Queue<(TUnit unit, float startProp, float endProp)>();

        var runningStrength = 0f;
        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            var startProp = runningStrength / totalStrength;
            runningStrength += getStrength(unit);
            if (float.IsNaN(runningStrength)) throw new Exception();
            var endProp = runningStrength / totalStrength;
            unitProportions.Enqueue((unit, startProp, endProp));
        }

        var pointProportions = new List<float>{0};
        var runningCost = 0f;
        for (var i = 1; i < points.Count; i++)
        {
            runningCost += getSegCost(points[i - 1], points[i]);
            if (float.IsNaN(runningCost)) throw new Exception();
            pointProportions.Add(runningCost / totalCost);
        }

        var startEndPoints = new Dictionary<TUnit, (float cumulProp, Vector2 start, Vector2 end)>();

        while (unitProportions.TryDequeue(out var info))
        {
            var (unit, startProp, endProp) = info;
            var line = new List<Vector2>();
            var start = getPointAtProportion(startProp);
            line.Add(start);
            var firstIndexBetween = pointProportions
                .FindIndex(p => p > startProp && p < endProp);
            var lastIndexBetween = pointProportions
                .FindLastIndex(p => p > startProp && p < endProp);
            if (firstIndexBetween != -1 && lastIndexBetween != -1)
            {
                for (int i = firstIndexBetween; i <= lastIndexBetween; i++)
                {
                    line.Add(getPos(points[i]));
                }
            }
            

            var end = getPointAtProportion(endProp);
            line.Add(end);
            assign(unit, line);
        }

        Vector2 getPointAtProportion(float prop)
        {
            if (prop == 0f) return getPos(points[0]);
            if (prop == 1f) return getPos(points[points.Count - 1]);
            var lowerBoundIndex = pointProportions
                .FindLastIndex(f => prop >= f);
            if (lowerBoundIndex == -1)
            {
                throw new Exception();
            }
            if (lowerBoundIndex == points.Count - 1)
            {
                return getPos(points[points.Count - 1]);
            }

            var lowerPoint = points[lowerBoundIndex];
            var upperPoint = points[lowerBoundIndex + 1];
            var lowerPointProportion = pointProportions[lowerBoundIndex];
            var upperPointProportion = pointProportions[lowerBoundIndex + 1];

            var ratioAlongSeg = (prop - lowerPointProportion) / (upperPointProportion - lowerPointProportion);

            var offset = getOffset(getPos(lowerPoint), getPos(upperPoint));
            return getPos(lowerPoint) + offset * ratioAlongSeg;
        }


    
    }
}