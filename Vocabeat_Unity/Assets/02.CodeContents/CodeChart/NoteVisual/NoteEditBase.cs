using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class NoteEditData
{
    public ENoteVisualType VisualType;
    public Sprite NoteSymbol;
}

public abstract class NoteEditBase : MonoBehaviour
{
    [Header("이미지")]
    [SerializeField] protected Image ImgPreview;

    [Header("데이터")]
    [SerializeField] protected NoteEditData[] GhostDatas;

    public RectTransform RectTrs => (RectTransform)transform;

    /// <summary>
    /// ENoteVisualType 에 대응하는 프리뷰/고스트 스프라이트 정보 조회
    /// </summary>
    protected NoteEditData GetGhostData(ENoteVisualType visualType)
    {
        if (GhostDatas == null)
            return null;

        for (int i = 0; i < GhostDatas.Length; i++)
        {
            var data = GhostDatas[i];
            if (data != null && data.VisualType == visualType)
                return data;
        }

        return null;
    }

    /// <summary>
    /// 타입을 직접 지정해서 프리뷰 아이콘을 변경 (고스트에서 주로 사용)
    /// </summary>
    public void SetVisualByType(ENoteVisualType visualType)
    {
        var data = GetGhostData(visualType);
        if (data == null || data.NoteSymbol == null)
        {
            gameObject.SetActive(false);
            return;
        }

        ImgPreview.overrideSprite = data.NoteSymbol;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 실제 노트 데이터를 기준으로 프리뷰 아이콘 설정
    /// (페이지에 뿌려지는 NotePreview 용 기본 구현)
    /// </summary>
    public virtual void NoteEditVisualSetting(Note noteData)
    {
        ENoteVisualType visualType = ENoteVisualType.Normal;

        if (noteData != null)
        {
            switch (noteData.NoteType)
            {
                case ENoteType.Normal:
                    visualType = ENoteVisualType.Normal;
                    break;

                case ENoteType.FlowHold:
                    // 에디터 상에선 FlowHold 노트를 "시작 노트"처럼 표시
                    visualType = ENoteVisualType.Place_Start;
                    break;

                default:
                    visualType = ENoteVisualType.Normal;
                    break;
            }
        }

        var data = GetGhostData(visualType);
        if (data == null || data.NoteSymbol == null)
        {
            gameObject.SetActive(false);
            return;
        }

        ImgPreview.overrideSprite = data.NoteSymbol;
        gameObject.SetActive(true);
    }
}
