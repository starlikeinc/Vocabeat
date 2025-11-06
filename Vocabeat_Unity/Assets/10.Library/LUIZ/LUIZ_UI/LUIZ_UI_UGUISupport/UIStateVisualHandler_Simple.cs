using UnityEngine;
using UnityEngine.Events;

public class UIStateVisualHandler_Simple : MonoBehaviour
{
    private static StateData s_stateNone = new StateData { StateName = null, OnEventInvoke = null };
    
    [System.Serializable]
    private struct StateData
    {
        public string StateName;
        public UnityEvent OnEventInvoke;
    }

    [SerializeField] private StateData[] StateDataAry;
    
    //---------------------------------------------------
    public bool DoTryInvokeState(string stateName)
    {
        if (TryFindState(stateName, out StateData stateData))
        {
            EventInvoke(stateData);
            return true;
        }

        return false;
    }

    public bool DoTryInvokeState(int stateIndex)
    {
        if (TryFindState(stateIndex, out StateData stateData))
        {
            EventInvoke(stateData);
            return true;
        }

        return false;
    }
    
    //-------------------------------------------------------------------
    //--------------------------------------------------------------
    private bool TryFindState(string stateName, out StateData stateData)
    {
        stateData = s_stateNone;

        for (int i = 0; i < StateDataAry.Length; i++)
        {
            if (StateDataAry[i].StateName == stateName)
            {
                stateData = StateDataAry[i];
                break;
            }
        }

        if (string.IsNullOrEmpty(stateData.StateName))
        {
            Debug.LogError($"[UIStateVisualHandler] State : {stateName} Not found!!! State is not announced or StateName is NULL or Empty");
            return false;
        }

        return true;
    }

    private bool TryFindState(int stateIndex, out StateData stateData)
    {
        stateData = s_stateNone;

        if (stateIndex >= 0 && stateIndex < StateDataAry.Length)
        {
            stateData = StateDataAry[stateIndex];
        }

        if (string.IsNullOrEmpty(stateData.StateName))
        {
            Debug.LogError($"[UIStateVisualHandler] State Index : {stateIndex} Not found!!! State is not announced or StateName is NULL or Empty");
            return false;
        }

        return true;
    }
    private void EventInvoke(StateData stateData)
    {
        if (stateData.OnEventInvoke == null)
            Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Event is null");
        else
        {
            stateData.OnEventInvoke.Invoke();
        }
    }
}
