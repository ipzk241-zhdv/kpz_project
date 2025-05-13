using System;

[Serializable]
public partial class OrbitData
{ 
    public double MG;
    public double GravConst = 1;
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

    private readonly OrbitVectorsCalculator _vectorCalc = new OrbitVectorsCalculator();
    private readonly OrbitElementsCalculator _elementsCalc = new OrbitElementsCalculator();
    
    /// <summary>Ініціалізує порожній обʼєкт OrbitData.</summary>
    public OrbitData() { }

    /// <summary>Обраховує орбіту за допомогою вектору швидкості, положення центру тяготіння та самого об'єкту.</summary>
    public void CalculateOrbitStateFromOrbitalVectors()
    {
        _vectorCalc.CalculateOrbitStateFromOrbitalVectors(this);
    }

    /// <summary>Обраховує орбіту за допомогою базових орбітальних елементів.</summary>
    public void CalculateOrbitStateFromOrbitalElements()
    {
        _elementsCalc.CalculateOrbitStateFromOrbitalElements(this);
    }

    /// <summary>Повертає пройдений час орбіти за середньою аномалією.</summary>
    public double GetCurrentOrbitTime()
    {
        if (Eccentricity < 1.0)
        {
            if (Period > 0 && Period < double.PositiveInfinity)
            {
                var anomaly = MeanAnomaly % Utils.PI_2;
                if (anomaly < 0) anomaly += Utils.PI_2;
                return anomaly / Utils.PI_2 * Period;
            }
            return 0.0;
        }
        else
        {
            if (MeanMotion > 0) return MeanAnomaly / MeanMotion;
            return 0.0;
        }
    }

    /// <summary>Встановлює середню аномалію та оновлює стан орбіти.</summary>
    public void SetMeanAnomaly(double m)
    => OrbitAnomalyCalculator.SetMeanAnomaly(this, m);

    /// <summary>Повертає нахил орбіти в радіанах.</summary>
    public double Inclination
    {
        get
        {
            var dot = Vector3d.Dot(OrbitNormal, EclipticConstants.EclipticNormal);
            return Math.Acos(dot);
        }
    }

    /// <summary>Повертає довготу висхідного вузла в радіанах.</summary>
    public double AscendingNodeLongitude
    {
        get
        {
            var ascNodeDir = Vector3d.Cross(EclipticConstants.EclipticNormal, OrbitNormal).normalized;
            var dot = Vector3d.Dot(ascNodeDir, EclipticConstants.EclipticRight);
            var angle = Math.Acos(dot);
            if (Vector3d.Dot(Vector3d.Cross(ascNodeDir, EclipticConstants.EclipticRight), EclipticConstants.EclipticNormal) >= 0)
                angle = Utils.PI_2 - angle;
            return angle;
        }
    }

    /// <summary>Повертає аргумент перицентра в радіанах.</summary>
    public double ArgumentOfPerifocus
    {
        get
        {
            var ascNodeDir = Vector3d.Cross(EclipticConstants.EclipticNormal, OrbitNormal).normalized;
            var dot = Vector3d.Dot(ascNodeDir, SemiMajorAxisBasis.normalized);
            var angle = Math.Acos(dot);
            if (Vector3d.Dot(Vector3d.Cross(ascNodeDir, SemiMajorAxisBasis), OrbitNormal) < 0)
                angle = Utils.PI_2 - angle;
            return angle;
        }
    }

    /// <summary>Перевіряє, чи стан орбіти є валідним.</summary>
    public bool IsValidOrbit
    {
        get
        {
            return Eccentricity >= 0
                   && Period > 0
                   && AttractorMass > 0;
        }
    }

    /// <summary>Повертає швидкість за ексцентричною аномалією.</summary>
    public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
    => OrbitAnomalyCalculator.GetVelocityAtTrueAnomaly(this, eccentricAnomaly);

    /// <summary>Оновлює весь стан орбіти за проміжок часу.</summary>
    public void UpdateOrbitDataByTime(double deltaTime)
    => OrbitAnomalyCalculator.UpdateOrbitAnomaliesByTime(this, deltaTime);

    /// <summary>Повертає список точок орбіти без створення нового масиву.</summary>
    public void GetOrbitPoints(ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d gravitySourceOrigin, double maxDistance = 500d)
    => OrbitPositionCalculator.GetOrbitPoints(this, ref orbitPoints, orbitPointsCount, gravitySourceOrigin, maxDistance);

    /// <summary>Повертає середню аномалію відносно поточної позиції.</summary
    public double CalculateMeanAnomalyFromPosition()
    => OrbitAnomalyCalculator.CalculateMeanAnomalyFromPosition(this);
}
