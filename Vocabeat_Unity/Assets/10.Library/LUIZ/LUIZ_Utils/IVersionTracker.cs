using UnityEngine;

namespace LUIZ
{
    //DB나 로컬 데이터 등에서 버전 검사 후 갱신 로직 등을 처리 할때 공용으로 이용하기 위함
    //ex) 유닛 스크롤 쪽에서 최초에 DB데이터를 통해 갱신후 DB의 버전 캐싱, 이후 스크롤 화면 재진입시 이전 캐싱버전과 현재 DB버전 비교 후 다르면 갱신하는 등..
    public interface IVersionTracker
    {
        public int GetVersion();
    }

    public interface IVersionTrackable
    {
        public IVersionTracker GetVersionTracker();
    }

    //----------------------------------------------------------
    public class VersionTracker_Common : IVersionTracker
    {
        private int m_version = -1;
        
        //--------------------------------------
        public int GetVersion() => m_version;

        //--------------------------------------
        public void IncrementVersion()
        {
            if (m_version == -1) //비교 로직 실수 방지를 위해 최초에는 -1로 세팅
                m_version = 0;
            else if (m_version == int.MaxValue)
                m_version = 0;
            else
                m_version++;
        }
    }
}