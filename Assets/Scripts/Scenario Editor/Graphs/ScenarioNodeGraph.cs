using System.Linq;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class ScenarioNodeGraph : NodeGraph
{
    /*public override Node AddNode(Type type)
    {
        if (type == typeof(ScenarioNode))
        {
            if (GetScenarioNode() != null)
            {
                Debug.LogError("[NODE EDITOR] ERROR: Trying to add a second Scenario node when one already exists. Aborting node spawn!");
                return null;
            }
        }

        return base.AddNode(type);
    }*/

    //TODO(caspar): A GetScenario with Scenario class makes more sense here
    public ScenarioNode GetScenarioNode()
    {
        return nodes.OfType<ScenarioNode>().FirstOrDefault();
    }

    public MatrixNode GetMatrixNode()
    {
        return nodes.OfType<MatrixNode>().FirstOrDefault();
    }

    public bool HasEntityNodes()
    {
        return nodes.FirstOrDefault(x => x.GetType() == typeof(EntityNode)) != null;
    }

    /*public Scenario GetScenario()
    {
        ScenarioNode node = GetScenarioNode();
        if (node != null)
        {
            return node.GetScenario();
        }

        return null;
    }	*/
}
