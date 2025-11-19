using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class NoteEditData
{
    public ENoteType NoteType;
    public Sprite NotePreview;
}

public abstract class NoteEditBase : MonoBehaviour
{
    [Header("이미지")]
    [SerializeField] protected Image ImgPreview;

    [Header("데이터")]
    [SerializeField] protected NoteEditData[] GhostDatas;

    public RectTransform RectTrs => (RectTransform)transform;

    protected NoteEditData GetGhostData(ENoteType noteType)
    {
        foreach (var data in GhostDatas)
        {
            if (data.NoteType == noteType)
                return data;
        }
        return null;
    }

    public virtual void NoteEditVisualSetting(ENoteType noteType)
    {
        NoteEditData data = GetGhostData(noteType);
        if (data == null)
            return;

        ImgPreview.overrideSprite = data.NotePreview;
        gameObject.SetActive(true);
    }

    public virtual void SetNoteEditPosition(Vector2 localPosition)
    {
        RectTrs.anchoredPosition = localPosition;
        gameObject.SetActive(true);
    }
}
