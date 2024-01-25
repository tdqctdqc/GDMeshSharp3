
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
    
    public static void AssignAllAlongLine<TPoint, TUnit>(
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

    public static Dictionary<TUnit, TFace> 
        PickBestAndAssignAlongFacesSingle<TUnit, TFace>(
        List<TFace> faces,
        IEnumerable<TUnit> units,
        Func<TUnit, float> getStrength,
        Func<TUnit, TFace, float> rank,
        float minStrengthToTake,
        Func<TFace, float> getFaceCost)
    {
        if (faces.Count == 0) return new Dictionary<TUnit, TFace>();
        if (faces.Count == 1) return units.ToDictionary(u => u, u => faces.First());
        
        var totalCost = 0f;
        for (var i = 0; i < faces.Count; i++)
        {
            totalCost += getFaceCost(faces[i]);
        }
        if (units.Sum(getStrength) < minStrengthToTake) throw new Exception(); 
        if (totalCost == 0f) throw new Exception();
        if (float.IsNaN(totalCost)) throw new Exception();

        var res = new Dictionary<TUnit, TFace>();
        var faceProportions = new float[faces.Count];
        var runningCost = 0f;
        for (var i = 0; i < faces.Count; i++)
        {
            runningCost += getFaceCost(faces[i]);
            if (float.IsNaN(runningCost)) throw new Exception();
            faceProportions[i] = runningCost / totalCost;
        }

        var runningStrength = 0f;
        var unitsInOrder = new List<TUnit>();
        var pickFrom = units.ToHashSet();
        while (runningStrength / minStrengthToTake < 1f)
        {
            var proportion = runningStrength / minStrengthToTake;
            var face = getFaceAtProportion(proportion);
            var picked = pickFrom.MaxBy(u => rank(u, face));
            pickFrom.Remove(picked);
            unitsInOrder.Add(picked);
            res.Add(picked, face);
            runningStrength += getStrength(picked);
        }

        return res;
        
        TFace getFaceAtProportion(float prop)
        {
            if (prop == 0f) return faces[0];
            if (prop == 1f) return faces[faces.Count - 1];
            for (var i = 0; i < faceProportions.Length; i++)
            {
                var faceProp = faceProportions[i];
                if (faceProp >= prop) return faces[i];
            }

            return faces[faces.Count - 1];
        }
    }
    
    
    
    
    
    
    public static Dictionary<TUnit, Vector2I> 
        PickInOrderAndAssignAlongFaces<TUnit, TFace>(
        IReadOnlyList<TFace> faces,
        IReadOnlyList<TUnit> units,
        Func<TUnit, float> getStrength,
        Func<TFace, float> getFaceCost)
    {
        if (faces.Count == 0) throw new Exception();
        if (faces.Count == 1) return units.ToDictionary(u => u, 
            u => new Vector2I(0, 0));

        var totalCost = faces.Sum(getFaceCost);
        if (totalCost <= 0f) throw new Exception();
        if (float.IsNaN(totalCost)) throw new Exception();
        var totalStrength = units.Sum(getStrength);
        if (totalStrength <= 0f) throw new Exception();
        if (float.IsNaN(totalStrength)) throw new Exception();

        var res = new Dictionary<TUnit, Vector2I>();
        var faceProportions = new Vector2[faces.Count];
        
        var runningCost = 0f;
        for (var i = 0; i < faces.Count; i++)
        {
            if (float.IsNaN(runningCost)) throw new Exception();
            var startProp = runningCost / totalCost;
            runningCost += getFaceCost(faces[i]);
            var endProp = runningCost / totalCost;
            if (i == faces.Count - 1) endProp = 1f;
            faceProportions[i] = new Vector2(startProp, endProp);
        }

        var runningStrength = 0f;
        
        
        for (var j = 0; j < units.Count; j++)
        {
            var picked = units[j];
            var startProp = runningStrength / totalStrength;
            runningStrength += getStrength(picked);
            var endProp = runningStrength / totalStrength;
            if (j == units.Count - 1) endProp = 1f;
            
            var startFace = getFaceAtProportion(startProp);
            var endFace = getFaceAtProportion(endProp);
            var list = new List<TFace>();
            for (int i = startFace; i <= endFace; i++)
            {
                list.Add(faces[i]);
            }
            res.Add(picked, new Vector2I(startFace, endFace));
        }

        return res;
        
        int getFaceAtProportion(float prop)
        {
            if (prop < 0 || prop > 1f) throw new Exception();
            if (prop == 0f) return 0;
            if (prop == 1f) return faces.Count - 1;
            for (var i = 0; i < faceProportions.Length; i++)
            {
                var faceProps = faceProportions[i];
                if (faceProps.X <= prop && prop <= faceProps.Y)
                {
                    return i;
                }
            }

            return faces.Count - 1;
        }
    }
}