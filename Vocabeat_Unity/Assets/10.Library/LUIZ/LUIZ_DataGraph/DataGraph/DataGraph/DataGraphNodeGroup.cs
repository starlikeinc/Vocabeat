using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DataGraphNodeGroup
{
    public ulong GroupID;        //식별자
    public string Title;         //그룹 이름
    public Color Color;          //배경 색상
    public Rect Position;        //위치와 크기
    
    public List<ulong> ContainedNodeIDs = new(); //그룹 내 노드 ID 리스트
}
