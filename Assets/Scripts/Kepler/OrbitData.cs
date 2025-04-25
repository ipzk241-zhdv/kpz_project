using System;
using UnityEngine;

[Serializable]
public class OrbitData
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

    /// <summary>Ініціалізує порожній обʼєкт OrbitData.</summary>
    public OrbitData() { }

    /// <summary>Ініціалізує дані орбіти за вектором положення та швидкості.</summary>
    public OrbitData(Vector3d position, Vector3d velocity, double attractorMass, double gConst)
    {
        positionRelativeToAttractor = position;
        velocityRelativeToAttractor = velocity;
        AttractorMass = attractorMass;
        GravConst = gConst;
        CalculateOrbitStateFromOrbitalVectors();
    }

    /// <summary>Ініціалізує дані орбіти за класичними орбітальними елементами.</summary>
    public OrbitData(double eccentricity,
                     double semiMajorAxis,
                     double meanAnomalyDeg,
                     double inclinationDeg,
                     double argOfPerifocusDeg,
                     double ascendingNodeDeg,
                     double attractorMass,
                     double gConst)
    {
        Eccentricity = eccentricity;
        SemiMajorAxis = semiMajorAxis;
        if (eccentricity < 1.0)
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(1 - eccentricity * eccentricity);
        else if (eccentricity > 1.0)
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(eccentricity * eccentricity - 1);
        else
            SemiMinorAxis = 0;

        var normal = EclipticNormal.normalized;
        var ascendingNode = EclipticRight.normalized;

        ascendingNodeDeg %= 360;
        if (ascendingNodeDeg > 180) ascendingNodeDeg -= 360;
        inclinationDeg %= 360;
        if (inclinationDeg > 180) inclinationDeg -= 360;
        argOfPerifocusDeg %= 360;
        if (argOfPerifocusDeg > 180) argOfPerifocusDeg -= 360;

        ascendingNode = Vector3d.RotateVectorByAngle(
            ascendingNode,
            ascendingNodeDeg * Utils.Deg2Rad,
            normal
        ).normalized;

        normal = Vector3d.RotateVectorByAngle(
            normal,
            inclinationDeg * Utils.Deg2Rad,
            ascendingNode
        ).normalized;

        Periapsis = Vector3d.RotateVectorByAngle(
            ascendingNode,
            argOfPerifocusDeg * Utils.Deg2Rad,
            normal
        ).normalized;

        SemiMajorAxisBasis = Periapsis;
        SemiMinorAxisBasis = Vector3d.Cross(Periapsis, normal);

        MeanAnomaly = meanAnomalyDeg * Utils.Deg2Rad;
        EccentricAnomaly = Utils.ConvertMeanToEccentricAnomaly(MeanAnomaly, Eccentricity);
        TrueAnomaly = Utils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
        AttractorMass = attractorMass;
        GravConst = gConst;

        CalculateOrbitStateFromOrbitalElements();
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

    /// <summary>Обчислює всі поля орбіти за класичними елементами.</summary>
    public void CalculateOrbitStateFromOrbitalElements()
    {
        MG = AttractorMass * GravConst;
        OrbitNormal = -Vector3d.Cross(SemiMajorAxisBasis, SemiMinorAxisBasis).normalized;
        OrbitNormalDotEclipticNormal = Vector3d.Dot(OrbitNormal, EclipticNormal);

        if (Eccentricity < 1.0)
        {
            OrbitCompressionRatio = 1 - Eccentricity * Eccentricity;
            CenterPoint = -SemiMajorAxisBasis * SemiMajorAxis * Eccentricity;
            Period = Utils.PI_2 * Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / MG);
            MeanMotion = Utils.PI_2 / Period;
            Apoapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
            Periapsis = CenterPoint + SemiMajorAxisBasis * SemiMajorAxis;
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = Apoapsis.magnitude;
        }
        else if (Eccentricity > 1.0)
        {
            CenterPoint = SemiMajorAxisBasis * SemiMajorAxis * Eccentricity;
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG / Math.Pow(SemiMajorAxis, 3));
            Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            Periapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = double.PositiveInfinity;
        }
        else
        {
            CenterPoint = new Vector3d();
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG * 0.5 / Math.Pow(PeriapsisDistance, 3));
            Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            PeriapsisDistance = SemiMajorAxis;
            SemiMajorAxis = 0;
            Periapsis = -PeriapsisDistance * SemiMajorAxisBasis;
            ApoapsisDistance = double.PositiveInfinity;
        }

        positionRelativeToAttractor = GetFocalPositionAtEccentricAnomaly(EccentricAnomaly);
        double comp = Eccentricity < 1
            ? (1 - Eccentricity * Eccentricity)
            : (Eccentricity * Eccentricity - 1);
        FocalParameter = SemiMajorAxis * comp;
        velocityRelativeToAttractor = GetVelocityAtTrueAnomaly(TrueAnomaly);
        AttractorDistance = positionRelativeToAttractor.magnitude;
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

    /// <summary>Обчислює стан орбіти за векторами положення та швидкості.</summary>
    public void CalculateOrbitStateFromOrbitalVectors()
    {
        MG = AttractorMass * GravConst;
        AttractorDistance = positionRelativeToAttractor.magnitude;
        var h = Vector3d.Cross(positionRelativeToAttractor, velocityRelativeToAttractor);
        OrbitNormal = h.normalized;
        Vector3d ecc;
        if (OrbitNormal.sqrMagnitude < 0.99)
        {
            OrbitNormal = Vector3d.Cross(positionRelativeToAttractor, EclipticUp).normalized;
            ecc = new Vector3d();
        }
        else
        {
            ecc = Vector3d.Cross(velocityRelativeToAttractor, h) / MG
                  - positionRelativeToAttractor / AttractorDistance;
        }
        OrbitNormalDotEclipticNormal = Vector3d.Dot(OrbitNormal, EclipticNormal);
        FocalParameter = h.sqrMagnitude / MG;
        Eccentricity = ecc.magnitude;
        SemiMinorAxisBasis = Vector3d.Cross(h, -ecc).normalized;
        if (SemiMinorAxisBasis.sqrMagnitude < 0.99)
            SemiMinorAxisBasis = Vector3d.Cross(OrbitNormal, positionRelativeToAttractor).normalized;
        SemiMajorAxisBasis = Vector3d.Cross(OrbitNormal, SemiMinorAxisBasis).normalized;

        if (Eccentricity < 1.0)
        {
            OrbitCompressionRatio = 1 - Eccentricity * Eccentricity;
            SemiMajorAxis = FocalParameter / OrbitCompressionRatio;
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(OrbitCompressionRatio);
            CenterPoint = -SemiMajorAxis * ecc;
            var p = Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / MG);
            Period = Utils.PI_2 * p;
            MeanMotion = 1d / p;
            Apoapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
            Periapsis = CenterPoint + SemiMajorAxisBasis * SemiMajorAxis;
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = Apoapsis.magnitude;
            TrueAnomaly = Vector3d.Angle(positionRelativeToAttractor, SemiMajorAxisBasis) * Utils.Deg2Rad;
            if (Vector3d.Dot(Vector3d.Cross(positionRelativeToAttractor, -SemiMajorAxisBasis), OrbitNormal) < 0)
                TrueAnomaly = Utils.PI_2 - TrueAnomaly;
            EccentricAnomaly = Utils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
            MeanAnomaly = EccentricAnomaly - Eccentricity * Math.Sin(EccentricAnomaly);
        }
        else if (Eccentricity > 1.0)
        {
            OrbitCompressionRatio = Eccentricity * Eccentricity - 1;
            SemiMajorAxis = FocalParameter / OrbitCompressionRatio;
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(OrbitCompressionRatio);
            CenterPoint = SemiMajorAxis * ecc;
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG / Math.Pow(SemiMajorAxis, 3));
            Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            Periapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
            PeriapsisDistance = Periapsis.magnitude;
            ApoapsisDistance = double.PositiveInfinity;
            TrueAnomaly = Vector3d.Angle(positionRelativeToAttractor, ecc) * Utils.Deg2Rad;
            if (Vector3d.Dot(Vector3d.Cross(positionRelativeToAttractor, -SemiMajorAxisBasis), OrbitNormal) < 0)
                TrueAnomaly = -TrueAnomaly;
            EccentricAnomaly = Utils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
            MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
        }
        else
        {
            OrbitCompressionRatio = 0;
            SemiMajorAxis = 0;
            SemiMinorAxis = 0;
            PeriapsisDistance = h.sqrMagnitude / MG;
            CenterPoint = new Vector3d();
            Periapsis = -PeriapsisDistance * SemiMinorAxisBasis;
            Period = double.PositiveInfinity;
            MeanMotion = Math.Sqrt(MG / Math.Pow(PeriapsisDistance, 3));
            Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            ApoapsisDistance = double.PositiveInfinity;
            TrueAnomaly = Vector3d.Angle(positionRelativeToAttractor, ecc) * Utils.Deg2Rad;
            if (Vector3d.Dot(Vector3d.Cross(positionRelativeToAttractor, -SemiMajorAxisBasis), OrbitNormal) < 0)
                TrueAnomaly = -TrueAnomaly;
            EccentricAnomaly = Utils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
            MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
        }
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
}
