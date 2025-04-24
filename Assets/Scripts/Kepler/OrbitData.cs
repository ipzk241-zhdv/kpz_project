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

    #region OrbitalPoints

    /// <summary>
    /// Поточна позиція об'єкта на орбіті (вже задана)
    /// </summary>
    public Vector3d GetCurrentPosition()
    {
        return positionRelativeToAttractor;
    }

    /// <summary>
    /// Поточна швидкість об'єкта на орбіті (вже задана)
    /// </summary>
    public Vector3d GetCurrentVelocity()
    {
        return velocityRelativeToAttractor;
    }

    /// <summary>
    /// Позиція періапсису у світових координатах
    /// </summary>
    public Vector3d GetPeriapsisPoint()
    {
        return CenterPoint + GetPeriapsisDirection() * ComputePeriapsisDistance();
    }

    /// <summary>
    /// Позиція апоапсису у світових координатах
    /// </summary>
    public Vector3d GetApoapsisPoint()
    {
        return CenterPoint + GetApoapsisDirection() * ComputeApoapsisDistance();
    }

    /// <summary>
    /// Відстань до періапсису: r_p = a * (1 - e)
    /// </summary>
    public double ComputePeriapsisDistance()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * (1 - e);
    }

    /// <summary>
    /// Відстань до апоапсису: r_a = a * (1 + e)
    /// </summary>
    public double ComputeApoapsisDistance()
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        return a * (1 + e);
    }

    /// <summary>
    /// Центр орбіти (середина великої осі, між апо- і періапсисом)
    /// </summary>
    public Vector3d ComputeEllipseCenter()
    {
        return (GetPeriapsisPoint() + GetApoapsisPoint()) * 0.5;
    }

    #endregion

    #region TemporalCalculations

    /// <summary>
    /// Розрахунок середньої аномалії в заданий час
    /// M(t) = M0 + n * (t - t0)
    /// </summary>
    public double ComputeMeanAnomalyAtTime(double meanAnomalyAtEpoch, double meanMotion, double timeSinceEpoch)
    {
        double M = meanAnomalyAtEpoch + meanMotion * timeSinceEpoch;
        return NormalizeAngle(M);
    }

    /// <summary>
    /// Розв'язання рівняння Кеплера: M = E - e * sin(E)
    /// методом Ньютона, щоб знайти ексцентричну аномалію
    /// </summary>
    public double SolveKeplersEquation(double M, double e, int maxIterations = 20, double tolerance = 1e-8)
    {
        double E = M;
        for (int i = 0; i < maxIterations; i++)
        {
            double f = E - e * Math.Sin(E) - M;
            double fPrime = 1 - e * Math.Cos(E);
            double delta = f / fPrime;
            E -= delta;
            if (Math.Abs(delta) < tolerance) break;
        }
        return E;
    }

    /// <summary>
    /// Розрахунок істинної аномалії з ексцентричної
    /// </summary>
    public double ComputeTrueAnomalyFromEccentric(double E, double e)
    {
        double cosNu = (Math.Cos(E) - e) / (1 - e * Math.Cos(E));
        double sinNu = (Math.Sqrt(1 - e * e) * Math.Sin(E)) / (1 - e * Math.Cos(E));
        return Math.Atan2(sinNu, cosNu);
    }

    /// <summary>
    /// Позиція на орбіті в момент часу t
    /// </summary>
    public Vector3d ComputePositionAtTime(double timeSinceEpoch)
    {
        double a = ComputeSemiMajorAxis();
        double e = ComputeEccentricity();
        double M = ComputeMeanAnomalyAtTime(MeanAnomaly, ComputeMeanMotion(), timeSinceEpoch);
        double E = SolveKeplersEquation(M, e);
        double nu = ComputeTrueAnomalyFromEccentric(E, e);

        double r = a * (1 - e * e) / (1 + e * Math.Cos(nu));

        Vector3d direction = GetPeriapsisDirection().RotateAround(OrbitNormal, nu);
        return CenterPoint + direction * r;
    }

    /// <summary>
    /// Нормалізація кута в межах [0, 2?]
    /// </summary>
    private double NormalizeAngle(double angle)
    {
        double twoPi = 2 * Math.PI;
        angle = angle % twoPi;
        if (angle < 0) angle += twoPi;
        return angle;
    }

    #endregion


}
