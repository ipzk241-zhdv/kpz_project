using System;

public class Utils
{
    public const double PI_2 = 6.2831853071796d;
    public const double PI = 3.14159265358979;
    public const double Deg2Rad = 0.017453292519943d;
    public const double Rad2Deg = 57.295779513082d;

    /// <summary>
    /// Обчислення оберненого гіперболічного косинуса.
    /// </summary>
    public static double Acosh(double x)
    {
        if (x < 1.0)
        {
            return 0;
        }

        return Math.Log(x + Math.Sqrt(x * x - 1.0));
    }

    /// <summary>
    /// Розв'язання рівняння Кеплера для гіперболічної орбіти.
    /// </summary>
    public static double KeplerSolverHyperbolicCase(double meanAnomaly, double eccentricity)
    {
        double delta = 1d;

        double F = Math.Log(2d * Math.Abs(meanAnomaly) / eccentricity + 1.8d);
        if (double.IsNaN(F) || double.IsInfinity(F))
        {
            return meanAnomaly;
        }

        while (delta > 1e-8 || delta < -1e-8)
        {
            delta = (eccentricity * Math.Sinh(F) - F - meanAnomaly) / (eccentricity * Math.Cosh(F) - 1d);
            F -= delta;
        }

        return F;
    }

    /// <summary>
    /// Розв'язання рівняння Кеплера для еліптичної орбіти.
    /// </summary>
    public static double SolveKeplersEquation(double meanAnomaly, double eccentricity)
    {
        int iterations = (int)(Math.Ceiling((eccentricity + 0.7d) * 1.25d)) << 1;
        double m = meanAnomaly;
        double esinE;
        double ecosE;
        double deltaE;
        double n;
        for (int i = 0; i < iterations; i++)
        {
            esinE = eccentricity * Math.Sin(m);
            ecosE = eccentricity * Math.Cos(m);
            deltaE = m - esinE - meanAnomaly;
            n = 1.0 - ecosE;
            m += -5d * deltaE / (n + Math.Sign(n) * Math.Sqrt(Math.Abs(16d * n * n - 20d * deltaE * esinE)));
        }

        return m;
    }

    /// <summary>
    /// Конвертація середньої аномалії в ексцентричну.
    /// </summary>
    public static double ConvertMeanToEccentricAnomaly(double meanAnomaly, double eccentricity)
    {
        if (eccentricity < 1.0)
        {
            return SolveKeplersEquation(meanAnomaly, eccentricity);
        }
        else if (eccentricity > 1.0)
        {
            return KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
        }
        else
        {
            var m = meanAnomaly * 2;
            var v = 12d * m + 4d * Math.Sqrt(4d + 9d * m * m);
            var pow = Math.Pow(v, 1d / 3d);
            var t = 0.5 * pow - 2 / pow;
            return 2 * Math.Atan(t);
        }
    }

    /// <summary>
    /// Конвертація ексцентричної аномалії в середню.
    /// </summary>
    public static double ConvertEccentricToMeanAnomaly(double eccentricAnomaly, double eccentricity)
    {
        if (eccentricity < 1.0)
        {
            return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
        }
        else if (eccentricity > 1.0)
        {
            return Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
        }
        else
        {
            var t = Math.Tan(eccentricAnomaly * 0.5);
            return (t + t * t * t / 3d) * 0.5d;
        }
    }

    /// <summary>
    /// Конвертація істинної аномалії в ексцентричну.
    /// </summary>
    public static double ConvertTrueToEccentricAnomaly(double trueAnomaly, double eccentricity)
    {
        if (double.IsNaN(eccentricity) || double.IsInfinity(eccentricity))
        {
            return trueAnomaly;
        }

        trueAnomaly %= PI_2;
        if (eccentricity < 1.0)
        {
            if (trueAnomaly < 0)
            {
                trueAnomaly += PI_2;
            }

            double cosT2 = Math.Cos(trueAnomaly);
            double eccAnom = Math.Acos((eccentricity + cosT2) / (1d + eccentricity * cosT2));
            if (trueAnomaly > Math.PI)
            {
                eccAnom = PI_2 - eccAnom;
            }

            return eccAnom;
        }
        else if (eccentricity > 1.0)
        {
            double cosT = Math.Cos(trueAnomaly);
            double eccAnom = Acosh((eccentricity + cosT) / (1d + eccentricity * cosT)) * Math.Sign(trueAnomaly);
            return eccAnom;
        }
        else
        {
            return trueAnomaly;
        }
    }

    /// <summary>
    /// Конвертація ексцентричної аномалії в істинну.
    /// </summary>
    public static double ConvertEccentricToTrueAnomaly(double eccentricAnomaly, double eccentricity)
    {
        if (eccentricity < 1.0)
        {
            double cosE = Math.Cos(eccentricAnomaly);
            double tAnom = Math.Acos((cosE - eccentricity) / (1d - eccentricity * cosE));
            if (eccentricAnomaly > PI)
            {
                tAnom = PI_2 - tAnom;
            }

            return tAnom;
        }
        else if (eccentricity > 1.0)
        {
            double tAnom = Math.Atan2(
                Math.Sqrt(eccentricity * eccentricity - 1d) * Math.Sinh(eccentricAnomaly),
                eccentricity - Math.Cosh(eccentricAnomaly)
            );
            return tAnom;
        }
        else
        {
            return eccentricAnomaly;
        }
    }

    /// <summary>
    /// Повертає істинну аномалію від поточної позиції до фокусу.
    /// </summary>
    public static double CalcTrueAnomalyForDistance(double distance, double eccentricity, double semiMajorAxis, double periapsisDistance)
    {
        if (eccentricity < 1.0)
        {
            return Math.Acos((semiMajorAxis * (1d - eccentricity * eccentricity) - distance) / (distance * eccentricity));
        }
        else if (eccentricity > 1.0)
        {
            return Math.Acos((semiMajorAxis * (eccentricity * eccentricity - 1d) - distance) / (distance * eccentricity));
        }
        else
        {
            return Math.Acos((periapsisDistance / distance) - 1d);
        }
    }
}
