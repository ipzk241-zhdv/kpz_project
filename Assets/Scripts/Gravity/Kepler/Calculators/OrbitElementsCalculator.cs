using System;

public class OrbitElementsCalculator : IOrbitElementsCalculator
{
    /// <summary>Обчислює всі поля орбіти за класичними елементами.</summary>
    public void CalculateOrbitStateFromOrbitalElements(OrbitData d)
    {
        d.MG = d.AttractorMass * d.GravConst;
        d.OrbitNormal = -Vector3d.Cross(d.SemiMajorAxisBasis, d.SemiMinorAxisBasis).normalized;
        d.OrbitNormalDotEclipticNormal = Vector3d.Dot(d.OrbitNormal, EclipticConstants.EclipticNormal);

        if (d.Eccentricity < 1.0)
            CalculateEllipticElements(d);
        else if (d.Eccentricity > 1.0)
            CalculateHyperbolicElements(d);
        else
            CalculateParabolicElements(d);

        FinalizeOrbitalElementCalculation(d);
    }

    private void CalculateEllipticElements(OrbitData d)
    {
        d.OrbitCompressionRatio = 1 - d.Eccentricity * d.Eccentricity;
        d.CenterPoint = -d.SemiMajorAxisBasis * d.SemiMajorAxis * d.Eccentricity;
        d.Period = Utils.PI_2 * Math.Sqrt(Math.Pow(d.SemiMajorAxis, 3) / d.MG);
        d.MeanMotion = Utils.PI_2 / d.Period;
        d.Apoapsis = d.CenterPoint - d.SemiMajorAxisBasis * d.SemiMajorAxis;
        d.Periapsis = d.CenterPoint + d.SemiMajorAxisBasis * d.SemiMajorAxis;
        d.PeriapsisDistance = d.Periapsis.magnitude;
        d.ApoapsisDistance = d.Apoapsis.magnitude;
    }

    private void CalculateHyperbolicElements(OrbitData d)
    {
        d.CenterPoint = d.SemiMajorAxisBasis * d.SemiMajorAxis * d.Eccentricity;
        d.Period = double.PositiveInfinity;
        d.MeanMotion = Math.Sqrt(d.MG / Math.Pow(d.SemiMajorAxis, 3));
        d.Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        d.Periapsis = d.CenterPoint - d.SemiMajorAxisBasis * d.SemiMajorAxis;
        d.PeriapsisDistance = d.Periapsis.magnitude;
        d.ApoapsisDistance = double.PositiveInfinity;
    }

    private void CalculateParabolicElements(OrbitData d)
    {
        d.CenterPoint = new Vector3d();
        d.Period = double.PositiveInfinity;
        d.MeanMotion = Math.Sqrt(d.MG * 0.5 / Math.Pow(d.PeriapsisDistance, 3));
        d.Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        d.PeriapsisDistance = d.SemiMajorAxis;
        d.SemiMajorAxis = 0;
        d.Periapsis = -d.PeriapsisDistance * d.SemiMajorAxisBasis;
        d.ApoapsisDistance = double.PositiveInfinity;
    }

    private void FinalizeOrbitalElementCalculation(OrbitData d)
    {
        d.positionRelativeToAttractor = GetFocalPositionAtEccentricAnomaly(d, d.EccentricAnomaly);
        double comp = d.Eccentricity < 1
            ? (1 - d.Eccentricity * d.Eccentricity)
            : (d.Eccentricity * d.Eccentricity - 1);
        d.FocalParameter = d.SemiMajorAxis * comp;
        d.velocityRelativeToAttractor = OrbitAnomalyCalculator.GetVelocityAtTrueAnomaly(d, d.TrueAnomaly);
        d.AttractorDistance = d.positionRelativeToAttractor.magnitude;
    }

    /// <summary>Повертає положення фокальної точки за ексцентричною аномалією.</summary>
    public Vector3d GetFocalPositionAtEccentricAnomaly(OrbitData d, double eccentricAnomaly)
    {
        return OrbitPositionCalculator.GetCentralPositionAtEccentricAnomaly(d, eccentricAnomaly) + d.CenterPoint;
    }
}

