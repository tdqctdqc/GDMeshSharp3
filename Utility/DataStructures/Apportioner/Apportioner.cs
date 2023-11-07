
using System;
using System.Collections.Generic;
using System.Linq;

public static class Apportioner
{
    public static List<float> ApportionLinear<T>(float toApportion, IEnumerable<T> cands, Func<T, float> getPriority)
    {
        var res = cands.Select(getPriority).ToList();
        var totalPriority = res.Sum();
        for (var i = 0; i < res.Count; i++)
        {
            res[i] = toApportion * res[i] / totalPriority;
        }

        return res;
    }
    public static List<int> ApportionLinear<T>(int toApportion, IEnumerable<T> cands, Func<T, int> getScore)
    {
        var res = cands.Select(getScore).ToList();
        var totalScore = res.Sum();
        if (totalScore == 0)
        {
            for (var i = 0; i < res.Count; i++)
            {
                res[i] = toApportion / res.Count;
            }

            return res;
        }
        for (var i = 0; i < res.Count; i++)
        {
            res[i] = toApportion * res[i] / totalScore;
        }

        return res;
    }
}
