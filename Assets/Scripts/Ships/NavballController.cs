using UnityEngine;

[ExecuteAlways]
public class NavballController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Трансформ корпусу корабля")]
    public Transform shipBody;
    [Tooltip("Центр навігаційної кулі (точка, від якої дивимося на корабель)")]
    public Transform centerBody;
    [Tooltip("Внутрішній NavBall (маркер «вперед/назад»)")]
    public Transform navBallInner;
    [Tooltip("Зовнішній гімбал NavBall")]
    public Transform navBallGimbal;

    [Header("Smoothing")]
    [Range(0f, 1f)]
    [Tooltip("0 — швидка реакція, 1 — максимально плавно")]
    public float smoothFactor = 0.15f;

    [Header("Initial Offsets (Euler degrees)")]
    [Tooltip("Додатковий кут обертання навколо внутрішнього шару")]
    public Vector3 innerEulerOffset = Vector3.zero;
    [Tooltip("Додатковий кут обертання навколо зовнішнього гімбала")]
    public Vector3 gimbalEulerOffset = Vector3.zero;

    void LateUpdate()
    {
        if (shipBody == null || centerBody == null || navBallInner == null || navBallGimbal == null)
            return;

        // === Inner NavBall: дивиться на shipBody з центру centerBody ===
        Quaternion faceShip = Quaternion.LookRotation(
            shipBody.position - centerBody.position,
            centerBody.up
        );
        faceShip = Quaternion.Inverse(faceShip);

        // застосовуємо користувацький зсув
        Quaternion innerOffset = Quaternion.Euler(innerEulerOffset);
        Quaternion targetInner = faceShip * innerOffset;

        // плавно змінюємо локальний оберт
        navBallInner.localRotation = Quaternion.Slerp(
            navBallInner.localRotation,
            targetInner,
            1f - smoothFactor
        );


        // === Outer Gimbal: під загальний оберт корабля ===
        Quaternion shipRot = shipBody.rotation;
        shipRot.z = -shipRot.z; // якщо досі потрібна інверсія Z

        // додаємо Euler‑зсув до гімбала
        Quaternion gimbalOffset = Quaternion.Euler(gimbalEulerOffset);
        Quaternion targetGimbal = shipRot * gimbalOffset;

        navBallGimbal.localRotation = Quaternion.Slerp(
            navBallGimbal.localRotation,
            targetGimbal,
            1f - smoothFactor
        );
    }
}
