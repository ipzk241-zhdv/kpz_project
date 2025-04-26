using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OrbitMover))]
[ExecuteAlways]
public class OrbitDisplay : MonoBehaviour
{
    public int OrbitPointsCount = 30;
    public LineRenderer lrReference;

    private OrbitMover _moverReference;
    private Vector3d[] _orbitPoints;

#if UNITY_EDITOR
    [Header("Gizmo")]
    public bool ShowGizmo = true;
    public bool ShowOrbitGizmo = true;
    public bool ShowVelocityGizmo = true;
#endif

    private void OnEnable()
    {
        _moverReference = GetComponent<OrbitMover>();
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (ShowGizmo)
        {
            if (ShowOrbitGizmo && _moverReference != null)
            {
                if (_moverReference.AttractorSettings != null && _moverReference.AttractorSettings.AttractorObject != null)
                {
                    if (ShowVelocityGizmo)
                    {
                        ShowVelocity();
                    }

                    if (ShowOrbitGizmo)
                    {
                        ShowOrbit();
                    }
                }
            }
        }

    }

    /// <summary>
    /// Візуалізує потчону швидкість об'єкта на гізмо.
    /// </summary>
    private void ShowVelocity() 
    {
        Gizmos.color = new Color(0f, 0.5f, 1f);
        Vector3d velocity = _moverReference.orbitData.GetVelocityAtEccentricAnomaly(_moverReference.orbitData.EccentricAnomaly);
        
        if (_moverReference.VelocityHandleLengthScale > 0)
        {
            velocity *= _moverReference.VelocityHandleLengthScale;
        }

        var currentPosition = transform.position;
        Gizmos.DrawLine(currentPosition, currentPosition + velocity.ToVector3());
    }

    private void ShowOrbit() { }
#endif
}
