using UnityEngine;

[System.Serializable]
public class GravitySource : IGravityConstantReceiver
{
    public Transform AttractorObject;
    public double AttractorMass = 1000f;
    public double GravityConstant = 0.1f;

    public void OnGravityConstantChanged(double gravityConstant)
    {
        GravityConstant = gravityConstant;
    }
}
