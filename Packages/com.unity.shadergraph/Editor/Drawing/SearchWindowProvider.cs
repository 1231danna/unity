using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Graphing;
using UnityEditor.Graphing.Util;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine.Profiling;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace UnityEditor.ShaderGraph.Drawing
{
    internal struct NodeEntry
    {
        public string[] title;
        public AbstractMaterialNode node;
        public int compatibleSlotId;
        public string slotName;
    }

    class SearchWindowProvider : IDisposable
    {
        internal EditorWindow m_EditorWindow;
        internal GraphData m_Graph;
        internal GraphView m_GraphView;
        internal Texture2D m_Icon;
        public List<NodeEntry> currentNodeEntries;
        public ShaderPort connectedPort { get; set; }
        public bool nodeNeedsRepositioning { get; set; }
        public SlotReference targetSlotReference { get; internal set; }
        public Vector2 targetPosition { get; internal set; }
        public VisualElement target { get; internal set; }
        public bool regenerateEntries { get; set; }
        private const string k_HiddenFolderName = "Hidden";
        ShaderStageCapability m_ConnectedSlotCapability; // calculated in GenerateNodeEntries

        public void Initialize(EditorWindow editorWindow, GraphData graph, GraphView graphView)
        {
            m_EditorWindow = editorWindow;
            m_Graph = graph;
            m_GraphView = graphView;
            GenerateNodeEntries();

            // Transparent icon to trick search window into indenting items
            m_Icon = new Texture2D(1, 1);
            m_Icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            m_Icon.Apply();
        }

        public void Dispose()
        {
            if (m_Icon != null)
            {
                Object.DestroyImmediate(m_Icon);
                m_Icon = null;
            }

            m_EditorWindow = null;
            m_Graph = null;
            m_GraphView = null;
            connectedPort = null;

            currentNodeEntries?.Clear();
            currentNodeEntries = null;
        }

        List<int> m_Ids;
        List<MaterialSlot> m_Slots = new List<MaterialSlot>();

        public void GenerateNodeEntries()
        {
            Profiler.BeginSample("SearchWindowProvider.GenerateNodeEntries");
            // First build up temporary data structure containing group & title as an array of strings (the last one is the actual title) and associated node type.
            List<NodeEntry> nodeEntries = new List<NodeEntry>();

            bool hideCustomInterpolators = m_Graph.activeTargets.All(at => at.ignoreCustomInterpolators);

            if (connectedPort != null)
            {
                var slot = connectedPort.slot;

                // Precalculate slot compatibility to avoid traversing graph for every added entry.
                m_ConnectedSlotCapability = slot.stageCapability;
                if (m_ConnectedSlotCapability == ShaderStageCapability.All || slot.owner is SubGraphNode)
                {
                    m_ConnectedSlotCapability = NodeUtils.GetEffectiveShaderStageCapability(slot, true)
                        & NodeUtils.GetEffectiveShaderStageCapability(slot, false);
                }
            }
            else
            {
                m_ConnectedSlotCapability = ShaderStageCapability.All;
            }

            if (target is ContextView contextView)
            {
                // Iterate all BlockFieldDescriptors currently cached on GraphData
                foreach (var field in m_Graph.blockFieldDescriptors)
                {
                    if (field.isHidden)
                        continue;

                    // Test stage
                    if (field.shaderStage != contextView.contextData.shaderStage)
                        continue;

                    // Create title
                    List<string> title = ListPool<string>.Get();
                    if (!string.IsNullOrEmpty(field.path))
                    {
                        var path = field.path.Split('/').ToList();
                        title.AddRange(path);
                    }
                    title.Add(field.displayName);

                    // Create and initialize BlockNode instance then add entry
                    var node = (BlockNode)Activator.CreateInstance(typeof(BlockNode));
                    node.Init(field);
                    AddEntries(node, title.ToArray(), nodeEntries);
                }

                SortEntries(nodeEntries);

                if (contextView.contextData.shaderStage == ShaderStage.Vertex && !hideCustomInterpolators)
                {
                    var customBlockNodeStub = (BlockNode)Activator.CreateInstance(typeof(BlockNode));
                    customBlockNodeStub.InitCustomDefault();
                    AddEntries(customBlockNodeStub, new string[] { "Custom Interpolator" }, nodeEntries);
                }

                currentNodeEntries = nodeEntries;
                return;
            }


            Profiler.BeginSample("SearchWindowProvider.GenerateNodeEntries.IterateKnowNodes");
            foreach (var type in NodeClassCache.knownNodeTypes)
            {
                if ((!type.IsClass || type.IsAbstract)
                    || type == typeof(PropertyNode)
                    || type == typeof(KeywordNode)
                    || type == typeof(DropdownNode)
                    || type == typeof(SubGraphNode))
                    continue;

                TitleAttribute titleAttribute = NodeClassCache.GetAttributeOnNodeType<TitleAttribute>(type);
                if (titleAttribute != null)
                {
                    var node = (AbstractMaterialNode)Activator.CreateInstance(type);
                    if (!node.ExposeToSearcher)
                        continue;

                    if (ShaderGraphPreferences.allowDeprecatedBehaviors && node.latestVersion > 0)
                    {
                        var versions = node.allowedNodeVersions ?? Enumerable.Range(0, node.latestVersion + 1);
                        bool multiple = (versions.Count() > 1);
                        foreach (int i in versions)
                        {
                            var depNode = (AbstractMaterialNode)Activator.CreateInstance(type);
                            depNode.ChangeVersion(i);
                            if (multiple)
                                AddEntries(depNode, titleAttribute.title.Append($"v{i}").ToArray(), nodeEntries);
                            else
                                AddEntries(depNode, titleAttribute.title, nodeEntries);
                        }
                    }
                    else
                    {
                        AddEntries(node, titleAttribute.title, nodeEntries);
                    }
                }
            }
            Profiler.EndSample();


            Profiler.BeginSample("SearchWindowProvider.GenerateNodeEntries.IterateSubgraphAssets");
            foreach (var asset in NodeClassCache.knownSubGraphAssets)
            {
                if (asset == null)
                    continue;

                var node = new SubGraphNode { asset = asset };
                var title = asset.path.Split('/').ToList();

                if (asset.descendents.Contains(m_Graph.assetGuid) || asset.assetGuid == m_Graph.assetGuid)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(asset.path))
                {
                    AddEntries(node, new string[1] { asset.name }, nodeEntries);
                }
                else if (title[0] != k_HiddenFolderName)
                {
                    title.Add(asset.name);
                    AddEntries(node, title.ToArray(), nodeEntries);
                }
            }
            Profiler.EndSample();


            Profiler.BeginSample("SearchWindowProvider.GenerateNodeEntries.IterateGraphInputs");
            foreach (var property in m_Graph.properties)
            {
                if (property is Serialization.MultiJsonInternal.UnknownShaderPropertyType)
                    continue;

                var node = new PropertyNode();
                node.property = property;
                AddEntries(node, new[] { "Properties", "Property: " + property.displayName }, nodeEntries);
            }
            foreach (var keyword in m_Graph.keywords)
            {
                var node = new KeywordNode();
                node.keyword = keyword;
                AddEntries(node, new[] { "Keywords", "Keyword: " + keyword.displayName }, nodeEntries);
            }
            foreach (var dropdown in m_Graph.dropdowns)
            {
                var node = new DropdownNode();
                node.dropdown = dropdown;
                AddEntries(node, new[] { "Dropdowns", "dropdown: " + dropdown.displayName }, nodeEntries);
            }
            if (!hideCustomInterpolators)
            {
                foreach (var cibnode in m_Graph.vertexContext.blocks.Where(b => b.value.isCustomBlock))
                {
                    var node = Activator.CreateInstance<CustomInterpolatorNode>();
                    node.ConnectToCustomBlock(cibnode.value);
                    AddEntries(node, new[] { "Custom Interpolator", cibnode.value.customName }, nodeEntries);
                }
            }
            Profiler.EndSample();

            SortEntries(nodeEntries);
            currentNodeEntries = nodeEntries;
            Profiler.EndSample();
        }

        void SortEntries(List<NodeEntry> nodeEntries)
        {
            // Sort the entries lexicographically by group then title with the requirement that items always comes before sub-groups in the same group.
            // Example result:
            // - Art/BlendMode
            // - Art/Adjustments/ColorBalance
            // - Art/Adjustments/Contrast
            nodeEntries.Sort((entry1, entry2) =>
            {
                for (var i = 0; i < entry1.title.Length; i++)
                {
                    if (i >= entry2.title.Length)
                        return 1;
                    var value = entry1.title[i].CompareTo(entry2.title[i]);
                    if (value != 0)
                    {
                        // Make sure that leaves go before nodes
                        if (entry1.title.Length != entry2.title.Length && (i == entry1.title.Length - 1 || i == entry2.title.Length - 1))
                        {
                            //once nodes are sorted, sort slot entries by slot order instead of alphebetically
                            var alphaOrder = entry1.title.Length < entry2.title.Length ? -1 : 1;
                            var slotOrder = entry1.compatibleSlotId.CompareTo(entry2.compatibleSlotId);
                            return alphaOrder.CompareTo(slotOrder);
                        }

                        return value;
                    }
                }
                return 0;
            });
        }

        void AddEntries(AbstractMaterialNode node, string[] title, List<NodeEntry> addNodeEntries)
        {
            if (m_Graph.isSubGraph && !node.allowedInSubGraph)
                return;
            if (!m_Graph.isSubGraph && !node.allowedInMainGraph)
                return;
            if (connectedPort == null)
            {
                addNodeEntries.Add(new NodeEntry
                {
                    node = node,
                    title = title,
                    compatibleSlotId = -1
                });
                return;
            }

            var connectedSlot = connectedPort.slot;
            m_Slots.Clear();
            node.GetSlots(m_Slots);

            foreach (var slot in m_Slots)
            {
                if (!slot.IsCompatibleWith(connectedSlot))
                {
                    continue;
                }

                if (!slot.IsCompatibleStageWith(m_ConnectedSlotCapability))
                {
                    continue;
                }

                //var entryTitle = new string[title.Length];
                //title.CopyTo(entryTitle, 0);
                //entryTitle[entryTitle.Length - 1] += ": " + slot.displayName;
                addNodeEntries.Add(new NodeEntry
                {
                    title = title,
                    node = node,
                    compatibleSlotId = slot.id,
                    slotName = slot.displayName
                });
            }
        }
    }
    class SearcherProvider : SearchWindowProvider
    {
        const string k_SearcherDisabledTitle = "Shader Graph Search Disabled";
        const string k_SearcherDisabledMessage =
            "Shader Graph search is temporarily disabled because com.unity.searcher could not be restored in the current environment.\n\n" +
            "Node creation from the search window and dragged-edge quick creation are unavailable until that package is restored.";

        static bool s_HasShownDisabledDialog;

        public void NotifySearcherUnavailable()
        {
            if (s_HasShownDisabledDialog || m_EditorWindow == null)
                return;

            s_HasShownDisabledDialog = true;
            EditorUtility.DisplayDialog(k_SearcherDisabledTitle, k_SearcherDisabledMessage, "OK");
        }
    }
}
