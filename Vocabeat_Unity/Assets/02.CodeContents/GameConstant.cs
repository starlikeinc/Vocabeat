using System.Collections.Generic;
using UnityEngine;

public static class GameConstant
{
    public static int RequirePoint_S { get; private set; } = 15000;
    public static int RequirePoint_A { get; private set; } = 10000;
    public static int RequirePoint_B { get; private set; } = 7500;
    public static int RequirePoint_C { get; private set; } = 5000;    

    private static Dictionary<EJudgementType, int> PointByJudgeType = new Dictionary<EJudgementType, int>
    {
        { EJudgementType.BlueStar, 500 },
        { EJudgementType.WhiteStar, 100 },
        { EJudgementType.YellowStar, 50 },
        { EJudgementType.RedStar, 0 },
    };

    public static int GetPointByJudgement(EJudgementType type)
    {
        if (!PointByJudgeType.TryGetValue(type, out int point))
            Debug.LogError($"{type} 에 해당하는 점수가 지정되어있지 않음.");
        return point;
    }
        
}
