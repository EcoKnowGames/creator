using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;

[NodeWidth(300)]
public class ScenarioNode : Node
{
    [SerializeField] protected string scenarioName;
    [SerializeField] protected string authorName;
    [SerializeField, Multiline] protected string description;
    [SerializeField, HideInInspector] protected string coverImageBase64;
    [SerializeField] protected int rounds;
    [SerializeField] protected int actionsPerRound;
    [SerializeField] protected int startCurrency;
    [SerializeField] protected string calculatorId = CalculatorRegistry.DefaultId;

    private Texture2D coverImageCache;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private Matrix _matrix;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private MapLayout _map;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private string _winConditions;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private string _items;

    public Matrix Matrix
    {
        get
        {
            //Return first valid matrix (legacy compat)
            if ((GetInputPort("_matrix") != null) && (GetInputPort("_matrix").ConnectionCount > 0))
            {
                return GetInputPort("_matrix").GetConnections().Select(x => x.node).OfType<MatrixNode>().First().matrix;
            }

            return null;
        }
    }

    public Matrix[] Matrices
    {
        get
        {
            if ((GetInputPort("_matrix") != null) && (GetInputPort("_matrix").ConnectionCount > 1))
            {
                return GetInputPort("_matrix").GetConnections()
                    .Select(x => x.node).OfType<MatrixNode>()
                    .Select(x => x.matrix).ToArray();
            }

            return null; // Single matrix or none -- use legacy Matrix
        }
    }

    public MapLayout MapLayout => GetInputPort("_map") != null && GetInputPort("_map").GetConnections().Count > 0 ? (MapLayout)GetInputPort("_map").GetInputValue() : null;

    public List<WinConditionRecord> WinConditions
    {
        get
        {
            //Return valid win condition list
            if ((GetInputPort("_winConditions") != null) && (GetInputPort("_winConditions").ConnectionCount > 0))
            {
                return GetInputPort("_winConditions").GetConnections().Select(x => x.node).OfType<WinConditionNode>().Select(x => x.GetWinCondition()).ToList();
            }

            return null;
        }
    }

    public bool HasWinConditions => WinConditions != null && WinConditions.Count > 0;


    public List<Item> ItemDefs
    {
        get
        {
            //Return valid item list
            //This ensures that the item list is always in the "correct" order
            if ((GetInputPort("_items") != null) && (GetInputPort("_items").ConnectionCount > 0))
            {
                return GetInputPort("_items").GetConnections().Select(x => x.node).OfType<ItemNode>().Select(x => x.GetItem()).OrderBy(x => x.ID).ToList();
            }

            return null;
        }
    }

    public bool HasItemDefs => ItemDefs != null && ItemDefs.Count > 0;

    [SerializeField] private int _seed;
    public int Seed => _seed;

    private List<NodePort> _entityPorts = new List<NodePort>();
    public List<NodePort> EntityPorts => _entityPorts;

    // True between Init and OnDisable. xNode's Node.OnEnable runs UpdatePorts before Init, and
    // UpdatePorts clears stale static ports (e.g. _matrix) one connection at a time before
    // re-adding and reconnecting them; connection callbacks during that phase must be ignored.
    [NonSerialized] private bool _initialised;

    public string Name => scenarioName;
    public string Author => string.IsNullOrEmpty(authorName) ? "Unknown Author" : authorName;
    public string Description => string.IsNullOrEmpty(description) ? null : description;
    public string CoverImageBase64
    {
        get
        {
            return coverImageBase64;
        }
        set
        {
            coverImageBase64 = value;
            if (value == null)
            {
                ClearCoverImageCache();
            }
            else
            {
                CacheCoverImage(value);
            }
        }
    }
    public Texture2D CoverImage
    {
        get
        {
            if (coverImageCache == null)
            {
                CacheCoverImage(coverImageBase64);
            }

            return coverImageCache;
        }
    }
    public int TotalRounds => rounds;
    public int ActionsPerRound => actionsPerRound;

    public int StartCurrency => startCurrency;
    public string CalculatorId => string.IsNullOrEmpty(calculatorId) ? CalculatorRegistry.DefaultId : calculatorId;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();

        // _entityPorts is rebuilt each load from xNode's persisted DynamicOutputs.
        // Don't filter by Connection.node — neighbouring nodes may not be deserialised
        // yet at this point, which would silently drop ports whose connections still exist.
        RefreshEntityPortCache();

        _initialised = true;
    }

    // xNode's Node defines no OnDisable, so there is no base implementation to call
    private void OnDisable()
    {
        _initialised = false;
    }

    private void OnValidate()
    {

    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        foreach (NodePort entity in _entityPorts)
        {
            if (entity.fieldName == port.fieldName)
            {
                return entity.fieldName;
            }
        }

        return null;
    }

    // Updates the entity ports in place. Existing ports (and their connections) are never
    // removed and re-added: removing a dynamic port clears its connections, which blanks the
    // connected EntityNode's ID.
    public void ProcessMatrix()
    {
        NodePort matrixPort = GetInputPort("_matrix");
        if (matrixPort == null)
        {
            return;
        }

        int rawCount = matrixPort.ConnectionCount;

        List<MatrixNode> matrixNodes = matrixPort.GetConnections()
            .Select(x => x.node).OfType<MatrixNode>().ToList();

        // Matrices without a parsed CSV contribute no entity IDs
        List<MatrixNode> parsedMatrixNodes = matrixNodes
            .Where(x => x.matrix != null && x.matrix.entityIDs != null && x.matrix.entityIDs.Length > 0)
            .ToList();

        // Union entity IDs across all parsed matrices, in first-seen (CSV) order
        List<string> requiredIDs = new List<string>();
        HashSet<string> requiredSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (MatrixNode matrixNode in parsedMatrixNodes)
        {
            foreach (string id in matrixNode.matrix.entityIDs)
            {
                if (!string.IsNullOrEmpty(id) && requiredSet.Add(id))
                {
                    requiredIDs.Add(id);
                }
            }
        }

        foreach (string id in requiredIDs)
        {
            if (!HasPort(id))
            {
                AddDynamicOutput(typeof(string), fieldName: id, connectionType: ConnectionType.Override);
            }
        }

        // Only remove ports when the full set of required IDs is known: at least one matrix is
        // connected and every connection resolved to a parsed matrix. Otherwise only add.
        // - No matrix connected: never remove. On every load xNode's NodeDataCache.UpdatePorts
        //   clears the static _matrix port when its serialized settings differ from the declared
        //   ones (e.g. old graphs saved with ConnectionType.Override), which fires
        //   OnRemoveConnection with zero connections before re-adding and reconnecting the port.
        //   Removing here would wipe every entity port and blank every EntityNode's ID. As a
        //   result, disconnecting the last matrix deliberately keeps the entity ports and their
        //   connections; export validation still requires a connected matrix.
        // - Unresolved or unparsed matrices (possible while loading): removing would wipe
        //   connections to entities that are still valid.
        bool canRemove = rawCount > 0
            && matrixNodes.Count == rawCount
            && parsedMatrixNodes.Count == matrixNodes.Count;

        if (canRemove)
        {
            foreach (NodePort port in DynamicOutputs.ToList())
            {
                if (!requiredSet.Contains(port.fieldName))
                {
                    RemoveDynamicPort(port);
                }
            }
        }

        RefreshEntityPortCache(requiredIDs);
    }

    // Rebuilds _entityPorts from xNode's persisted DynamicOutputs (the source of truth).
    // When an order is given, ports are sorted by it and any others follow in their existing order.
    public void RefreshEntityPortCache(IList<string> order = null)
    {
        List<NodePort> ports = DynamicOutputs.ToList();

        if (order != null)
        {
            ports = ports.OrderBy(x =>
            {
                int index = order.IndexOf(x.fieldName);
                return index < 0 ? int.MaxValue : index;
            }).ToList();
        }

        _entityPorts = ports;
    }

    public bool DoesMapPopulationCountMatchMatrix()
    {
        if ((MapLayout != null) && (MapLayout.gridDef != null))
        {
            if (!MapLayout.gridDef.HasPopulations())
            {
                return true;
            }
            else
            {
                if (MapLayout.gridDef.GetValidEntityCount() != GetConnectedEntityCount())
                {
                    return false;
                }
            }
        }
        return true;
    }

    #region Entities
    public int GetConnectedEntityCount()
    {
        if (graph is ScenarioNodeGraph scenarioGraph)
        {
            return scenarioGraph.GetEntityList().Count;
        }

        return 0;
    }
    #endregion

    #region Cover Image
    private void CacheCoverImage(string base64)
    {
        if (string.IsNullOrEmpty(base64))
        {
            return;
        }

        byte[] imageBytes = Convert.FromBase64String(base64);
        Texture2D coverImage = new Texture2D(2, 2);
        coverImage.LoadImage(imageBytes);
        coverImageCache = coverImage;
    }

    private void ClearCoverImageCache()
    {
        coverImageCache = null;
    }
    #endregion

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        // Skip while xNode's UpdatePorts (pre-Init) clears a stale _matrix port: with several
        // matrices connected, the partial connection set would remove still-valid entity ports
        if (_initialised && port == GetInputPort("_matrix"))
        {
            ProcessMatrix();
        }
    }
}
