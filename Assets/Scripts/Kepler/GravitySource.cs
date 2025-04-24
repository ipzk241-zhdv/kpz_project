using UnityEngine;

[System.Serializable]
public class AttractorData
{
    public Transform AttractorObject;
    public float AttractorMass = 1000f;
    public float GravityConstant = 0.1f;
}
