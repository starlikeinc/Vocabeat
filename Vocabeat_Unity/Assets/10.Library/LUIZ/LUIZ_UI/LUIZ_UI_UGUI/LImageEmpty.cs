using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// 이미지와 같은 쓰임세를 가지나 드로우 자체를 하지 않는다.
// 영역을 가지고 포인트 체크를 하나 이미지가 필요하지 않을때 사용, 클릭 블락용 등으로 이용 가능
// RaycastPass에 값을 등록하면 해당 부분은 레이캐스트를 하지 않음
[RequireComponent(typeof(CanvasRenderer))]
public class LImageEmpty : Graphic
{
    [SerializeField] private RectTransform RaycastPass = null;

    //-----------------------------------------------------
    public override void SetMaterialDirty() { return; }
    public override void SetVerticesDirty() { return; }

    ///Rebuild()->UpdateGeometry()->DoMeshGeneration()->OnPopulateMesh()과정으로 오기때문에 호출될 일이 없으나 fail 방지용
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        return;
    }

    //-----------------------------------------------------
    public override bool Raycast(Vector2 screenPosition, Camera eventCamera)
    {
        if (isActiveAndEnabled == false)
            return false;

        bool bBlock = false;
        if (RaycastPass != null)
        {
            bBlock = CheckRayCastPassRect(screenPosition, eventCamera);
        }
        else
        {
            bBlock = base.Raycast(screenPosition, eventCamera);
        }

        return bBlock;
    }

    //-----------------------------------------------------
    private bool CheckRayCastPassRect(Vector2 screenPosition, Camera eventCamera)
    {
        bool bBlock = false;

        if (eventCamera)
        {
            bBlock = !RectTransformUtility.RectangleContainsScreenPoint(RaycastPass, screenPosition, eventCamera);
        }
        else
        {
            bBlock = !RectTransformUtility.RectangleContainsScreenPoint(RaycastPass, screenPosition);
        }

        return bBlock;
    }
}
