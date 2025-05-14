using System;

public static class OrbitAnomalyCalculator
{
    /// <summary>Оновлює аномалії орбіти за проміжок часу.</summary>
    public static void UpdateOrbitAnomaliesByTime(OrbitData d, double deltaTime)
    {
        if (d.Eccentricity < 1.0)
        {
            d.MeanAnomaly += d.MeanMotion * deltaTime;
            d.MeanAnomaly %= Utils.PI_2;
            if (d.MeanAnomaly < 0) d.MeanAnomaly += Utils.PI_2;
            d.EccentricAnomaly = Utils.SolveKeplersEquation(d.MeanAnomaly, d.Eccentricity);
            var cosE = Math.Cos(d.EccentricAnomaly);
            d.TrueAnomaly = Math.Acos((cosE - d.Eccentricity) / (1 - d.Eccentricity * cosE));
            if (d.MeanAnomaly > Math.PI) d.TrueAnomaly = Utils.PI_2 - d.TrueAnomaly;
        }
        else if (d.Eccentricity > 1.0)
        {
            d.MeanAnomaly += d.MeanMotion * deltaTime;
            d.EccentricAnomaly = Utils.SolveKeplersEquation(d.MeanAnomaly, d.Eccentricity);
            d.TrueAnomaly = Math.Atan2(
                Math.Sqrt(d.Eccentricity * d.Eccentricity - 1.0) * Math.Sinh(d.EccentricAnomaly),
                d.Eccentricity - Math.Cosh(d.EccentricAnomaly)
            );
        }
        else
        {
            d.MeanAnomaly += d.MeanMotion * deltaTime;
            d.EccentricAnomaly = Utils.ConvertMeanToEccentricAnomaly(d.MeanAnomaly, d.Eccentricity);
            d.TrueAnomaly = d.EccentricAnomaly;
        }
        SetPositionByCurrentAnomaly(d);
        SetVelocityByCurrentAnomaly(d);
    }

    public static void SetMeanAnomaly(OrbitData d, double m)
    {
        d.MeanAnomaly = m % Utils.PI_2;
        if (d.Eccentricity < 1.0)
        {
            if (d.MeanAnomaly < 0) d.MeanAnomaly += Utils.PI_2;
            d.EccentricAnomaly = Utils.SolveKeplersEquation(d.MeanAnomaly, d.Eccentricity);
            d.TrueAnomaly = Utils.ConvertEccentricToTrueAnomaly(d.EccentricAnomaly, d.Eccentricity);
        }
        else if (d.Eccentricity > 1.0)
        {
            d.EccentricAnomaly = Utils.KeplerSolverHyperbolicCase(d.MeanAnomaly, d.Eccentricity);
            d.TrueAnomaly = Utils.ConvertEccentricToTrueAnomaly(d.EccentricAnomaly, d.Eccentricity);
        }
        else
        {
            d.EccentricAnomaly = Utils.ConvertMeanToEccentricAnomaly(d.MeanAnomaly, d.Eccentricity);
            d.TrueAnomaly = d.EccentricAnomaly;
        }
        SetPositionByCurrentAnomaly(d);
        SetVelocityByCurrentAnomaly(d);
    }

    public static double CalculateMeanAnomalyFromPosition(OrbitData d)
    {
        double r = d.positionRelativeToAttractor.magnitude;
        double cosTrue = Vector3d.Dot(d.positionRelativeToAttractor, d.SemiMajorAxisBasis) / r;
        cosTrue = Math.Clamp(cosTrue, -1.0, 1.0);
        double trueAnom = Math.Acos(cosTrue);

        if (Vector3d.Dot(Vector3d.Cross(d.SemiMajorAxisBasis, d.positionRelativeToAttractor), d.OrbitNormal) < 0)
            trueAnom = Utils.PI_2 - trueAnom;

        double M;
        if (d.Eccentricity < 1.0)
        {
            double E = Utils.ConvertTrueToEccentricAnomaly(trueAnom, d.Eccentricity);
            M = E - d.Eccentricity * Math.Sin(E);
        }
        else if (d.Eccentricity > 1.0)
        {
            double H = Utils.ConvertTrueToEccentricAnomaly(trueAnom, d.Eccentricity);
            M = d.Eccentricity * Math.Sinh(H) - H;
        }
        else
        {
            double E = Utils.ConvertTrueToEccentricAnomaly(trueAnom, d.Eccentricity);
            M = E - d.Eccentricity * Math.Sin(E);
        }
        return M;
    }

    /// <summary>Встановлює позицію за поточною ексцентричною аномалією.</summary>
    public static void SetPositionByCurrentAnomaly(OrbitData d)
    {
        d.positionRelativeToAttractor = OrbitPositionCalculator.GetFocalPositionAtEccentricAnomaly(d, d.EccentricAnomaly);
    }

    /// <summary>Встановлює швидкість за поточною ексцентричною аномалією.</summary>
    public static void SetVelocityByCurrentAnomaly(OrbitData d)
    {
        d.velocityRelativeToAttractor = d.GetVelocityAtEccentricAnomaly(d.EccentricAnomaly);
    }

    /// <summary>Повертає швидкість за заданою істинною аномалією.</summary>
    public static Vector3d GetVelocityAtTrueAnomaly(OrbitData d, double eccentricAnomaly)
    {
        double trueAnomaly = Utils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, d.Eccentricity);
        if (d.FocalParameter <= 0) return new Vector3d();
        var sqrtMGp = Math.Sqrt(d.AttractorMass * d.GravConst / d.FocalParameter);
        var vX = sqrtMGp * (d.Eccentricity + Math.Cos(trueAnomaly));
        var vY = sqrtMGp * Math.Sin(trueAnomaly);
        return -d.SemiMinorAxisBasis * vX - d.SemiMajorAxisBasis * vY;
    }
}
