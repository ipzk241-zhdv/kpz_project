using System;

public static class OrbitPositionCalculator
{
    /// <summary>Повертає положення фокальної точки за ексцентричною аномалією.</summary>
    public static Vector3d GetFocalPositionAtEccentricAnomaly(OrbitData d, double eccentricAnomaly)
    {
        return GetCentralPositionAtEccentricAnomaly(d, eccentricAnomaly) + d.CenterPoint;
    }

    /// <summary>Повертає центральне положення за ексцентричною аномалією.</summary>
    public static Vector3d GetCentralPositionAtEccentricAnomaly(OrbitData d, double eccentricAnomaly)
    {
        if (d.Eccentricity < 1.0)
        {
            var r = new Vector3d(
                Math.Sin(eccentricAnomaly) * d.SemiMinorAxis,
               -Math.Cos(eccentricAnomaly) * d.SemiMajorAxis
            );
            return -d.SemiMinorAxisBasis * r.x - d.SemiMajorAxisBasis * r.y;
        }
        else if (d.Eccentricity > 1.0)
        {
            var r = new Vector3d(
                Math.Sinh(eccentricAnomaly) * d.SemiMinorAxis,
                Math.Cosh(eccentricAnomaly) * d.SemiMajorAxis
            );
            return -d.SemiMinorAxisBasis * r.x - d.SemiMajorAxisBasis * r.y;
        }
        else
        {
            var r = new Vector3d(
                d.PeriapsisDistance * Math.Sin(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly)),
                d.PeriapsisDistance * Math.Cos(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly))
            );
            return -d.SemiMinorAxisBasis * r.x + d.SemiMajorAxisBasis * r.y;
        }
    }

    /// <summary>
    /// Повертає центральну позицію при істинної аномалії.
    /// </summary>
    public static Vector3d GetCentralPositionAtTrueAnomaly(OrbitData d, double trueAnomaly)
    {
        double ecc = Utils.ConvertTrueToEccentricAnomaly(trueAnomaly, d.Eccentricity);
        return GetCentralPositionAtEccentricAnomaly(d, ecc);
    }

    public static Vector3d GetFocalPositionAtTrueAnomaly(OrbitData d, double trueAnomaly)
    {
        return GetCentralPositionAtTrueAnomaly(d, trueAnomaly) + d.CenterPoint;
    }

    public static void GetOrbitPoints(OrbitData d, ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d gravitySourceOrigin, double maxDistance = 500d)
    {
        if (orbitPointsCount < 2)
        {
            orbitPoints = new Vector3d[0];
            return;
        }

        if (d.Eccentricity < 1)
        {
            GenerateEllipticOrbitPoints(d, ref orbitPoints, orbitPointsCount, gravitySourceOrigin);
        }
        else
        {
            GenerateHyperbolicOrbitPoints(d, ref orbitPoints, orbitPointsCount, gravitySourceOrigin, maxDistance);
        }
    }

    private static void GenerateEllipticOrbitPoints(OrbitData d, ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d origin)
    {
        if (orbitPoints == null || orbitPoints.Length != orbitPointsCount)
        {
            orbitPoints = new Vector3d[orbitPointsCount];
        }

        for (int i = 0; i < orbitPointsCount; i++)
        {
            double eccentricAnomaly = i * Utils.PI_2 / (orbitPointsCount - 1);
            orbitPoints[i] = GetFocalPositionAtEccentricAnomaly(d, eccentricAnomaly) + origin;
        }
    }

    private static void GenerateHyperbolicOrbitPoints(OrbitData d, ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d origin, double maxDistance)
    {
        if (maxDistance < d.PeriapsisDistance)
        {
            orbitPoints = new Vector3d[0];
            return;
        }

        double maxAngle = Utils.CalcTrueAnomalyForDistance(maxDistance, d.Eccentricity, d.SemiMajorAxis, d.PeriapsisDistance);
        orbitPoints = new Vector3d[orbitPointsCount];

        for (int i = 0; i < orbitPointsCount; i++)
        {
            double trueAnomaly = -maxAngle + i * 2d * maxAngle / (orbitPointsCount - 1);
            orbitPoints[i] = GetFocalPositionAtTrueAnomaly(d, trueAnomaly) + origin;
        }
    }
}
