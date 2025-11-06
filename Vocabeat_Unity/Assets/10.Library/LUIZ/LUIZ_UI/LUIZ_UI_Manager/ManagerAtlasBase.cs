using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace LUIZ.UI
{
    public abstract class ManagerAtlasBase : SingletonBase<ManagerAtlasBase>
    {
        private List<SpriteAtlas> m_listSpriteAtlasInstance = new List<SpriteAtlas>();
        private Dictionary<int, Dictionary<string, Sprite>> m_dicSpriteAtlas = new Dictionary<int, Dictionary<string, Sprite>>();

        //--------------------------------------------------------
        protected void ProtMgrAtlasAdd(int category, SpriteAtlas atlas) // 모든 스프라이트는 Clone이며 메모리에 상주한다.
        {
            Dictionary<string, Sprite> dicAtlasSprite = PrivFindOrAllocAtlasCategory(category);
            PrivAtlasAdd(dicAtlasSprite, atlas);
            m_listSpriteAtlasInstance.Add(atlas);
        }

        protected Sprite ProtMgrAtlasFindSprite(int category, string strSpriteName)
        {
            Sprite foundSprite = null;
            Dictionary<string, Sprite> dicAtlasSprite = PrivFindOrAllocAtlasCategory(category);
            if (dicAtlasSprite.ContainsKey(strSpriteName))
            {
                foundSprite = dicAtlasSprite[strSpriteName];
            }
            return foundSprite;
        }

        //-------------------------------------------------------------
        private void PrivAtlasAdd(Dictionary<string, Sprite> dicAtlasSprite, SpriteAtlas atlas)
        {
            Sprite[] arySprite = new Sprite[atlas.spriteCount];
            atlas.GetSprites(arySprite);

            for (int i = 0; i < arySprite.Length; i++)
            {
                dicAtlasSprite[RemoveCloneObjectName(arySprite[i].name)] = arySprite[i];
            }
        }

        private Dictionary<string, Sprite> PrivFindOrAllocAtlasCategory(int category)
        {
            Dictionary<string, Sprite> atlasCategory = null;
            if (m_dicSpriteAtlas.ContainsKey(category))
            {
                atlasCategory = m_dicSpriteAtlas[category];
            }
            else
            {
                atlasCategory = new Dictionary<string, Sprite>();
                m_dicSpriteAtlas[category] = atlasCategory;
            }

            return atlasCategory;
        }
    }
}
