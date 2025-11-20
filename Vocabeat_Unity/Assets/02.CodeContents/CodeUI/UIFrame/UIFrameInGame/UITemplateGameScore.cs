using LUIZ.UI;
using UnityEngine;
using System.Collections.Generic;

public class UITemplateGameScore : UITemplateBase
{
    [Header("Container")]
    [SerializeField] private RectTransform _container;

    [Header("Digit Sprites")]
    [SerializeField] private Sprite sprite0;
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private Sprite sprite3;
    [SerializeField] private Sprite sprite4;
    [SerializeField] private Sprite sprite5;
    [SerializeField] private Sprite sprite6;
    [SerializeField] private Sprite sprite7;
    [SerializeField] private Sprite sprite8;
    [SerializeField] private Sprite sprite9;
    [SerializeField] private Sprite spriteComma;

    private Dictionary<char, Sprite> _map;

    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        _map = new Dictionary<char, Sprite>()
        {
            ['0'] = sprite0,
            ['1'] = sprite1,
            ['2'] = sprite2,
            ['3'] = sprite3,
            ['4'] = sprite4,
            ['5'] = sprite5,
            ['6'] = sprite6,
            ['7'] = sprite7,
            ['8'] = sprite8,
            ['9'] = sprite9,
            [','] = spriteComma,
        };
    }

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        ManagerRhythm.Instance.OnScoreChanged -= SetScore;
        ManagerRhythm.Instance.OnScoreChanged += SetScore;
    }

    public void SetScore(int score)
    {
        DoUITemplateReturnAll();
        
        string formatted = score.ToString("N0"); 

        // 3) 문자 하나씩 이미지 생성
        foreach (char c in formatted)
        {
            if (!_map.ContainsKey(c))
                continue;

            var uiItem = DoTemplateRequestItem<UIItemGameScore>(_container);
            uiItem.SetScoreImage(_map[c], c == ',');
        }
    }
}
