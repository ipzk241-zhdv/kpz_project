using System.Collections.Generic;
using System;

[Serializable]
public class BodyNodeJSON
{
    public string name;
    public OrbitDataDTO orbitData;
    public AttractorSettingsJSON attractorSettings;
}

[Serializable]
public class AttractorSettingsJSON
{
    public string attractorName;
    public double attractorMass;
}

[Serializable]
public class SystemJSON
{
    public List<BodyNodeJSON> bodies = new List<BodyNodeJSON>();
}

[Serializable]
public class OrbitDataDTO
{
    public double MG;
    public double GravConst;
    public double SemiMinorAxis;
    public double SemiMajorAxis;
    public double FocalParameter;
    public double Eccentricity;
    public double Period;
    public double TrueAnomaly;
    public double MeanAnomaly;
    public double EccentricAnomaly;
    public double MeanMotion;
    public Vector3d Periapsis;
    public double PeriapsisDistance;
    public Vector3d Apoapsis;
    public double ApoapsisDistance;
    public Vector3d CenterPoint;
    public double OrbitCompressionRatio;
    public Vector3d OrbitNormal;
    public Vector3d SemiMinorAxisBasis;
    public Vector3d SemiMajorAxisBasis;
    public double OrbitNormalDotEclipticNormal;
    public Vector3d positionRelativeToAttractor;
    public Vector3d velocityRelativeToAttractor;
    public double AttractorMass;
    public double AttractorDistance;
}
