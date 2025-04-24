using System;

public class Utils
{
    public const double PI_2 = 6.2831853071796d;
    public const double PI = 3.14159265358979;
    public const double Deg2Rad = 0.017453292519943d;
    public const double Rad2Deg = 57.295779513082d;

    /// <summary>
    /// Розв'язання рівняння Кеплера: M = E - e * sin(E)
    /// методом Ньютона, щоб знайти ексцентричну аномалію
    /// </summary>
    public static double SolveKeplersEquation(double M, double e, int maxIterations = 20, double tolerance = 1e-8)
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

    public static double ConvertMeanToEccentricAnomaly(double meanAnomaly, double eccentricity)
    {
        if (eccentricity < 1.0)
        {
            return SolveKeplersEquation(meanAnomaly, eccentricity);
        }
        else if (eccentricity > 1.0)
        {
            return 0;
            // TODO
            //return KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
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
}
