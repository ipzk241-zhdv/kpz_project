using UnityEngine;

[System.Serializable]
public class GravitySource
{
    public Transform AttractorObject;
    public double AttractorMass = 1000f;
    public double GravityConstant = 0.1f;
}
