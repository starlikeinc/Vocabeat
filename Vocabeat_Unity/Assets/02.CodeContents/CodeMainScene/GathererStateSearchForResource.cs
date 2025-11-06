using LUIZ.AI.FSM;
using System.Linq;
using UnityEngine;

public class GathererStateSearchForResource : IFSMState
{
    private readonly EntityGatherer m_gatherer;

    //----------------------------------------------
    public GathererStateSearchForResource(EntityGatherer gatherer)
    {
        m_gatherer = gatherer;
    }

    //----------------------------------------------
    public void Update()
    {
        GatherableResource target = PrivChooseRandomResourceNearest(3);
        m_gatherer.SetTargetResource(target);
    }

    //----------------------------------------------
    private GatherableResource PrivChooseRandomResourceNearest(int pickFromNearset)
    {
        return Object.FindObjectsByType<GatherableResource>(FindObjectsSortMode.None)
            .Where(resouce => resouce.IsDepleted == false)
            .OrderBy(resource => Vector3.Distance(m_gatherer.transform.position, resource.transform.position))
            .Take(pickFromNearset)
            .OrderBy(resource => Random.Range(0, int.MaxValue))
            .FirstOrDefault();
    }

    //----------------------------------------------
    public void OnEnter() { }
    public void OnExit() { }
}
