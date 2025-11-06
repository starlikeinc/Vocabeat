namespace LUIZ
{
    public static class IndexSelectors
    {
        public static readonly IIndexSelector Forward = new IndexSelectorForward();
        public static readonly IIndexSelector Backward = new IndexSelectorBackward();
        public static readonly IIndexSelector Random = new IndexSelectorRandom();
    }
    
    public interface IIndexSelector
    {
        //현재 인덱스와 전체 크기를 제공 받아 다음 인덱스를 반환.
        public int GetNextIndex(int curIdx, int totalSize);
    }

    internal class IndexSelectorForward : IIndexSelector
    {
        public int GetNextIndex(int curIdx, int totalSize)
        {
            if (totalSize <= 0) return 0;
            return (curIdx + 1) % totalSize;
        }
    }
    
    internal class IndexSelectorBackward : IIndexSelector
    {
        public int GetNextIndex(int curIdx, int totalSize)
        {
            if (totalSize <= 0) return 0;
            return (curIdx - 1 + totalSize) % totalSize;
        }
    }
    
    internal class IndexSelectorRandom : IIndexSelector
    {
        public int GetNextIndex(int curIdx, int totalSize)
        {
            if (totalSize <= 0) return 0;

            int nextIdx;
            do
            {
                nextIdx = CommonUtils.LRandom.Next(0, totalSize);
            }
            while (totalSize > 1 && nextIdx == curIdx); // 현재와 다른 값 보장

            return nextIdx;
        }
    }
}