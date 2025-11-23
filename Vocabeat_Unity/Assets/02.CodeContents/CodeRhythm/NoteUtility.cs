using System.Collections.Generic;
using UnityEngine;

public static class NoteUtility
{
    public static Vector2 GetNotePosition(RectTransform anchorRectTrs, int tick, float y01)
    {
        float parentWidth = anchorRectTrs.rect.width;
        float parentHeight = anchorRectTrs.rect.height;

        int tickPerPage = ManagerRhythm.Instance.RTimeline.TicksPerPage;
        int tickInPage = tick % tickPerPage; // 현재 노트의 페이지 내 Tick
        float x01 = (float)tickInPage / tickPerPage;

        float posX = (x01 - 0.5f) * parentWidth;
        float posY = (y01 - 0.5f) * parentHeight;

        return new Vector2(posX, posY);
    }

    #region FlowHold Note 관련

    /// <summary>
    /// (기존) Bezier 기반 Y 평가 함수.
    /// 다른 곳에서 쓸 수 있으니 남겨두고,
    /// FlowHold 전용 로직은 아래 EvaluateFlowHoldY를 사용.
    /// </summary>
    public static float EvaluateY01(FlowLongMeta meta, float t)
    {
        if (meta == null || meta.CurvePoints == null || meta.CurvePoints.Count == 0)
            return Mathf.Clamp01(t);

        t = Mathf.Clamp01(t);

        List<FlowCurvePoint> pts = meta.CurvePoints;

        // 점이 1개면 그대로
        if (pts.Count == 1)
            return pts[0].y01;

        // 점이 2개면 직선
        if (pts.Count == 2)
            return Mathf.Lerp(pts[0].y01, pts[1].y01, t);

        // 점이 3개 이상이면 베지어 곡선
        return EvaluateBezier(pts, t);
    }

    /// <summary>
    /// FlowHold 전용:
    /// FlowLongMeta.CurvePoints 안에 (t:0~1, y01:0~1)가
    /// t 오름차순으로 들어있다는 전제에서 선형 보간으로 y01을 계산.
    /// → 나중에 Spline에서 샘플링한 포인트들을 그대로 넣어 쓰기 좋게 설계.
    /// </summary>
    public static float EvaluateFlowHoldY(FlowLongMeta meta, float t)
    {
        if (meta == null)
            return Mathf.Clamp01(t);

        // 1순위: SampledPoints가 있을 때 (Spline에서 굽힌 결과)
        List<FlowCurvePoint> pts = null;

        if (meta.SampledPoints != null && meta.SampledPoints.Count > 1)
            pts = meta.SampledPoints;
        else
            pts = meta.CurvePoints;

        if (pts == null || pts.Count == 0)
            return Mathf.Clamp01(t);

        t = Mathf.Clamp01(t);

        if (pts.Count == 1)
            return pts[0].y01;

        if (t <= pts[0].t)
            return pts[0].y01;
        if (t >= pts[pts.Count - 1].t)
            return pts[pts.Count - 1].y01;

        for (int i = 0; i < pts.Count - 1; i++)
        {
            var a = pts[i];
            var b = pts[i + 1];

            if (t >= a.t && t <= b.t)
            {
                float segT = Mathf.InverseLerp(a.t, b.t, t);
                return Mathf.Lerp(a.y01, b.y01, segT);
            }
        }

        return pts[pts.Count - 1].y01;
    }

    public static Vector2 EvaluateLocalPos(RectTransform drawRect, float t, float y01)
    {
        float width = drawRect.rect.width;
        float height = drawRect.rect.height;
        Vector2 pivot = drawRect.pivot;

        // t, y01 → 로컬 좌표
        float x = (t * width) - (pivot.x * width);
        float y = (y01 * height) - (pivot.y * height);

        return new Vector2(x, y);
    }

    public static void BuildBezierLine(FlowLongMeta meta, int resolution, List<Vector2> outPoints)
    {
        outPoints.Clear();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;

            float y01 = EvaluateY01(meta, t);

            outPoints.Add(new Vector2(t, y01));
            // t는 X축(0~1), y01은 Y축(0~1)
            // UI 좌표 변환은 이후 별도 처리
        }
    }

    private static float EvaluateBezier(List<FlowCurvePoint> pts, float t)
    {
        if (pts == null || pts.Count == 0)
            return Mathf.Clamp01(t); // fallback

        int n = pts.Count - 1;

        // De Casteljau 알고리즘
        float[] temp = new float[pts.Count];
        for (int i = 0; i < pts.Count; i++)
            temp[i] = pts[i].y01;

        for (int k = 1; k <= n; k++)
        {
            for (int i = 0; i <= n - k; i++)
            {
                temp[i] = Mathf.Lerp(temp[i], temp[i + 1], t);
            }
        }

        return temp[0];
    }

    #endregion
}
