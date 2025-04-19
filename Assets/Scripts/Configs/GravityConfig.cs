using UnityEngine;

[CreateAssetMenu(fileName = "GravityConfig", menuName = "Configs/GravityConfig")]
public class GravityConfig : ScriptableObject
{
    [Header("Физические константы")]
    [Tooltip("G в km³ / t / s²")]
    public float gravitationalConstant = 6.67430e-20f;

    public static GravityConfig Instance;

    private void OnEnable() => Instance = this;
}
