using UnityEngine;

public static class FlowLongUtil
{    
    public static float EvaluateY01(FlowLongMeta meta, float t)
    {
        if (meta == null || meta.CurvePoints == null || meta.CurvePoints.Count == 0)
            return Mathf.Clamp01(t);

        t = Mathf.Clamp01(t);

        var points = meta.CurvePoints;

        // t 이하에서 가장 가까운 포인트, t 이상에서 가장 가까운 포인트를 찾는다.
        FlowCurvePoint prev = points[0];
        FlowCurvePoint next = points[points.Count - 1];

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].t <= t)
                prev = points[i];
            if (points[i].t >= t)
            {
                next = points[i];
                break;
            }
        }

        if (Mathf.Approximately(prev.t, next.t))
            return prev.y01;

        float localT = (t - prev.t) / (next.t - prev.t);
        return Mathf.Lerp(prev.y01, next.y01, localT);
    }
}
