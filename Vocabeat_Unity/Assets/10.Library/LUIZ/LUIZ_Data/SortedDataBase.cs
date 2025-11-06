using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace LUIZ
{
    //지속적으로 업데이트 되는 데이터. SortedDataBase에서 업데이트 때 구독함
    public interface IDataUpdated
    {
        public event Action OnDataUpdate;
    }

    public interface IDataComparer<TCompareType, TData> : IComparer<TData> where TCompareType : Enum
    {
        public TCompareType CompareType { get; }
    }

    //[ 개요 ]
    //정렬 마다 List를 죄다 가지고 있지 않고 한개의 리스트를 이용하여 메모리 이용을 줄임
    //마지막에 소팅된 리스트를 캐싱 및 버전화, 굳이 정렬 필요가 없을때는 중복 정렬을 피함
    //새로운 데이터가 추가되었거나, 기존 데이터가 업데이트 된 이후 첫 데이터 요청 시 소팅한다.

    //[주의 !] 중복 데이터는 검사하지 않기때문에 Add시 유의가 필요함.
    public abstract class SortedDataBase<TCompareType, TData> where TCompareType : Enum where TData : IDataUpdated
    {
        private List<IDataComparer<TCompareType, TData>> m_listDataComparer = new();

        private List<TData> m_listSortedData = new();
        private readonly ReadOnlyCollection<TData> m_listSortedDataReadOnly;

        private TCompareType m_curSortType = default(TCompareType);

        private readonly VersionTracker_Common m_versionTracker = new();
        private int m_curSortVersion = -1; //첨음엔 무조건 버전이 다름
        private int m_sortVersionOffset = 1; //갱신 여부 판단을 위한 버전 오프셋

        //-----------------------------------------------
        protected SortedDataBase()
        {
            m_listSortedDataReadOnly = m_listSortedData.AsReadOnly();
        }
        
        //-------------------------------------------------
        protected void ProtAddDataComparer(IDataComparer<TCompareType, TData> dataComparer)
        {
            m_listDataComparer.Add(dataComparer);
        }

        /// <summary>
        /// 소팅의 기준이 될 버전 오프셋 조절. 기본값 = 1
        /// </summary>
        protected void ProtSetSortVersionOffset(int offset)
        {
            if (offset < 1)
            {
                Debug.LogWarning($"[SortedDataBase] sortVersionOffset must be >= 1. Given: {offset}. Forcing to 1.");
                offset = 1;
            }
            
            m_sortVersionOffset = offset;
        }

        //-------------------------------------------------
        public IVersionTracker GetVersionTracker() => m_versionTracker;
        
        public bool IsSortDirty()
        {
            return Math.Abs(m_curSortVersion - m_versionTracker.GetVersion()) >= m_sortVersionOffset;
        }

        public void ClearAllData()
        {
            foreach (var data in m_listSortedData)
            {
                data.OnDataUpdate -= UpdateSortVersion;
            }

            m_listSortedData.Clear();

            m_curSortType = default(TCompareType);
            m_curSortVersion = Math.Max(-1, m_versionTracker.GetVersion() - m_sortVersionOffset); //강제 갱신 유도
        }

        public void AddData(TData data)
        {
            m_listSortedData.Add(data);

            data.OnDataUpdate -= UpdateSortVersion; // 혹시모를 중복 방지용 안전장치

            //해당 데이터가 업데이트 되거나, 새로운 데이터가 추가 될때 버전 업데이트
            data.OnDataUpdate += UpdateSortVersion;
            UpdateSortVersion();
        }

        /// <summary>
        /// 리스트에서 제거하는 로직이므로 자주 호출하지 말것!!!!!!
        /// </summary>
        public void RemoveData(TData data)
        {
            if (m_listSortedData.Remove(data))
                data.OnDataUpdate -= UpdateSortVersion;
        }

        /// <summary>
        /// 정렬 된 데이터를 받아온다.
        /// </summary>
        public void GetSortedData(TCompareType sortType, out IReadOnlyList<TData> sortedData)
        {
            sortedData = m_listSortedDataReadOnly;
            IDataComparer<TCompareType, TData> dataComparer = FindComparer(sortType);
            if (dataComparer == null)
            {
                Debug.LogError($"[SortedDataBase] dataComparer not Found!! Returning last data... sortType : {sortType}");
                return;
            }

            //동일한 sort 타입의 경우 최근에 정렬한 이후로 데이터의 업데이트가 없었다면 정렬할 필요가 없음
            if (m_curSortType.Equals(sortType))
            {
                //최소 오프셋 보다 버전이 많이 변경 됐을 경우 갱신
                if (IsSortDirty())
                {
                    m_curSortVersion = m_versionTracker.GetVersion();

                    m_listSortedData.Sort(dataComparer);
                }
            }
            else //이전 sort타입과 다르면 무조건 새로 정렬 후 반환
            {
                m_curSortVersion = m_versionTracker.GetVersion();
                m_listSortedData.Sort(dataComparer);
            }

            m_curSortType = sortType;
        }

        //--------------------------------------------------------------
        private IDataComparer<TCompareType, TData> FindComparer(TCompareType compareType)
        {
            foreach (var comparer in m_listDataComparer)
            {
                if (comparer.CompareType.Equals(compareType))
                    return comparer;
            }

            Debug.LogError($"[SortedDataBase] Comparer not found for compareType: {compareType}");
            return null;
        }

        private void UpdateSortVersion()
        {
            m_versionTracker.IncrementVersion();
        }
    }
}
