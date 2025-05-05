using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyNode
{
    public string Name { get; }
    public Transform Transform { get; }

    private readonly List<CelestialBodyNode> _children = new List<CelestialBodyNode>();
    public double SphereOfInfluenceRadius { get; set; }

    public CelestialBodyNode(string name, Transform transform)
    {
        Name = name;
        Transform = transform;
    }

    public void AddChild(CelestialBodyNode child)
    {
        if (child == null || _children.Contains(child))
            return;
        _children.Add(child);
    }

    public void RemoveChild(CelestialBodyNode child)
    {
        if (child == null) return;
        _children.Remove(child);
    }

    public IReadOnlyList<CelestialBodyNode> GetChildren() => _children;
}