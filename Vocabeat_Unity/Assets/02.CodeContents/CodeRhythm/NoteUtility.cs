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

        float halfWidth = parentWidth * 0.5f;
        float posX = (x01 - 0.5f) * parentWidth;

        Vector2 pos = new();
        pos.x = posX;
        pos.y = (y01 - 0.5f) * parentHeight;

        return pos;
    }

    #region FlowHold Note 관련
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
