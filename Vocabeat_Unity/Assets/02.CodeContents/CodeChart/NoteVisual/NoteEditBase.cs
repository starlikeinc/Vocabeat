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

    protected NoteEditData GetGhostData(ENoteVisualType visualType)
    {
        foreach (var data in GhostDatas)
        {
            if (data.VisualType == visualType)
                return data;
        }
        return null;
    }

    public virtual void NoteEditVisualSetting(Note noteData)
    {
        // TODO : 노트 프리뷰 보여주는 로직 바꿔야함.

        ENoteVisualType visualType = ENoteVisualType.Normal; // 이거 아님


        NoteEditData data = GetGhostData(visualType);
        if (data == null)
            return;

        ImgPreview.overrideSprite = data.NoteSymbol;
        gameObject.SetActive(true);
    }
}
