using System;
using UnityEngine;
using UnityEngine.UI;

//추가 기능이 필요할 경우 프로젝트에서 추가로 상속받아 기능 확장할 것
[AddComponentMenu("UI_LUIZ/LText", 10)]
public class LText : Text
{
    //로컬라이징 테이블이나 차트같이 다양한 참조 변환을 위한 브릿지
    private Func<string, string> m_funcTextRef = null;

    //---------------------------------------------------
    public override string text
    {
        get => base.text;

        set
        {
            if (m_funcTextRef != null)
            {
                base.text = m_funcTextRef(value);
            }
            else
            {
                base.text = value;
            }
        }
    }

    //---------------------------------------------------
    public void SetTextReference(Func<string, string> funcTextRef)
    {
        m_funcTextRef = funcTextRef;
    }

    public void SetFontData(FontData fontData)
    {
        font = fontData.font;
        fontSize = fontData.fontSize;
        fontStyle = fontData.fontStyle;
        lineSpacing = fontData.lineSpacing;
        supportRichText = fontData.richText;
        alignment = fontData.alignment;
        horizontalOverflow = fontData.horizontalOverflow;
        verticalOverflow = fontData.verticalOverflow;
        resizeTextForBestFit = fontData.bestFit;
        alignByGeometry = fontData.alignByGeometry;
    }
}
