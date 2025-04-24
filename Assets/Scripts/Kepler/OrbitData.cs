using System;

[Serializable]
public class OrbitData
{
    public double MG;
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

    public Vector3d positionRelativeToAttractor;
    public Vector3d velocityRelativeToAttractor;

    #region VectorCalculations

    /// <summary>
    /// Орбітальний момент імпульсу: h = r ? v
    /// </summary>
    public Vector3d ComputeSpecificAngularMomentum()
    {
        return Vector3d.Cross(positionRelativeToAttractor, velocityRelativeToAttractor);
    }

    /// <summary>
    /// Вектор лінії вузлів: n = k ? h (k = (0,0,1))
    /// </summary>
    public Vector3d ComputeNodeVector()
    {
        Vector3d h = ComputeSpecificAngularMomentum();
        return Vector3d.Cross(new Vector3d(0, 0, 1), h);
    }

    /// <summary>
    /// Вектор ексцентриситету: e = (v ? h)/? - r?
    /// </summary>
    public Vector3d ComputeEccentricityVector()
    {
        Vector3d h = ComputeSpecificAngularMomentum();
        Vector3d term = Vector3d.Cross(velocityRelativeToAttractor, h) / MG;
        Vector3d rNorm = positionRelativeToAttractor.normalized;
        return term - rNorm;
    }

    /// <summary>
    /// Специфічна механічна енергія: ? = v?/2 ? ?/r
    /// </summary>
    public double ComputeSpecificEnergy()
    {
        double v2 = velocityRelativeToAttractor.sqrMagnitude;
        double r = positionRelativeToAttractor.magnitude;
        return v2 / 2.0 - MG / r;
    }

    /// <summary>
    /// Вектор нормалі орбіти: n = (r ? v).normalized
    /// </summary>
    public Vector3d ComputeOrbitNormal()
    {
        return Vector3d.Cross(positionRelativeToAttractor, velocityRelativeToAttractor).normalized;
    }

    #endregion

    #region OrbitalGeometry

    /// <summary>
    /// Розрахунок великої півосі: a = -? / (2?)
    /// </summary>
    public double ComputeSemiMajorAxis()
    {
        double energy = ComputeSpecificEnergy();
        return -MG / (2 * energy);
    }

    /// <summary>
    /// Розрахунок ексцентриситету: e = |e?|
    /// </summary>
    public double ComputeEccentricity()
    {
        return ComputeEccentricityVector().magnitude;
    }

    /// <summary>
    /// Розрахунок малої півосі: b = a * sqrt(1 - e^2)
    /// </summary>
    public double ComputeSemiMinorAxis()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * Math.Sqrt(1 - e * e);
    }

    /// <summary>
    /// Параметр фокуса (параболічний параметр): p = a * (1 - e^2)
    /// </summary>
    public double ComputeFocalParameter()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * (1 - e * e);
    }

    /// <summary>
    /// Орбітальний період: T = 2? * sqrt(a^3 / ?)
    /// </summary>
    public double ComputePeriod()
    {
        double a = ComputeSemiMajorAxis();
        return 2 * Math.PI * Math.Sqrt(a * a * a / MG);
    }

    /// <summary>
    /// Середній рух: n = sqrt(? / a^3)
    /// </summary>
    public double ComputeMeanMotion()
    {
        double a = ComputeSemiMajorAxis();
        return Math.Sqrt(MG / (a * a * a));
    }

    #endregion

    #region OrbitalAnomalies

    /// <summary>
    /// Розрахунок істинної аномалії ? за положенням та вектором ексцентриситету
    /// </summary>
    public double ComputeTrueAnomaly()
    {
        Vector3d e = ComputeEccentricityVector();
        Vector3d r = positionRelativeToAttractor;

        double cosNu = Vector3d.Dot(e, r.normalized) / e.magnitude;
        double nu = Math.Acos(Math.Clamp(cosNu, -1.0, 1.0));

        // Орієнтація відносно швидкості визначає знак
        if (Vector3d.Dot(r, velocityRelativeToAttractor) < 0)
            nu = 2 * Math.PI - nu;

        return nu;
    }

    /// <summary>
    /// Розрахунок ексцентричної аномалії E за істинною аномалією ?
    /// </summary>
    public double ComputeEccentricAnomaly()
    {
        double e = ComputeEccentricity();
        double nu = ComputeTrueAnomaly();

        double cosE = (e + Math.Cos(nu)) / (1 + e * Math.Cos(nu));
        double sinE = Math.Sqrt(1 - e * e) * Math.Sin(nu) / (1 + e * Math.Cos(nu));
        return Math.Atan2(sinE, cosE);
    }

    /// <summary>
    /// Розрахунок середньої аномалії M за ексцентричною аномалією
    /// M = E - e * sin(E)
    /// </summary>
    public double ComputeMeanAnomaly()
    {
        double e = ComputeEccentricity();
        double E = ComputeEccentricAnomaly();
        return E - e * Math.Sin(E);
    }

    #endregion

    #region OrbitalOrientation

    /// <summary>
    /// Нормаль до площини орбіти (одиничний вектор)
    /// </summary>
    public Vector3d GetOrbitalPlaneNormal()
    {
        return ComputeOrbitNormal();
    }

    /// <summary>
    /// Напрямок на періапсис (одиничний вектор)
    /// </summary>
    public Vector3d GetPeriapsisDirection()
    {
        return ComputeEccentricityVector().normalized;
    }

    /// <summary>
    /// Напрямок на апоапсис (протилежний до періапсису)
    /// </summary>
    public Vector3d GetApoapsisDirection()
    {
        return -GetPeriapsisDirection();
    }

    /// <summary>
    /// Базис великої півосі (по напрямку періапсису)
    /// </summary>
    public Vector3d GetSemiMajorBasis()
    {
        return GetPeriapsisDirection();
    }

    /// <summary>
    /// Базис малої півосі (в площині орбіти, перпендикуляр до великої)
    /// </summary>
    public Vector3d GetSemiMinorBasis()
    {
        return Vector3d.Cross(GetOrbitalPlaneNormal(), GetSemiMajorBasis()).normalized;
    }

    /// <summary>
    /// Кут нахилу орбіти (i): між орбітальною нормаллю та віссю Z
    /// </summary>
    public double GetInclination()
    {
        Vector3d h = GetOrbitalPlaneNormal();
        return Math.Acos(Math.Clamp(h.z, -1.0, 1.0));
    }

    /// <summary>
    /// Довгота висхідного вузла (?): між віссю X і вектором вузлів
    /// </summary>
    public double GetLongitudeOfAscendingNode()
    {
        Vector3d n = ComputeNodeVector().normalized;
        double angle = Math.Acos(Math.Clamp(n.x, -1.0, 1.0));
        if (n.y < 0) angle = 2 * Math.PI - angle;
        return angle;
    }

    /// <summary>
    /// Аргумент перицентра (?): між вектором вузлів і напрямком на періапсис
    /// </summary>
    public double GetArgumentOfPeriapsis()
    {
        Vector3d n = ComputeNodeVector().normalized;
        Vector3d e = GetPeriapsisDirection();

        double angle = Math.Acos(Math.Clamp(Vector3d.Dot(n, e), -1.0, 1.0));
        if (e.z < 0) angle = 2 * Math.PI - angle;
        return angle;
    }

    #endregion
}
