using System;

namespace LUIZ.Patcher
{
    public enum EPatchErrorType
    {
        NotInitialized,         // 초기화가 되어 있지 않다.
        AlreadyPatchProcess,    // 패치 진행중
        NotEnoughDiskSpace,     // 저장공간 부족
        NetworkDisable,         // 인터넷이 연결되지 않았다.
        CatalogUpdateFail,      // 내부적으로 카탈로그 업데이트가 실패 했다.
        PatchFail,              // 패치가 실패 했다.
        HTTPError,              // 404 등 프로토콜 에러가 발생했다.
        WebRequestError,        // WebRequestDownload 에서 에러가 발생했다.
    }

    public interface IPatcherHandle
    {
        public event Action PatchInitComplete;
        // 패치이름, 다운로드된 바이트, 전체 바이트, 전체 다운로드 비율, 로드된 에셋번들, 로드할 전체 에셋
        public event Action<string, long, long, float, int, int> PatchProgress;
        public event Action<EPatchErrorType, string> PatchError;
        public event Action PatchFinish;
        public event Action<string> PatchLabelStart;
    }
}