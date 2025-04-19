using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitalLauncher : MonoBehaviour
{
    public Transform planet; // Тіло, навколо якого орбіта
    public float altitudeAboveSurface = 2f; // Висота над поверхнею планети

    private Rigidbody rb;
    private Rigidbody planetrb;
    private GravityConfig config;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        planetrb = planet.GetComponent<Rigidbody>();

        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
            Debug.LogError("GravityConfig is missing.");

        // Розрахунок відстані від центру планети до об'єкта
        float planetRadius = planet.localScale.x / 2f; // якщо масштаб в localScale
        float orbitalRadius = planetRadius + altitudeAboveSurface;

        // Виставляємо об'єкт на правильну позицію
        Vector3 upDirection = (transform.position - planet.position).normalized;
        transform.position = planet.position + upDirection * orbitalRadius;

        // Розрахунок орбітальної швидкості
        float orbitalVelocity = Mathf.Sqrt(config.gravitationalConstant * planetrb.mass / orbitalRadius);

        // Орієнтуємо напрям горизонтально (перпендикуляр до сили тяжіння)
        Vector3 orbitalDirection = Vector3.Cross(upDirection, Vector3.forward).normalized;
        if (orbitalDirection == Vector3.zero) // страхування, якщо forward співнаправлений з up
        {
            orbitalDirection = Vector3.Cross(upDirection, Vector3.right).normalized;
        }

        // Встановлюємо початкову швидкість
        rb.velocity = orbitalDirection * orbitalVelocity;

        Debug.Log($"Запуск на орбіту! Орбітальна швидкість: {orbitalVelocity}, Напрям: {orbitalDirection}");
    }
}
