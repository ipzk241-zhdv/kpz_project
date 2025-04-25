using UnityEngine;

[System.Serializable]
public class GravitySource
{
    public Transform AttractorObject;
    public float AttractorMass = 1000f;
    public float GravityConstant = 0.1f;
}
