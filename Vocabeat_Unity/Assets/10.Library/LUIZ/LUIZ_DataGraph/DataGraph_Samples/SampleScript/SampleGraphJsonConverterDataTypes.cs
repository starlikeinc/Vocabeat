using System.Collections.Generic;

[System.Serializable]
public class SampleGraphJson
{
	public ulong StartMainNodeID;
	public List<SampleMainJson> ListMainNodes;
}

[System.Serializable]
public class SampleMainJson
{
	public ulong ID; // NodeID
	public SampleMainInfo Info;
	public List<ulong> ListNextNodeIDs;
	public List<SampleStepJson> ListSteps;
}

[System.Serializable]
public class SampleMainInfo
{
	public string Name;
	public string Description;
	public List<SampleItemData> ListRewards;
}

[System.Serializable]
public class SampleStepJson
{
	public ulong ID; // NodeID
	public string Description;
	public List<ulong> ListNextStepIDs;
	public SampleConditionJson Condition;
}

[System.Serializable]
public class SampleConditionJson
{
	public List<SampleItemData> ListConditions;
}