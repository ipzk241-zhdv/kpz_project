using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GravityPlanet : MonoBehaviour
{
    [Header("Параметры")]
    [Tooltip("Масса в тоннах")]
    public double Mass = 5.291e22;   

    [Tooltip("Радиус тела в км")]
    public float Radius = 600f;         

    [Header("Атмосферные профили")]
    [Tooltip("Плотность атмосферы vs высота (km)")]
    public AnimationCurve densityByAltitude = new AnimationCurve(
        new Keyframe(0, 1.225f),     // на поверхности (kg/m³)
        new Keyframe(50, 0.1f),       // 50 км
        new Keyframe(100, 0f)          // за границей атмосферы
    );

    [Tooltip("Температура vs высота (km)")]
    public AnimationCurve temperatureByAltitude = new AnimationCurve(
        new Keyframe(0, 288f),        // 15 °C на поверхности
        new Keyframe(11, 216f),        // стратосфера
        new Keyframe(20, 217f),
        new Keyframe(47, 270f),
        new Keyframe(53, 270f),
        new Keyframe(80, 190f)
    );

    private SphereCollider col;

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        // автоматически ставим размер коллайдера по радиусу?
        //col.radius = Radius;
    }

    /// <summary>
    /// Направление гравитации к центру тела
    /// </summary>
    public Vector3 GetGravityDirection(Vector3 position)
        => (transform.position - position).normalized;

    /// <summary>
    /// Возвращает массу (для формулы F = G·m1·m2 / r²)
    /// </summary>
    public double GetMass() => Mass;

    /// <summary>
    /// Плотность на заданной высоте (в км)
    /// </summary>
    public float GetAtmosphericDensity(float altitudeKm)
        => densityByAltitude.Evaluate(altitudeKm);

    /// <summary>
    /// Температура на высоте (в км)
    /// </summary>
    public float GetTemperature(float altitudeKm)
        => temperatureByAltitude.Evaluate(altitudeKm);
}
