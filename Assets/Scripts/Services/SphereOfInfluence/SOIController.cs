using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SOIController : MonoBehaviour
{
    public static SOIController Instance { get; private set; }

    [Header("References")]
    [Tooltip("The rocket whose SOI will be tracked")]
    public Transform ActiveRocket;

    private readonly Dictionary<string, CelestialBodyNode> _nodes = new Dictionary<string, CelestialBodyNode>();
    private readonly List<CelestialBodyNode> _rootBodies = new List<CelestialBodyNode>();

    private readonly Dictionary<string, OrbitData> _savedOrbitData = new Dictionary<string, OrbitData>();
    private readonly Dictionary<string, Transform> _savedAttractors = new Dictionary<string, Transform>();

    private CelestialBodyNode _currentSOI;
    private List<CelestialBodyNode> _previousChain = new List<CelestialBodyNode>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable() => BuildHierarchy();

    /// <summary>
    /// Будує дерево SOI за виключенням об'єктів з тегом Rocket
    /// Тіла, що мають AttractorObject == null вважаються кореневими
    /// </summary>
    public void BuildHierarchy()
    {
        _nodes.Clear();
        _rootBodies.Clear();

        var movers = FindObjectsOfType<OrbitMover>()
            .Where(m => m.transform != ActiveRocket && !m.CompareTag("Rocket"));

        foreach (var m in movers)
            _nodes[m.gameObject.name] = new CelestialBodyNode(m.gameObject.name, m.transform);

        foreach (var m in movers)
        {
            var node = _nodes[m.gameObject.name];
            var attractor = m.AttractorSettings?.AttractorObject;
            if (attractor != null)
            {
                var parentMover = attractor.GetComponentInParent<OrbitMover>();
                if (parentMover != null && _nodes.TryGetValue(parentMover.gameObject.name, out var pNode))
                {
                    pNode.AddChild(node);
                    continue;
                }
            }
            _rootBodies.Add(node);
        }
    }

    private void Update()
    {
        if (ActiveRocket == null) return;
        var newSOI = DetectNewSOI();
        if (newSOI == _currentSOI) return;
        Debug.Log($"[SOI] Switched to: {newSOI?.Name ?? "None"}");
        HandleSOIChange(newSOI);
        _currentSOI = newSOI;
    }

    private CelestialBodyNode DetectNewSOI()
    {
        CelestialBodyNode best = null;
        double bestDist = double.MaxValue;

        foreach (var node in _nodes.Values)
        {
            var mover = node.Transform.GetComponent<OrbitMover>();
            if (mover == null) continue;
            double dist = Vector3.Distance(ActiveRocket.position, node.Transform.position);
            double soi = mover.orbitData.SphereOfInfluenceRadius;
            if (dist <= soi && dist < bestDist)
            {
                bestDist = dist;
                best = node;
            }
        }
        return best;
    }

    private void HandleSOIChange(CelestialBodyNode newSOI)
    {
        RestorePreviousChain();
        var newChain = BuildChain(newSOI);
        SaveChainData(newChain);
        ApplyNewChainSettings(newChain, newSOI);
        _previousChain = newChain;
    }

    private void RestorePreviousChain()
    {
        foreach (var node in _previousChain)
        {
            var mover = node.Transform.GetComponent<OrbitMover>();
            if (mover == null) continue;

            if (_savedOrbitData.TryGetValue(node.Name, out var data))
                CopyOrbitData(data, mover.orbitData);

            if (mover.AttractorSettings != null && _savedAttractors.TryGetValue(node.Name, out var orig))
                mover.AttractorSettings.AttractorObject = orig;

            mover.enabled = true;
        }
    }

    private List<CelestialBodyNode> BuildChain(CelestialBodyNode soiNode)
    {
        var chain = new List<CelestialBodyNode>();
        for (var n = soiNode; n != null; n = FindParent(n))
            chain.Add(n);
        chain.Reverse();
        return chain;
    }

    private void SaveChainData(List<CelestialBodyNode> chain)
    {
        foreach (var node in chain)
        {
            var mover = node.Transform.GetComponent<OrbitMover>();
            if (mover == null || _savedOrbitData.ContainsKey(node.Name)) continue;

            _savedOrbitData[node.Name] = CloneOrbitData(mover.orbitData);
            if (mover.AttractorSettings != null)
                _savedAttractors[node.Name] = mover.AttractorSettings.AttractorObject;
        }
    }

    private void ApplyNewChainSettings(List<CelestialBodyNode> chain, CelestialBodyNode newSOI)
    {
        for (int i = 0; i < chain.Count; i++)
        {
            var node = chain[i];
            var mover = node.Transform.GetComponent<OrbitMover>();
            if (mover == null) continue;

            mover.enabled = (node != newSOI);
            Transform next = (i + 1 < chain.Count) ? chain[i + 1].Transform : null;
            if (mover.AttractorSettings != null)
                mover.AttractorSettings.AttractorObject = next;

            if (next != null && _savedOrbitData.TryGetValue(chain[i + 1].Name, out var srcData))
            {
                CopyOrbitData(srcData, mover.orbitData);
                Vector3 worldPos = node.Transform.position;
                Vector3 attrWorldPos = next.position;
                mover.orbitData.positionRelativeToAttractor = new Vector3d(
                    worldPos.x - attrWorldPos.x,
                    worldPos.y - attrWorldPos.y,
                    worldPos.z - attrWorldPos.z);
            }

            double m = mover.orbitData.CalculateMeanAnomalyFromPosition();
            mover.orbitData.SetMeanAnomaly(m);
        }
    }

    private OrbitData CloneOrbitData(OrbitData src)
    {
        var dst = new OrbitData();
        foreach (var f in typeof(OrbitData).GetFields())
            f.SetValue(dst, f.GetValue(src));
        return dst;
    }

    private void CopyOrbitData(OrbitData src, OrbitData dst)
    {
        foreach (var f in typeof(OrbitData).GetFields())
            f.SetValue(dst, f.GetValue(src));
    }

    private CelestialBodyNode FindParent(CelestialBodyNode child)
    {
        return _nodes.Values.FirstOrDefault(n => n.GetChildren().Contains(child));
    }
}
