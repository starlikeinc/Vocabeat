using System.Collections.Generic;
using System.Linq;
using LUIZ.DataGraph;
using LUIZ.DataGraph.Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleGraphJsonExporter", menuName = "LUIZ/DataGraph/JsonExporters/SampleGraphExporter")]
public class SampleGraphJsonExporter : DataGraphJsonExporterBase
{
    public override void ExportGraphToJson(DataGraph graph)
    {
	    //개선 해야하기는 하는데 일단 이런식
        if (graph == null)
        {
            Debug.LogError("[SampleGraphJsonExporter] Can't Convert Graph. Graph is null.");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Save JSON", "", "SampleGraph.json", "json");
        if (string.IsNullOrEmpty(path))
        {
	        Debug.LogError("[SampleGraphJsonExporter] path is null.");
	        return;
        }

        string jsonData = CreateJsonData(graph);
        System.IO.File.WriteAllText(path, jsonData);
        Debug.Log("Export Success!!! Saved to: " + path);
    }

    private string CreateJsonData(DataGraph graph)
    {
        var listAllNodes = graph.Nodes.OfType<SampleMainNode>().ToList();
        var hashSetReferenced = new HashSet<ulong>();

        var listJson = new List<SampleMainJson>();

        foreach (var sampleNode in listAllNodes)
        {
            var nextNodes = graph.GetOutgoingNodes(sampleNode).OfType<SampleMainNode>().ToList();
            foreach (var q in nextNodes)
                hashSetReferenced.Add(q.NodeID);

            listJson.Add(new SampleMainJson
            {
                ID = sampleNode.NodeID,
                Info = new SampleMainInfo
                {
                    Name = sampleNode.Name,
                    Description = sampleNode.Description,
                    ListRewards = sampleNode.ListItems,
                },
                ListNextNodeIDs = nextNodes.Select(q => q.NodeID).ToList(),
            });
        }

        var startNode = listAllNodes.FirstOrDefault(n => !hashSetReferenced.Contains(n.NodeID));
        var graphJson = new SampleGraphJson
        {
            StartMainNodeID = startNode?.NodeID ?? 0UL,
            ListMainNodes = listJson
        };

        return JsonConvert.SerializeObject(graphJson, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        });
    }
}
