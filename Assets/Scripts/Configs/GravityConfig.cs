using UnityEngine;

/// <summary>
/// Глобальні константи гравітації, які можна підключати в інші скрипти.
/// Масштабування світу для зручного використання в Unity.
/// 
/// ==== Масштабні одиниці ====
/// ============================
/// </summary>
[CreateAssetMenu(fileName = "GravityConfig", menuName = "Configs/GravityConfig")]
public class GravityConfig : ScriptableObject
{
    [Header("Фізичні константи")]
    public float gravitationalConstant = 66.588f;

    public static GravityConfig Instance;

    private void OnEnable()
    {
        Instance = this;
    }
}
