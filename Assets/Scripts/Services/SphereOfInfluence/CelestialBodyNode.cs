using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyNode
{
    public string Name { get; }
    public Transform Transform { get; }
    private readonly List<CelestialBodyNode> _children = new List<CelestialBodyNode>();
    public CelestialBodyNode(string name, Transform t) { Name = name; Transform = t; }
    public void AddChild(CelestialBodyNode c) { if (c != null && !_children.Contains(c)) _children.Add(c); }
    public IReadOnlyList<CelestialBodyNode> GetChildren() => _children;
}