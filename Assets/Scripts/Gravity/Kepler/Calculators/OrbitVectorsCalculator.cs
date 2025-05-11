using System;

public class OrbitVectorsCalculator : IOrbitVectorsCalculator
{
    /// <summary>Обчислює стан орбіти за векторами положення та швидкості.</summary>
    public void CalculateOrbitStateFromOrbitalVectors(OrbitData d)
    {
        d.MG = d.AttractorMass * d.GravConst;
        d.AttractorDistance = d.positionRelativeToAttractor.magnitude;
        var h = Vector3d.Cross(d.positionRelativeToAttractor, d.velocityRelativeToAttractor);
        d.OrbitNormal = h.normalized;

        Vector3d ecc;
        if (d.OrbitNormal.sqrMagnitude < 0.99)
        {
            d.OrbitNormal = Vector3d.Cross(d.positionRelativeToAttractor, EclipticConstants.EclipticUp).normalized;
            ecc = new Vector3d();
        }
        else
        {
            ecc = Vector3d.Cross(d.velocityRelativeToAttractor, h) / d.MG
                  - d.positionRelativeToAttractor / d.AttractorDistance;
        }

        ComputeBasisVectorsAndFocalParameter(d, h, ecc);
        ComputeAnomaliesAndExtremes(d, ecc, h);
    }

    private void ComputeBasisVectorsAndFocalParameter(OrbitData d, Vector3d h, Vector3d ecc)
    {
        d.OrbitNormalDotEclipticNormal = Vector3d.Dot(d.OrbitNormal, EclipticConstants.EclipticNormal);
        d.FocalParameter = h.sqrMagnitude / d.MG;
        d.Eccentricity = ecc.magnitude;

        d.SemiMinorAxisBasis = Vector3d.Cross(h, -ecc).normalized;
        if (d.SemiMinorAxisBasis.sqrMagnitude < 0.99)
            d.SemiMinorAxisBasis = Vector3d.Cross(d.OrbitNormal, d.positionRelativeToAttractor).normalized;

        d.SemiMajorAxisBasis = Vector3d.Cross(d.OrbitNormal, d.SemiMinorAxisBasis).normalized;
    }

    private void ComputeAnomaliesAndExtremes(OrbitData d, Vector3d ecc, Vector3d h)
    {
        if (d.Eccentricity < 1.0)
            ComputeEllipticOrbit(d, ecc);
        else if (d.Eccentricity > 1.0)
            ComputeHyperbolicOrbit(d, ecc);
        else
            ComputeParabolicOrbit(d, ecc, h);
    }

    private void ComputeEllipticOrbit(OrbitData d, Vector3d ecc)
    {
        d.OrbitCompressionRatio = 1 - d.Eccentricity * d.Eccentricity;
        d.SemiMajorAxis = d.FocalParameter / d.OrbitCompressionRatio;
        d.SemiMinorAxis = d.SemiMajorAxis * Math.Sqrt(d.OrbitCompressionRatio);
        d.CenterPoint = -d.SemiMajorAxis * ecc;
        var p = Math.Sqrt(Math.Pow(d.SemiMajorAxis, 3) / d.MG);
        d.Period = Utils.PI_2 * p;
        d.MeanMotion = 1d / p;

        d.Apoapsis = d.CenterPoint - d.SemiMajorAxisBasis * d.SemiMajorAxis;
        d.Periapsis = d.CenterPoint + d.SemiMajorAxisBasis * d.SemiMajorAxis;
        d.PeriapsisDistance = d.Periapsis.magnitude;
        d.ApoapsisDistance = d.Apoapsis.magnitude;

        d.TrueAnomaly = Vector3d.Angle(d.positionRelativeToAttractor, d.SemiMajorAxisBasis) * Utils.Deg2Rad;
        if (Vector3d.Dot(Vector3d.Cross(d.positionRelativeToAttractor, -d.SemiMajorAxisBasis), d.OrbitNormal) < 0)
            d.TrueAnomaly = Utils.PI_2 - d.TrueAnomaly;

        d.EccentricAnomaly = Utils.ConvertTrueToEccentricAnomaly(d.TrueAnomaly, d.Eccentricity);
        d.MeanAnomaly = d.EccentricAnomaly - d.Eccentricity * Math.Sin(d.EccentricAnomaly);
    }

    private void ComputeHyperbolicOrbit(OrbitData d, Vector3d ecc)
    {
        d.OrbitCompressionRatio = d.Eccentricity * d.Eccentricity - 1;
        d.SemiMajorAxis = d.FocalParameter / d.OrbitCompressionRatio;
        d.SemiMinorAxis = d.SemiMajorAxis * Math.Sqrt(d.OrbitCompressionRatio);
        d.CenterPoint = d.SemiMajorAxis * ecc;
        d.Period = double.PositiveInfinity;
        d.MeanMotion = Math.Sqrt(d.MG / Math.Pow(d.SemiMajorAxis, 3));

        d.Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        d.Periapsis = d.CenterPoint - d.SemiMajorAxisBasis * d.SemiMajorAxis;
        d.PeriapsisDistance = d.Periapsis.magnitude;
        d.ApoapsisDistance = double.PositiveInfinity;

        d.TrueAnomaly = Vector3d.Angle(d.positionRelativeToAttractor, ecc) * Utils.Deg2Rad;
        if (Vector3d.Dot(Vector3d.Cross(d.positionRelativeToAttractor, -d.SemiMajorAxisBasis), d.OrbitNormal) < 0)
            d.TrueAnomaly = -d.TrueAnomaly;

        d.EccentricAnomaly = Utils.ConvertTrueToEccentricAnomaly(d.TrueAnomaly, d.Eccentricity);
        d.MeanAnomaly = Math.Sinh(d.EccentricAnomaly) * d.Eccentricity - d.EccentricAnomaly;
    }

    private void ComputeParabolicOrbit(OrbitData d, Vector3d ecc, Vector3d h)
    {
        d.OrbitCompressionRatio = 0;
        d.SemiMajorAxis = 0;
        d.SemiMinorAxis = 0;
        d.PeriapsisDistance = h.sqrMagnitude / d.MG;
        d.CenterPoint = new Vector3d();
        d.Periapsis = -d.PeriapsisDistance * d.SemiMinorAxisBasis;
        d.Period = double.PositiveInfinity;
        d.MeanMotion = Math.Sqrt(d.MG / Math.Pow(d.PeriapsisDistance, 3));

        d.Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        d.ApoapsisDistance = double.PositiveInfinity;

        d.TrueAnomaly = Vector3d.Angle(d.positionRelativeToAttractor, ecc) * Utils.Deg2Rad;
        if (Vector3d.Dot(Vector3d.Cross(d.positionRelativeToAttractor, -d.SemiMajorAxisBasis), d.OrbitNormal) < 0)
            d.TrueAnomaly = -d.TrueAnomaly;

        d.EccentricAnomaly = Utils.ConvertTrueToEccentricAnomaly(d.TrueAnomaly, d.Eccentricity);
        d.MeanAnomaly = Math.Sinh(d.EccentricAnomaly) * d.Eccentricity - d.EccentricAnomaly;
    }
}

