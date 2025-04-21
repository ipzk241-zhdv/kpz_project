using UnityEngine;

[RequireComponent(typeof(CelestialBody))]
public class OrbitalMotion : MonoBehaviour
{
    public CelestialBody centralBody;
    public bool tidalLock = false;

    private CelestialBody self;
    private float orbitTimer;
    private float rotationAngle;

    void Start()
    {
        rotationAngle = 0f;
        self = GetComponent<CelestialBody>();

        // ==== Ініціалізація orbitTimer ====
        if (centralBody != null)
        {
            // шукаємо найближчий момент часу
            float bestTimer = 0f;
            float bestDist = float.MaxValue;
            Vector3 curPos = transform.position;
            int samples = 360; // можна знизити для швидкості

            float period = self.orbitalPeriod;
            float meanMotion = 2f * Mathf.PI / period;

            for (int i = 0; i < samples; i++)
            {
                float t = period * i / samples;
                Vector3 p = GetWorldPositionAtTime(t);
                float d = Vector3.Distance(curPos, p);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestTimer = t;
                }
            }
            orbitTimer = bestTimer;
        }
    }

    void FixedUpdate()
    {
        if (centralBody == null) return;

        orbitTimer += Time.fixedDeltaTime;
        transform.position = GetWorldPositionAtTime(orbitTimer);

        // обертання навколо своєї осі / tidal lock
        if (!tidalLock && self.rotationPeriod > 0f)
        {
            float rotSpeed = 360f / self.rotationPeriod;
            rotationAngle += rotSpeed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);
        }
        else if (tidalLock)
        {
            Vector3 dir = centralBody.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    /// <summary>
    /// Повертає світову позицію на орбіті через timeOffset секунд від початку (meanAnomalyAtEpoch).
    /// </summary>
    public Vector3 GetWorldPositionAtTime(float timeOffset)
    {
        if (self == null)
            self = GetComponent<CelestialBody>();

        float period = self.orbitalPeriod;
        float meanMotion = 2f * Mathf.PI / period;
        float M = self.meanAnomalyAtEpoch + meanMotion * timeOffset;
        float E = SolveKepler(M, self.eccentricity);

        float a = self.semiMajorAxis;
        float e = self.eccentricity;
        float x = a * (Mathf.Cos(E) - e);
        float z = a * Mathf.Sqrt(1 - e * e) * Mathf.Sin(E);
        Vector3 posOrbitalPlane = new Vector3(x, 0f, z);

        Quaternion incl = Quaternion.Euler(self.inclination, 0f, 0f);
        Quaternion arg = Quaternion.Euler(0f, self.argumentOfPeriapsis, 0f);
        Quaternion node = Quaternion.Euler(0f, self.longitudeOfAscendingNode, 0f);
        Quaternion rot = node * incl * arg;

        return centralBody.transform.position + (rot * posOrbitalPlane);
    }

    float SolveKepler(float M, float e, int iterations = 5)
    {
        float E = M;
        for (int i = 0; i < iterations; i++)
            E -= (E - e * Mathf.Sin(E) - M) / (1 - e * Mathf.Cos(E));
        return E;
    }
}
