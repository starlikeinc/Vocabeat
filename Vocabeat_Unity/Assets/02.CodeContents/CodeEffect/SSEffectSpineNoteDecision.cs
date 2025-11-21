//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using NSSBattleCore;
//using Spine.Unity;
//using Spine;

//public class SSEffectSpineNoteDecision : CEffectBase
//{
//    [System.Serializable]
//    private class SNoteDecisionInfo
//    {
//        public EArcNoteDecision NoteDecision = EArcNoteDecision.RedBad;
//        public SkeletonGraphic  SpineGraphic = null;
//    }

//    [SerializeField]
//    private List<SNoteDecisionInfo> NoteDecisionList = new List<SNoteDecisionInfo>();

//    //------------------------------------------------------------------
//    protected override void OnEffectInitialize()
//    {
//        base.OnEffectInitialize();

//        for(int i = 0; i < NoteDecisionList.Count; i++)
//        {
//            NoteDecisionList[i].SpineGraphic.gameObject.SetActive(false);
//            NoteDecisionList[i].SpineGraphic.AnimationState.Complete += HandleSpineNoteDecisionFinish;
//        }
//    }

//    //-----------------------------------------------------------------
//    public void DoEffectNoteDecision(EArcNoteDecision eNoteDecision, Vector3 vecWorldPosition, Transform pTrackTransform, UnityAction<CEffectBase> delFinish)
//    {
//        DoEffectStart(delFinish);
//        transform.SetParent(pTrackTransform, false);
//        transform.position = vecWorldPosition;

//        SNoteDecisionInfo pDecision = FindAndArrangeEffectNoteDecision(eNoteDecision);
//        if (pDecision != null)
//        {
//            PrivEffectNoteDecisionStart(pDecision);
//        }
//    }

//    //--------------------------------------------------------------------
//    private SNoteDecisionInfo FindAndArrangeEffectNoteDecision(EArcNoteDecision eNoteDecision)
//    {
//        SNoteDecisionInfo pFindNoteDecision = null;
//        for (int i = 0; i < NoteDecisionList.Count; i++)
//        {
//            SNoteDecisionInfo pNoteDecision = NoteDecisionList[i];
//            if (eNoteDecision == pNoteDecision.NoteDecision)
//            {
//                pFindNoteDecision = NoteDecisionList[i];
//                pNoteDecision.SpineGraphic.gameObject.SetActive(true);
//            }
//            else
//            {
//                pNoteDecision.SpineGraphic.gameObject.SetActive(false);
//            }
//        }
//        return pFindNoteDecision;
//    }

//    private void PrivEffectNoteDecisionStart(SNoteDecisionInfo pDecision)
//    {
//        pDecision.SpineGraphic.AnimationState.SetAnimation(0, pDecision.SpineGraphic.startingAnimation, false);
//    }

//    //------------------------------------------------------------------
//    public void HandleSpineNoteDecisionFinish(TrackEntry pTrackEntry)
//    {
//        DoEffectEnd();
//    }

//}
