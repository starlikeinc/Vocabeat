using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LUIZ;
public class StageBackground : MonoBase
{
    [SerializeField] private List<SpriteRenderer> StageSprites;

    //---------------------------------------------------------------
    public void DoResizeScaleByScreenSize()
    {
        //TODO : 화면 사이즈, 비율에 맞춰 배경을 적당한 크기로 조정해준다.
        Debug.Log("[StageBackground] Prefab Size Resized");
    }
}
