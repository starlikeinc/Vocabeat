using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using LUIZ.UI;
using LUIZ.Collections;

public class ManagerAtlas : ManagerAtlasBase
{
    public static new ManagerAtlas Instance { get { return ManagerAtlasBase.Instance as ManagerAtlas; } }

    //-----------------------------------------------------------------------
    public enum EAtlasCategory
    {
        AtlasItem,
        AtlasSkillIcon,
        AtlasBuffIcon,
    }

    [SerializeField] private SerializableDictionary<EAtlasCategory, SpriteAtlas> AtlasCategory;

    //---------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        Dictionary<EAtlasCategory, SpriteAtlas>.Enumerator it = AtlasCategory.GetEnumerator();
        while (it.MoveNext())
        {
            ProtMgrAtlasAdd((int)it.Current.Key, it.Current.Value);
        }
    }

    //---------------------------------------------------------------------
    public Sprite FindAtlasItemIcon(string itemIconName)
    {
        return ProtMgrAtlasFindSprite((int)EAtlasCategory.AtlasItem, itemIconName);
    }

    public Sprite FindAtlasSkillIcon(string skillIconName)
    {
        return ProtMgrAtlasFindSprite((int)EAtlasCategory.AtlasSkillIcon, skillIconName);
    }

    public Sprite FindAtlasBuffIcon(string buffIconName)
    {
        return ProtMgrAtlasFindSprite((int)EAtlasCategory.AtlasBuffIcon, buffIconName);
    }
}
