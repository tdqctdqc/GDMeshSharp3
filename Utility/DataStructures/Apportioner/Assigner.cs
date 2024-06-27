
using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using Godot;

public class Assigner
{
    public static List<(TPicker, TPicked, float)>
        AssignFractional<TPicker, TPicked>(
            IReadOnlyList<TPicker> pickers,
            IReadOnlyList<TPicked> toPick,
            Func<TPicker, float> getNeed,
            Func<TPicked, float> getCapability
            )
    {
        
        var res = new List<(TPicker, TPicked, float)>();
        var totalNeed = pickers.Sum(getNeed);
        var totalCapability = toPick.Sum(getCapability);
        if (totalCapability == 0f) return res;
        
        var needProportionBookmark = 0f;
        var capabilityProportionBookmark = 0f;
        var pickerIter = 0;
        var toPickIter = 0;
        while (pickerIter < pickers.Count
               && toPickIter < toPick.Count)
        {
            var nextToPick = toPick[toPickIter];
            var nextPicker = pickers[pickerIter];
            
            var cap = getCapability(nextToPick);
            var nextCap = capabilityProportionBookmark + cap / totalCapability;
            var need = getNeed(nextPicker);
            var nextNeed = needProportionBookmark + need / totalNeed;
            
            if (nextCap == nextNeed)
            {
                pickerIter++;
                toPickIter++;
                needProportionBookmark += need / totalNeed;
                capabilityProportionBookmark += cap / totalCapability;
                res.Add((nextPicker, nextToPick, needProportionBookmark));
            }
            else if (nextCap < nextNeed)
            {
                capabilityProportionBookmark += cap / totalCapability;
                res.Add((nextPicker, nextToPick, capabilityProportionBookmark));
                toPickIter++;
            }
            else if (nextCap > nextNeed)
            {
                needProportionBookmark += need / totalNeed;
                res.Add((nextPicker, nextToPick, needProportionBookmark));
                pickerIter++;
            }
            else throw new Exception();
        }

        var lastProp = 0f;
        for (var i = 0; i < res.Count; i++)
        {
            var (picker, picked, proportion) = res[i];
            var diff = proportion - lastProp;
            var cap = getCapability(picked);
            if (cap == 0f)
            {
                res[i] = (picker, picked, 0f);
            }
            else
            {
                var marginalCapability = diff * totalCapability;
                var selfProportion = marginalCapability / cap;
                if (selfProportion < 0f || selfProportion > 1.001f)
                {
                    GD.Print($"bad proportion {selfProportion}" +
                             $"\n capability {cap}" +
                             $"\n proportion {proportion}" +
                             $"\n last proportion {lastProp}" +
                             $"\n marginal cap {marginalCapability}" +
                             $"\n total cap {totalCapability}" +
                             $"\n diff {diff}");
                }

            res[i] = (picker, picked, selfProportion);
            }

            lastProp = proportion;
        }
        
        foreach (var picked in toPick)
        {
            var sum = res
                .Where(v => v.Item2.Equals(picked))
                .Sum(v => v.Item3);
            if (Mathf.Abs(sum - 0f) > .001f
                && Mathf.Abs(sum - 1f) > .001f)
            {
                GD.Print("bad sum " + sum);
            }
        }
        return res;
    }
    public static void AssignRanked<TPicker, TPicked>(IEnumerable<TPicker> pickers,
        Func<TPicker, float> getPriority,
        Func<TPicker, IEnumerable<TPicked>> getExisting,
        Func<TPicked, float> getPrice, 
        HashSet<TPicked> toPick,
        Action<TPicker, TPicked> assign,
        Func<TPicker, TPicked, float> ranker)
    {
        if (pickers.Any() == false) return;
        
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
    
    
    
    
    public static void AssignDiscrete<TPicker, TPicked>(IEnumerable<TPicker> pickers,
        Func<TPicker, float> getPriority,
        Func<TPicker, IEnumerable<TPicked>> getExisting,
        Func<TPicked, float> getValue, 
        HashSet<TPicked> toPick,
        Action<TPicker, TPicked> assign)
    {
        if (pickers.Any() == false) return;
        
        var totalPriority = pickers.Sum(getPriority);
        var priorities = pickers.ToDictionary(
            p => p,
            p => new Vector2(getExisting(p).Sum(getValue), 
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
            var preferred = toPick.MaxBy(pick => getValue(pick));
            assign(picker, preferred);
            var value = priorities[picker];
            priorities[picker] = new Vector2(value.X + getValue(preferred), value.Y);
            toPick.Remove(preferred);
        }
    }
    
    
    
    public static void AssignToLimit<TPicker, TPicked>(IEnumerable<TPicker> pickers,
        Func<TPicker, float> getPriority,
        Func<TPicker, float> getLimit,
        Func<TPicker, IEnumerable<TPicked>> getExisting,
        Func<TPicked, float> getValue, 
        HashSet<TPicked> toPick,
        Action<TPicker, TPicked> assign)
    {
        if (pickers.Any() == false) return;
        
        var totalPriority = pickers.Sum(getPriority);
        var priorities = pickers.ToDictionary(
            p => p,
            p => new Vector3(getExisting(p).Sum(getValue), 
                getPriority(p) / totalPriority,
                getLimit(p))
        );
        while (toPick.Count > 0)
        {
            var validPriorities = priorities
                .Where(p => p.Value.X < p.Value.Z);
            if (validPriorities.Any() == false) break;
            var picker = validPriorities
                .MinBy(kvp =>
                {
                    var v2 = kvp.Value;
                    return v2.X / v2.Y;
                }).Key;
            var preferred = toPick.MaxBy(pick => getValue(pick));
            assign(picker, preferred);
            var value = priorities[picker];
            priorities[picker] = new Vector3(value.X + getValue(preferred), value.Y, value.Z);
            toPick.Remove(preferred);
        }
    }
    
    
    
    
    
    
    public static void AssignSingle<TPicker, TPicked>(
        IEnumerable<TPicker> pickers,
        Func<TPicker, float> getPriority,
        Func<TPicked, float> getValue, 
        HashSet<TPicked> toPick,
        Action<TPicker, TPicked> assign)
    {
        if (pickers.Any() == false) return;
        foreach (var picker in pickers.OrderByDescending(getPriority))
        {
            if (toPick.Any() == false) break;
            var preferred = toPick.MaxBy(pick => getValue(pick));
            assign(picker, preferred);
            toPick.Remove(preferred);
        }
    }
    
    
    public static Dictionary<TUnit, TFace> 
        PickBestAndAssignAlongFacesSingle<TUnit, TFace>(
        List<TFace> faces,
        IEnumerable<TUnit> units,
        Func<TUnit, float> getStrength,
        Func<TUnit, TFace, float> rank,
        Func<TFace, float> getFaceCost)
    {
        if (faces.Count == 0) return new Dictionary<TUnit, TFace>();
        if (faces.Count == 1) return units.ToDictionary(u => u, u => faces.First());
        
        var totalCost = 0f;
        for (var i = 0; i < faces.Count; i++)
        {
            totalCost += getFaceCost(faces[i]);
        }
        if (totalCost == 0f) throw new Exception();
        if (float.IsNaN(totalCost)) throw new Exception();

        var totalStrength = units.Sum(getStrength);
        if (totalStrength == 0f) throw new Exception();
        if (float.IsNaN(totalStrength)) throw new Exception();

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
        var canCoverAll = pickFrom.Count >= faces.Count;
        

        while (pickFrom.Count > 0)
        {
            var proportion = runningStrength / totalStrength;
            var faceIndex = getFaceIndexOfProportion(proportion);
            
            var face = faces[faceIndex];
            var picked = pickFrom.MaxBy(u => rank(u, face));
            pickFrom.Remove(picked);
            unitsInOrder.Add(picked);
            res.Add(picked, face);
            runningStrength += getStrength(picked);
        }
        
        
        

        return res;
        
        int getFaceIndexOfProportion(float prop)
        {
            if (prop == 0f) return 0;
            if (prop == 1f) return faces.Count - 1;
            for (var i = 0; i < faceProportions.Length; i++)
            {
                var faceProp = faceProportions[i];
                if (faceProp >= prop) return i;
            }

            return faces.Count - 1;
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
        if (totalStrength < 0f) throw new Exception();
        if (totalStrength == 0f)
        {
            totalStrength = units.Count();
            getStrength = u => 1f;
        }
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
            if (prop < 0) throw new Exception();
            if (prop == 0f) return 0;
            if (prop >= 1f) return faces.Count - 1;
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
    
    
    
    
    
    
    
    
    
    
    
    
    
    public static Dictionary<TUnit, Vector2I> 
        PickInOrderAndAssignAlongFaces2<TUnit, TFace>(
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
        if (totalStrength < 0f) throw new Exception();
        if (totalStrength == 0f)
        {
            totalStrength = units.Count();
            getStrength = u => 1f;
        }
        if (float.IsNaN(totalStrength)) throw new Exception();

        var res = new Dictionary<TUnit, Vector2I>();
        var currUnitIndex = 0;
        var currFaceStartIndex = 0;
        var currFaceEndIndex = 0;
        var runningStrProp = getStrength(units[currUnitIndex]) / totalStrength;
        var runningCostProp = getFaceCost(faces[currFaceEndIndex]) / totalCost;
        while (currUnitIndex < units.Count())
        {
            while (runningCostProp < runningStrProp
                   && currFaceEndIndex < faces.Count - 1)
            {
                currFaceEndIndex++;
                runningCostProp += getFaceCost(faces[currFaceEndIndex]) / totalCost;
            }

            var span = new Vector2I(currFaceStartIndex, currFaceEndIndex);
            res.Add(units[currUnitIndex], span);
            
            currFaceStartIndex = currFaceEndIndex;
            
            currUnitIndex++;
            if (currUnitIndex > units.Count - 1) break;
            runningStrProp += getStrength(units[currUnitIndex]) / totalStrength;
        }

        return res;
    }
}