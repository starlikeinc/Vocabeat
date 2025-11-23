using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// 채보 에디터에서만 사용하는 FlowHold Spline 편집 컨텍스트.
///
/// - SplineContainer 하나를 들고 있음.
/// - 특정 Note를 "현재 편집 대상"으로 잡고
///   BuildSplineFromMeta / BakeMetaFromSpline 을 호출.
/// - 차트 화면(ChartVisualizer)은 여전히 FlowLongMeta.CurvePoints만 보고 그림.
/// </summary>
public class FlowHoldSplineContext : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer _splineContainer;

    [Header("Bake Settings")]
    [Min(2)]
    [SerializeField] private int _sampleCount = 24;

    private Note _editingNote;

    public bool HasEditingNote => _editingNote != null;
    public Note EditingNote => _editingNote;

    public Spline Spline => _splineContainer != null ? _splineContainer.Spline : null;

    /// <summary>
    /// 이 Note를 대상으로 Spline 편집을 시작한다.
    /// (기존 FlowLongMeta.CurvePoints 기준으로 Spline 구성)
    /// </summary>
    public void BeginEdit(Note targetNote)
    {
        _editingNote = targetNote;

        if (_splineContainer == null || targetNote == null)
            return;

        var spline = _splineContainer.Spline;
        FlowHoldSplineBaker.BuildSplineFromMeta(targetNote, spline);
    }

    /// <summary>
    /// 현재 Spline 상태를 FlowLongMeta.CurvePoints에 굽는다.
    /// (BeginEdit를 호출해둔 Note에 반영)
    /// </summary>
    public void BakeToMeta()
    {
        if (_editingNote == null || _splineContainer == null)
            return;

        var spline = _splineContainer.Spline;
        FlowHoldSplineBaker.BakeMetaFromSpline(_editingNote, spline, _sampleCount);
    }

    /// <summary>
    /// 편집 취소 / 다른 노트 편집으로 전환할 때 호출.
    /// </summary>
    public void ClearEditing()
    {
        _editingNote = null;

        if (_splineContainer != null)
            _splineContainer.Spline.Clear();
    }
}
