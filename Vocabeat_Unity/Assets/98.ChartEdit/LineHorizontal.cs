using TMPro;
using UnityEngine;

public class LineHorizontal : MonoBehaviour
{
    [SerializeField] private TMP_Text TextPosYValue;

    public void LineHorizonSetting(float posYValue)
    {
        TextPosYValue.text = posYValue.ToString("F2");
        gameObject.SetActive(true);
    }
}
