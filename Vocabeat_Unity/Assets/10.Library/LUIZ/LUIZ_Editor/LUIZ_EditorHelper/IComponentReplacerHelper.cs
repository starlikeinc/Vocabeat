using System;
using UnityEngine;

namespace LUIZ.Editor
{
    public interface IComponentReplacerHelper
    {
        public bool IsReplacerValid(Type typeBefore, Type typeAfter);
        public void OnDestroyComponentBefore(Component componentBefore);
        public void OnAddComponentAfter(Component componentAfter);
    }
}
