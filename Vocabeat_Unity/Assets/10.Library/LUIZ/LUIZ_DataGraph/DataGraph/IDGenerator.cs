using System;

namespace LUIZ.DataGraph
{
    public static class IDGenerator
    {
        public static ulong NewID()
        {
            Span<byte> buffer = stackalloc byte[16];
            Guid.NewGuid().TryWriteBytes(buffer);
            return BitConverter.ToUInt64(buffer); //충돌확률이 그극ㄱ극ㄱ히 낮음.. 따라서 용량 절감을 위해 배열 앞 절반만 떼서 ulong로 변환.
        }
    }
}
