using UnityEngine;

namespace LUIZ.UI
{
    public interface IScrollSnapJumpHelper
    {
        public Vector2 GetNextMovePosition(Vector2 startPos, Vector2 destPos, float timePercent, float curvePercent);
    }

    public class UIScrollSnapHelper_Default : IScrollSnapJumpHelper
    {
        public Vector2 GetNextMovePosition(Vector2 startPos, Vector2 destPos, float timePercent, float curveValue)
        {
            Vector2 nextPos = Vector2.Lerp(startPos, destPos, curveValue);
            return nextPos;
        }
    }
}