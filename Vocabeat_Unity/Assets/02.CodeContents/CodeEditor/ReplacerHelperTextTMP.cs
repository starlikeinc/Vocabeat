using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LUIZ.Editor;

//Text <-> TextMeshProUGUI 간의 변환 헬퍼
public class ReplacerHelperTextTMP : IComponentReplacerHelper
{
    private string m_stringContent = string.Empty;

    //----------------------------------------------------------
    public bool IsReplacerValid(Type typeBefore, Type typeAfter)
    {
        bool isValid = false;

        if (typeBefore == typeof(Text) && typeAfter == typeof(TextMeshProUGUI))
        {
            isValid = true;
        }

        if (typeBefore == typeof(TextMeshProUGUI) && typeAfter == typeof(Text))
        {
            isValid = true;
        }

        return isValid;
    }

    public void OnDestroyComponentBefore(Component typeBefore)
    {
        if (typeBefore is Text text)
        {
            m_stringContent = text.text;
        }
        else if (typeBefore is TextMeshProUGUI tmp)
        {
            m_stringContent = tmp.text;
        }
    }

    public void OnAddComponentAfter(Component typeAfter)
    {
        if (typeAfter is Text text)
        {
            text.text = m_stringContent;
        }
        else if (typeAfter is TextMeshProUGUI tmp)
        {
            tmp.text = m_stringContent;
        }
    }
}
