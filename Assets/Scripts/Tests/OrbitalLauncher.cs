using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitalLauncher : MonoBehaviour
{
    public Transform planet;
    public float altitudeAboveSurface = 2f;

    private Rigidbody rb;
    private Rigidbody planetrb;
    private GravityConfig config;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        planetrb = planet.GetComponent<Rigidbody>();
        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
        {
            Debug.LogError("GravityConfig is missing.");
            return;
        }

        // Обчислення параметрів орбіти
        float planetRadius = planet.localScale.x / 2f;
        float orbitalRadius = planetRadius + altitudeAboveSurface;

        Vector3 upDirection = (transform.position - planet.position).normalized;
        if (upDirection == Vector3.zero)
            upDirection = Vector3.up;

        // Переміщення на орбіту в площині XZ
        upDirection = Vector3.right; // переорієнтація вгору по Y
        transform.position = planet.position + upDirection * orbitalRadius;

        float orbitalVelocity = Mathf.Sqrt(config.gravitationalConstant * planetrb.mass / orbitalRadius);

        Vector3 orbitalDirection = Vector3.Cross(upDirection, Vector3.forward).normalized;
        if (orbitalDirection == Vector3.zero)
            orbitalDirection = Vector3.Cross(upDirection, Vector3.right).normalized;

        // Додаємо швидкість планети
        rb.velocity = planetrb.velocity + orbitalDirection * orbitalVelocity;

        Debug.Log($"Запуск на орбіту! Швидкість: {orbitalVelocity}, Напрям: {orbitalDirection}");
    }
}
