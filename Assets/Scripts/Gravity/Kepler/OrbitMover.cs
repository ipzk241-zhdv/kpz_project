using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways]
[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(Transform))]
public class OrbitMover : MonoBehaviour, IScalableAndGravityReceiver
{
    [Header("Debug Thrust Settings")]
    public bool ApplyTestThrust = false;
    public float TestThrustForce = 0.01f;

    /// <summary>
    /// Налаштування гравітаційного джерела. Об'єкт притягання повинен бути призначений.
    /// </summary>
    public GravitySource AttractorSettings = new GravitySource();

    /// <summary>
    /// Об'єкт керування вектором швидкості.
    /// </summary>
    public Transform VelocityHandle;

    /// <summary>
    /// Масштаб довжини вектора швидкості.
    /// </summary>
    [Range(0f, 10f)]
    public float VelocityHandleLengthScale = 0f;

    /// <summary>
    /// Множник масштабу часу.
    /// </summary>
    public float TimeScale = 1f;

    /// <summary>
    /// Поточні дані орбіти.
    /// </summary>
    public OrbitData orbitData = new OrbitData();

    /// <summary>
    /// Вимкнення редагування орбіти в оновленнях.
    /// </summary>
    public bool LockOrbitEditing = false;

#if UNITY_EDITOR
    private bool _debugErrorDisplayed = false;
#endif

    private Coroutine _updateRoutine;

    private bool IsReferencesAsigned
    {
        get { return AttractorSettings != null && AttractorSettings.AttractorObject != null; }
    }

    private void OnEnable()
    {
        if (!LockOrbitEditing)
        {
            ForceUpdateOrbitData();
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        if (_updateRoutine != null)
        {
            StopCoroutine(_updateRoutine);
        }

        _updateRoutine = StartCoroutine(OrbitUpdateLoop());
    }

    private void Start()
    {
        TimeWarpManager.Instance.Register(this);
        TimeWarpManager.Instance.Register(AttractorSettings);
        OnTimeScaleChanged(TimeWarpManager.Instance.CurrentTimeScale);
        AttractorSettings.OnGravityConstantChanged(TimeWarpManager.Instance.CurrentG);
    }

    public void OnTimeScaleChanged(float timeScale)
    {
        TimeScale = timeScale;
    }

    private void OnDisable()
    {
        if (_updateRoutine != null)
        {
            StopCoroutine(_updateRoutine);
            _updateRoutine = null;
        }
    }

    private void Update()
    {
        if (IsReferencesAsigned)
        {
            if (!LockOrbitEditing)
            {
                var pos = transform.position - AttractorSettings.AttractorObject.position;
                Vector3d position = new Vector3d(pos.x, pos.y, pos.z);

                bool velocityHandleChanged = false;
                if (VelocityHandle != null)
                {
                    Vector3 velocity = GetVelocityHandleDisplayedVelocity();
                    if (velocity != orbitData.velocityRelativeToAttractor.ToVector3())
                    {
                        velocityHandleChanged = true;
                    }
                }

                if (position != orbitData.positionRelativeToAttractor ||
                    velocityHandleChanged ||
                    orbitData.GravConst != AttractorSettings.GravityConstant ||
                    orbitData.AttractorMass != AttractorSettings.AttractorMass)
                {
                    ForceUpdateOrbitData();
                }
            }
        }
        else
        {
#if UNITY_EDITOR
            if (AttractorSettings.AttractorObject == null)
            {
                if (!_debugErrorDisplayed)
                {
                    _debugErrorDisplayed = true;
                    if (Application.isPlaying)
                    {
                        Debug.LogError("OrbitMover: Attractor reference not asigned", context: gameObject);
                    }
                    else
                    {
                        Debug.Log("OrbitMover: Attractor reference not asigned", context: gameObject);
                    }
                }
            }
            else
            {
                _debugErrorDisplayed = false;
            }
#endif
        }
    }

    /// <summary>
    /// Цикл оновлення орбітального руху в реальному часі.
    /// </summary>
    private IEnumerator OrbitUpdateLoop()
    {
        while (true)
        {
            if (IsReferencesAsigned)
            {
                if (!orbitData.IsValidOrbit)
                {
                    orbitData.CalculateOrbitStateFromOrbitalVectors();
                }

                if (orbitData.IsValidOrbit)
                {
                    orbitData.UpdateOrbitDataByTime(Time.deltaTime * TimeScale);
                    ForceUpdateViewFromInternalState();
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Отримує швидкість, яка відображається через VelocityHandle.
    /// </summary>
    public Vector3 GetVelocityHandleDisplayedVelocity()
    {
        if (VelocityHandle != null)
        {
            Vector3 velocity = VelocityHandle.position - transform.position;
            if (VelocityHandleLengthScale > 0 && !float.IsNaN(VelocityHandleLengthScale) && !float.IsInfinity(VelocityHandleLengthScale))
            {
                velocity /= VelocityHandleLengthScale;
            }

            return velocity;
        }

        return new Vector3();
    }

    /// <summary>
    /// Оновлює внутрішні дані орбіти згідно поточного положення та швидкості.
    /// </summary>
    public void ForceUpdateOrbitData()
    {
        if (IsReferencesAsigned)
        {
            orbitData.AttractorMass = AttractorSettings.AttractorMass;
            orbitData.GravConst = AttractorSettings.GravityConstant;

            var pos = transform.position - AttractorSettings.AttractorObject.position;
            orbitData.positionRelativeToAttractor = new Vector3d(pos.x, pos.y, pos.z);
            if (VelocityHandle != null)
            {
                Vector3 velocity = GetVelocityHandleDisplayedVelocity();
                orbitData.velocityRelativeToAttractor = new Vector3d(velocity.x, velocity.y, velocity.z);
            }

            orbitData.CalculateOrbitStateFromOrbitalVectors();
        }
    }

    /// <summary>
    /// Оновлює положення VelocityHandle на основі внутрішнього стану.
    /// </summary>
    public void ForceUpdateVelocityHandleFromInternalState()
    {
        if (VelocityHandle != null)
        {
            Vector3 velocityRelativePosition = orbitData.velocityRelativeToAttractor.ToVector3();
            if (VelocityHandleLengthScale > 0 && !float.IsNaN(VelocityHandleLengthScale) && !float.IsInfinity(VelocityHandleLengthScale))
            {
                velocityRelativePosition *= VelocityHandleLengthScale;
            }

            VelocityHandle.position = transform.position + velocityRelativePosition;
        }
    }

    /// <summary>
    /// Оновлює позицію об'єкта згідно розрахованої орбіти.
    /// </summary>
    public void ForceUpdateViewFromInternalState()
    {
        var pos = orbitData.positionRelativeToAttractor.ToVector3();
        transform.position = AttractorSettings.AttractorObject.position + pos;
        ForceUpdateVelocityHandleFromInternalState();
    }
}
