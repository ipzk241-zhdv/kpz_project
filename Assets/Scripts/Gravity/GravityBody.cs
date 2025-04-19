using System.Collections.Generic;
using UnityEngine;

public class GravityBody : MonoBehaviour
{
    // Список всех планет/солнечных систем, в чьём триггере мы сейчас находимся
    private readonly List<GravityPlanet> gravitySources = new List<GravityPlanet>();

    private Rigidbody rb;
    private GravityConfig config;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        config = GravityConfig.Instance
                 ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
            Debug.LogError("GravityConfig is missing.");
    }

    // Когда входим в зону триггера планеты/солнца — добавляем её в список
    private void OnTriggerEnter(Collider other)
    {
        var gp = other.GetComponent<GravityPlanet>();
        if (gp != null && !gravitySources.Contains(gp))
            gravitySources.Add(gp);
    }

    // Когда выходим из зоны — убираем
    private void OnTriggerExit(Collider other)
    {
        var gp = other.GetComponent<GravityPlanet>();
        if (gp != null)
            gravitySources.Remove(gp);
    }

    void FixedUpdate()
    {
        if (config == null || gravitySources.Count == 0)
            return;

        double bestForce = 0;
        GravityPlanet bestSource = null;

        // масса этого тела
        double m1 = rb.mass;
        double G = config.gravitationalConstant; // km^3 / t / s^2

        // Ищем источник с максимальной F = G·m1·m2 / r²
        foreach (var gp in gravitySources)
        {
            double m2 = gp.GetMass();
            float dist = Vector3.Distance(transform.position, gp.transform.position);

            if (dist <= 0f) continue;

            double force = G * (m1 * m2) / (dist * dist);
            if (force > bestForce)
            {
                bestForce = force;
                bestSource = gp;
                TrajectoryPredictor tr = GetComponent<TrajectoryPredictor>();
                if (tr != null)
                {
                    tr.gravitySource = bestSource;
                }
            }
        }

        if (bestSource != null)
        {
            // Применяем гравитацию от лучшего источника
            Vector3 dir = bestSource.GetGravityDirection(transform.position);
            rb.AddForce(dir * (float)bestForce);

            Debug.DrawRay(transform.position, dir * Mathf.Log10((float)bestForce), Color.red);
        }
    }
}
