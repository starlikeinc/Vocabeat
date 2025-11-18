using TMPro;
using UnityEngine;

public class LineVertical : MonoBehaviour
{
    [SerializeField] private TMP_Text TextTickCount;

    public void VertLineSetting(int tickCount)
    {
        TextTickCount.text = tickCount.ToString();
        gameObject.SetActive(true);
    }
}
