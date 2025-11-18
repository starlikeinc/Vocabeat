using TMPro;
using UnityEngine;

public class LineVertical : MonoBehaviour
{
    [SerializeField] private TMP_Text TextTickCount;

    public void VertLineSetting(int tickCount)
    {        
        UpdateTickCount(tickCount);
        gameObject.SetActive(true);
    }

    public void UpdateTickCount(int tickCount)
    {
        TextTickCount.text = tickCount.ToString();
    }
}
