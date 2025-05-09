using System;
using UnityEngine;
using static UnityEngine.UI.Image;

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
    public static readonly Vector3d EclipticRight = new Vector3d(1, 0, 0);
    public static readonly Vector3d EclipticUp = new Vector3d(0, 1, 0);
    public static readonly Vector3d EclipticNormal = new Vector3d(0, 0, 1);

    private readonly OrbitVectorsCalculator _vectorCalc = new OrbitVectorsCalculator();
    private readonly OrbitElementsCalculator _elementsCalc = new OrbitElementsCalculator();
    
    /// <summary>Ініціалізує порожній обʼєкт OrbitData.</summary>
    public OrbitData() { }


    public void CalculateOrbitStateFromOrbitalVectors()
    {
        _vectorCalc.CalculateOrbitStateFromOrbitalVectors(this);
    }

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
    {
        if (!IsValidOrbit) return;
        MeanAnomaly = m % Utils.PI_2;
        if (Eccentricity < 1.0)
        {
            if (MeanAnomaly < 0) MeanAnomaly += Utils.PI_2;
            EccentricAnomaly = Utils.SolveKeplersEquation(MeanAnomaly, Eccentricity);
            TrueAnomaly = Utils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
        }
        else if (Eccentricity > 1.0)
        {
            EccentricAnomaly = Utils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
            TrueAnomaly = Utils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
        }
        else
        {
            EccentricAnomaly = Utils.ConvertMeanToEccentricAnomaly(MeanAnomaly, Eccentricity);
            TrueAnomaly = EccentricAnomaly;
        }
        SetPositionByCurrentAnomaly();
        SetVelocityByCurrentAnomaly();
    }

    /// <summary>Повертає нахил орбіти в радіанах.</summary>
    public double Inclination
    {
        get
        {
            var dot = Vector3d.Dot(OrbitNormal, EclipticNormal);
            return Math.Acos(dot);
        }
    }

    /// <summary>Повертає довготу висхідного вузла в радіанах.</summary>
    public double AscendingNodeLongitude
    {
        get
        {
            var ascNodeDir = Vector3d.Cross(EclipticNormal, OrbitNormal).normalized;
            var dot = Vector3d.Dot(ascNodeDir, EclipticRight);
            var angle = Math.Acos(dot);
            if (Vector3d.Dot(Vector3d.Cross(ascNodeDir, EclipticRight), EclipticNormal) >= 0)
                angle = Utils.PI_2 - angle;
            return angle;
        }
    }

    /// <summary>Повертає аргумент перицентра в радіанах.</summary>
    public double ArgumentOfPerifocus
    {
        get
        {
            var ascNodeDir = Vector3d.Cross(EclipticNormal, OrbitNormal).normalized;
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

    /// <summary>Повертає положення фокальної точки за ексцентричною аномалією.</summary>
    public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly)
    {
        return GetCentralPositionAtEccentricAnomaly(eccentricAnomaly) + CenterPoint;
    }

    /// <summary>Повертає центральне положення за ексцентричною аномалією.</summary>
    public Vector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly)
    {
        if (Eccentricity < 1.0)
        {
            var r = new Vector3d(
                Math.Sin(eccentricAnomaly) * SemiMinorAxis,
               -Math.Cos(eccentricAnomaly) * SemiMajorAxis
            );
            return -SemiMinorAxisBasis * r.x - SemiMajorAxisBasis * r.y;
        }
        else if (Eccentricity > 1.0)
        {
            var r = new Vector3d(
                Math.Sinh(eccentricAnomaly) * SemiMinorAxis,
                Math.Cosh(eccentricAnomaly) * SemiMajorAxis
            );
            return -SemiMinorAxisBasis * r.x - SemiMajorAxisBasis * r.y;
        }
        else
        {
            var r = new Vector3d(
                PeriapsisDistance * Math.Sin(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly)),
                PeriapsisDistance * Math.Cos(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly))
            );
            return -SemiMinorAxisBasis * r.x + SemiMajorAxisBasis * r.y;
        }
    }

    /// <summary>Оновлює аномалії орбіти за проміжок часу.</summary>
    public void UpdateOrbitAnomaliesByTime(double deltaTime)
    {
        if (Eccentricity < 1.0)
        {
            MeanAnomaly += MeanMotion * deltaTime;
            MeanAnomaly %= Utils.PI_2;
            if (MeanAnomaly < 0) MeanAnomaly += Utils.PI_2;
            EccentricAnomaly = Utils.SolveKeplersEquation(MeanAnomaly, Eccentricity);
            var cosE = Math.Cos(EccentricAnomaly);
            TrueAnomaly = Math.Acos((cosE - Eccentricity) / (1 - Eccentricity * cosE));
            if (MeanAnomaly > Math.PI) TrueAnomaly = Utils.PI_2 - TrueAnomaly;
        }
        else if (Eccentricity > 1.0)
        {
            MeanAnomaly += MeanMotion * deltaTime;
            EccentricAnomaly = Utils.SolveKeplersEquation(MeanAnomaly, Eccentricity);
            TrueAnomaly = Math.Atan2(
                Math.Sqrt(Eccentricity * Eccentricity - 1.0) * Math.Sinh(EccentricAnomaly),
                Eccentricity - Math.Cosh(EccentricAnomaly)
            );
        }
        else
        {
            MeanAnomaly += MeanMotion * deltaTime;
            EccentricAnomaly = Utils.ConvertMeanToEccentricAnomaly(MeanAnomaly, Eccentricity);
            TrueAnomaly = EccentricAnomaly;
        }
    }

    /// <summary>Повертає швидкість за заданою істинною аномалією.</summary>
    public Vector3d GetVelocityAtTrueAnomaly(double trueAnomaly)
    {
        if (FocalParameter <= 0) return new Vector3d();
        var sqrtMGp = Math.Sqrt(AttractorMass * GravConst / FocalParameter);
        var vX = sqrtMGp * (Eccentricity + Math.Cos(trueAnomaly));
        var vY = sqrtMGp * Math.Sin(trueAnomaly);
        return -SemiMinorAxisBasis * vX - SemiMajorAxisBasis * vY;
    }

    /// <summary>Повертає швидкість за ексцентричною аномалією.</summary>
    public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
    {
        return GetVelocityAtTrueAnomaly(Utils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, Eccentricity));
    }

    /// <summary>Встановлює позицію за поточною ексцентричною аномалією.</summary>
    public void SetPositionByCurrentAnomaly()
    {
        positionRelativeToAttractor = GetFocalPositionAtEccentricAnomaly(EccentricAnomaly);
    }

    /// <summary>Встановлює швидкість за поточною ексцентричною аномалією.</summary>
    public void SetVelocityByCurrentAnomaly()
    {
        velocityRelativeToAttractor = GetVelocityAtEccentricAnomaly(EccentricAnomaly);
    }

    /// <summary>Оновлює весь стан орбіти за проміжок часу.</summary>
    public void UpdateOrbitDataByTime(double deltaTime)
    {
        UpdateOrbitAnomaliesByTime(deltaTime);
        SetPositionByCurrentAnomaly();
        SetVelocityByCurrentAnomaly();
    }

    /// <summary>
    /// Повертає центральну позицію при істинної аномалії.
    /// </summary>
    public Vector3d GetCentralPositionAtTrueAnomaly(double trueAnomaly)
    {
        double ecc = Utils.ConvertTrueToEccentricAnomaly(trueAnomaly, Eccentricity);
        return GetCentralPositionAtEccentricAnomaly(ecc);
    }

    /// <summary>
    /// Повертає фокальну позицію при істинній аномалії
    /// </summary>
    public Vector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly)
    {
        return GetCentralPositionAtTrueAnomaly(trueAnomaly) + CenterPoint;
    }

    public void GetOrbitPoints(ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d gravitySourceOrigin, double maxDistance = 500d)
    {
        if (orbitPointsCount < 2)
        {
            orbitPoints = new Vector3d[0];
            return;
        }

        if (Eccentricity < 1)
        {
            GenerateEllipticOrbitPoints(ref orbitPoints, orbitPointsCount, gravitySourceOrigin);
        }
        else
        {
            GenerateHyperbolicOrbitPoints(ref orbitPoints, orbitPointsCount, gravitySourceOrigin, maxDistance);
        }
    }

    private void GenerateEllipticOrbitPoints(ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d origin)
    {
        if (orbitPoints == null || orbitPoints.Length != orbitPointsCount)
        {
            orbitPoints = new Vector3d[orbitPointsCount];
        }

        for (int i = 0; i < orbitPointsCount; i++)
        {
            double eccentricAnomaly = i * Utils.PI_2 / (orbitPointsCount - 1);
            orbitPoints[i] = GetFocalPositionAtEccentricAnomaly(eccentricAnomaly) + origin;
        }
    }

    private void GenerateHyperbolicOrbitPoints(ref Vector3d[] orbitPoints, int orbitPointsCount, Vector3d origin, double maxDistance)
    {
        if (maxDistance < PeriapsisDistance)
        {
            orbitPoints = new Vector3d[0];
            return;
        }

        double maxAngle = Utils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis, PeriapsisDistance);
        orbitPoints = new Vector3d[orbitPointsCount];

        for (int i = 0; i < orbitPointsCount; i++)
        {
            double trueAnomaly = -maxAngle + i * 2d * maxAngle / (orbitPointsCount - 1);
            orbitPoints[i] = GetFocalPositionAtTrueAnomaly(trueAnomaly) + origin;
        }
    }

    public double CalculateMeanAnomalyFromPosition()
    {
        // Відстань до центрального тіла
        double r = positionRelativeToAttractor.magnitude;
        // Визначаємо істинну аномалію
        double cosTrue = Vector3d.Dot(positionRelativeToAttractor, SemiMajorAxisBasis) / r;
        cosTrue = Math.Clamp(cosTrue, -1.0, 1.0);
        double trueAnom = Math.Acos(cosTrue);
        // Враховуємо напрямок обертання
        if (Vector3d.Dot(Vector3d.Cross(SemiMajorAxisBasis, positionRelativeToAttractor), OrbitNormal) < 0)
            trueAnom = Utils.PI_2 - trueAnom;

        double M;
        if (Eccentricity < 1.0)
        {
            // Ексцентрична аномалія
            double E = Utils.ConvertTrueToEccentricAnomaly(trueAnom, Eccentricity);
            // Mean anomaly для еліптичної орбіти
            M = E - Eccentricity * Math.Sin(E);
        }
        else if (Eccentricity > 1.0)
        {
            // Ексцентрична аномалія для гіперболи
            double H = Utils.ConvertTrueToEccentricAnomaly(trueAnom, Eccentricity);
            M = Eccentricity * Math.Sinh(H) - H;
        }
        else
        {
            // Двогіперболічний випадок (е=1)
            double E = Utils.ConvertTrueToEccentricAnomaly(trueAnom, Eccentricity);
            M = E - Eccentricity * Math.Sin(E);
        }
        return M;
    }
}
