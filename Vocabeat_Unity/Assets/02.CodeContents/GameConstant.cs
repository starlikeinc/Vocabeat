using System.Collections.Generic;
using UnityEngine;

public static class GameConstant
{
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
