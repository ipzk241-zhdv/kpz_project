using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class SOIController : MonoBehaviour
{
    public static SOIController Instance { get; private set; }

    private readonly List<CelestialBodyNode> _rootBodies = new List<CelestialBodyNode>();
    private readonly Dictionary<string, CelestialBodyNode> _nodes = new Dictionary<string, CelestialBodyNode>();

    private CelestialBodyNode _currentSOI;
    public Transform ActiveRocket;

    private List<CelestialBodyNode> _previousChain = new List<CelestialBodyNode>();
    private Dictionary<string, OrbitData> _savedOrbitData = new();
    private Dictionary<string, Transform> _savedAttractors = new();
    private Dictionary<string, Vector3> _savedVelocities = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        BuildHierarchy();
    }

    public void BuildHierarchy()
    {
        _rootBodies.Clear();
        _nodes.Clear();

        foreach (var mover in FindObjectsOfType<OrbitMover>())
        {
            if (ActiveRocket != null && mover.transform == ActiveRocket)
                continue;

            var objName = mover.gameObject.name;
            if (!_nodes.ContainsKey(objName))
            {
                _nodes[objName] = new CelestialBodyNode(objName, mover.transform);
            }
        }

        foreach (var mover in FindObjectsOfType<OrbitMover>())
        {
            if (ActiveRocket != null && mover.transform == ActiveRocket)
                continue;

            Transform nodeTransform = mover.transform;

            bool hasMesh = mover.GetComponent<MeshRenderer>() != null;
            bool hasVelocityChild = mover.transform.Find("velocity") != null;

            if (hasMesh && !hasVelocityChild && mover.transform.parent != null)
            {
                nodeTransform = mover.transform.parent;
            }

            var nodeName = nodeTransform.gameObject.name;
            if (!_nodes.TryGetValue(nodeName, out var node))
            {
                Debug.LogWarning($"Не найден узел {nodeName} при построении дерева SOI");
                continue;
            }

            Transform rawAttractor = mover.AttractorSettings?.AttractorObject;
            Transform attractorTransform = null;
            if (rawAttractor != null)
            {
                bool parentHasMesh = rawAttractor.GetComponent<MeshRenderer>() != null;
                bool parentHasVelocity = rawAttractor.Find("velocity") != null;
                if (parentHasMesh && !parentHasVelocity && rawAttractor.parent != null)
                    attractorTransform = rawAttractor.parent;
                else
                    attractorTransform = rawAttractor;
            }

            if (attractorTransform != null &&
                _nodes.TryGetValue(attractorTransform.gameObject.name, out var parentNode))
            {
                parentNode.AddChild(node);
            }
            else
            {
                if (node.Transform != ActiveRocket)
                {
                    _rootBodies.Add(node);
                }
            }
        }

        foreach (var node in _nodes.Values)
        {
            var mover = node.Transform.GetComponent<OrbitMover>();
            if (mover == null || mover.transform == ActiveRocket) continue;

            double a = mover.orbitData.SemiMajorAxis;
            double m = mover.AttractorSettings.AttractorMass;
            double M = mover.orbitData.AttractorMass;
            node.SphereOfInfluenceRadius = a * Math.Pow(m / M, 0.4);
        }
        Debug.Log("Finished");
    }

    public IReadOnlyList<CelestialBodyNode> GetRootBodies() => _rootBodies;

    public CelestialBodyNode FindBody(string name)
    {
        _nodes.TryGetValue(name, out var node);
        return node;
    }

    private void Update()
    {
        if (ActiveRocket == null) return;

        CelestialBodyNode newSOI = null;
        double newDist = double.MaxValue;

        foreach (var node in _nodes.Values)
        {
            double d = Vector3.Distance(ActiveRocket.position, node.Transform.position);
            if (d <= node.SphereOfInfluenceRadius && d < newDist)
            {
                newDist = d;
                newSOI = node;
            }
        }

        if (newSOI != _currentSOI)
        {
            Debug.Log($"new soi: {newSOI.Name}");
            InvertOrbits(newSOI);
            _currentSOI = newSOI;
        }
    }

    public void InvertOrbits(CelestialBodyNode newSOI)
    {
        foreach (var node in _previousChain)
        {
            if (node == newSOI)
            {
                var newMover = node.Transform.GetComponent<OrbitMover>();
                if (newMover != null)
                {
                    newMover.enabled = false;
                }
                break;
            }
        }

        foreach (var oldNode in _previousChain)
        {
            var mover = oldNode.Transform.GetComponent<OrbitMover>();
            if (mover == null)
                continue;

            if (_savedOrbitData.TryGetValue(oldNode.Name, out var od))
                CopyOrbitData(od, mover.orbitData);

            if (_savedAttractors.TryGetValue(oldNode.Name, out var origAttr))
                mover.AttractorSettings.AttractorObject = origAttr;

            if (_savedOrbitData.TryGetValue(oldNode.Name, out od))
                mover.AttractorSettings.AttractorMass = od.AttractorMass;

            var vh = mover.VelocityHandle;
            if (vh != null && _savedVelocities.TryGetValue(oldNode.Name, out var pos))
            {
                vh.localPosition = pos;
                Debug.Log($"Restored velocity handle position for {oldNode.Name}: {pos}");
            }

            double beforeM = mover.orbitData.MeanAnomaly;

            var relPos = mover.orbitData.positionRelativeToAttractor;
            var sBasis = mover.orbitData.SemiMajorAxisBasis;
            var oNorm = mover.orbitData.OrbitNormal;

            double calcM = mover.orbitData.CalculateMeanAnomalyFromPosition();
            mover.orbitData.SetMeanAnomaly(calcM);

            mover.enabled = true;
        }

        var chain = new List<CelestialBodyNode>();
        var nodeItr = newSOI;
        while (nodeItr != null)
        {
            chain.Add(nodeItr);
            nodeItr = FindParent(nodeItr);
        }
        chain.Reverse();
        _previousChain = chain;

        foreach (var n in chain)
        {
            var mover = n.Transform.GetComponent<OrbitMover>();
            if (mover == null)
                continue;

            if (!_savedOrbitData.ContainsKey(n.Name))
                _savedOrbitData[n.Name] = CloneOrbitData(mover.orbitData);

            if (!_savedAttractors.ContainsKey(n.Name))
                _savedAttractors[n.Name] = mover.AttractorSettings.AttractorObject;

            var vh = mover.VelocityHandle;
            if (vh != null && !_savedVelocities.ContainsKey(n.Name))
                _savedVelocities[n.Name] = vh.localPosition;
        }

        for (int i = 0; i < chain.Count; i++)
        {
            var current = chain[i];
            var mover = current.Transform.GetComponent<OrbitMover>();
            if (mover == null)
                continue;

            mover.enabled = current != newSOI;

            Transform newAttr = (i + 1 < chain.Count) ? chain[i + 1].Transform : null;
            mover.AttractorSettings.AttractorObject = newAttr;

            if (newAttr != null && _savedOrbitData.TryGetValue(newAttr.gameObject.name, out var saved))
            {
                CopyOrbitData(saved, mover.orbitData);
                Vector3 worldPos = current.Transform.position;
                Vector3 attrWorldPos = newAttr.position;
                mover.orbitData.positionRelativeToAttractor = new Vector3d(
                    worldPos.x - attrWorldPos.x,
                    worldPos.y - attrWorldPos.y,
                    worldPos.z - attrWorldPos.z
                );
            }

            if (newAttr != null && _savedOrbitData.TryGetValue(newAttr.gameObject.name, out saved))
                mover.AttractorSettings.AttractorMass = saved.AttractorMass;

            var vh = mover.VelocityHandle;
            if (vh != null && newAttr != null && _savedVelocities.TryGetValue(newAttr.gameObject.name, out var vpos))
            {
                vh.localPosition = -vpos;
            }

            double newCalcM = mover.orbitData.CalculateMeanAnomalyFromPosition();
            mover.orbitData.SetMeanAnomaly(newCalcM);
            mover.GetComponent<OrbitDisplay>().enabled = false;
        }
    }

    private OrbitData CloneOrbitData(OrbitData original)
    {
        var clone = new OrbitData();
        foreach (var field in typeof(OrbitData)
                 .GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            field.SetValue(clone, field.GetValue(original));
        }
        return clone;
    }

    private void CopyOrbitData(OrbitData from, OrbitData to)
    {
        foreach (var field in typeof(OrbitData)
                 .GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            field.SetValue(to, field.GetValue(from));
        }
    }

    private CelestialBodyNode FindParent(CelestialBodyNode child)
    {
        return _nodes.Values.FirstOrDefault(n => n.GetChildren().Contains(child));
    }

    private void OnDrawGizmos()
    {
        if (_nodes == null || _nodes.Count == 0) return;

        var radii = _nodes.Values.Select(n => (float)n.SphereOfInfluenceRadius).ToList();
        float minR = radii.Min();
        float maxR = radii.Max();
        float range = maxR - minR;

        foreach (var node in _nodes.Values)
        {
            float r = (float)node.SphereOfInfluenceRadius;
            float t = range > 0 ? (r - minR) / range : 0f;
            Color c = Color.Lerp(Color.green, Color.red, t);
            c.a = 0.1f;
            Gizmos.color = c;
            Gizmos.DrawSphere(node.Transform.position, r);
        }
    }
}