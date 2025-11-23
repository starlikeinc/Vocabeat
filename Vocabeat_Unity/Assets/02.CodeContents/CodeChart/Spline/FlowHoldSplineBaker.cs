using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

/// <summary>
/// FlowLongMeta.CurvePoints (t, y01) <-> Unity Spline 변환 헬퍼
/// 
/// Plan A:
/// - 런타임은 FlowLongMeta만 사용.
/// - Spline은 채보 편집용으로 잠깐 만들어서 사용 후 CurvePoints에 굽기.
/// </summary>
public static class FlowHoldSplineBaker
{
    /// <summary>
    /// Note.FlowLongMeta.CurvePoints 를 기반으로 spline을 구성한다.
    /// CurvePoints가 없으면 "직선"을 구성해준다.
    /// </summary>
    public static void BuildSplineFromMeta(Note note, Spline spline)
    {
        spline.Clear();

        if (note == null)
            return;

        FlowLongMeta meta = note.FlowLongMeta;
        float baseY = note.Y;

        // CurvePoints가 없다면 "t:0, t:1" 기준의 직선 하나
        if (meta == null || meta.CurvePoints == null || meta.CurvePoints.Count == 0)
        {
            var start = new BezierKnot(new float3(0f, baseY, 0f));
            var end = new BezierKnot(new float3(1f, baseY, 0f));

            spline.Add(start);
            spline.Add(end);
            return;
        }

        List<FlowCurvePoint> pts = meta.CurvePoints;
        pts.Sort((a, b) => a.t.CompareTo(b.t));

        // t,y01을 그대로 0~1 좌표계에서 사용
        for (int i = 0; i < pts.Count; i++)
        {
            var cp = pts[i];
            float t = Mathf.Clamp01(cp.t);
            float y = Mathf.Clamp01(cp.y01);

            var knot = new BezierKnot(new float3(t, y, 0f));
            spline.Add(knot);
        }
    }

    /// <summary>
    /// Spline을 0~1 구간에서 샘플링해서 Note.FlowLongMeta.CurvePoints 에 굽는다.
    /// sampleCount가 많을수록 곡선이 부드러워지지만 데이터 양도 늘어난다.
    /// </summary>
    public static void BakeMetaFromSpline(Note note, Spline spline, int sampleCount)
    {
        if (note == null || spline == null)
            return;

        if (sampleCount < 2)
            sampleCount = 2;

        if (note.FlowLongMeta == null)
            note.FlowLongMeta = new FlowLongMeta();

        var meta = note.FlowLongMeta;

        // 리스트 준비
        meta.CurvePoints ??= new List<FlowCurvePoint>();
        meta.SampledPoints ??= new List<FlowCurvePoint>();

        // 1) 컨트롤 포인트(CurvePoints) 갱신: Spline의 Knot들을 그대로 저장
        meta.CurvePoints.Clear();
        foreach (var knot in spline.Knots)
        {
            var pos = knot.Position;   // (x=t, y=y01)로 쓰는 구조라고 가정
            float t = Mathf.Clamp01(pos.x);
            float y = Mathf.Clamp01(pos.y);

            meta.CurvePoints.Add(new FlowCurvePoint
            {
                t = t,
                y01 = y
            });
        }

        // t 기준으로 정렬
        meta.CurvePoints.Sort((a, b) => a.t.CompareTo(b.t));

        // 2) SampledPoints에 Spline 샘플 채우기 (실제 곡선용)
        meta.SampledPoints.Clear();

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (sampleCount == 1) ? 0f : i / (float)(sampleCount - 1);

            float3 pos = spline.EvaluatePosition(t);
            float y01 = Mathf.Clamp01(pos.y);

            meta.SampledPoints.Add(new FlowCurvePoint
            {
                t = t,
                y01 = y01
            });
        }
    }
}
