using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(OrbitMover))]
[ExecuteAlways]
public class OrbitDisplay : MonoBehaviour
{
    [Min(2)]
    public int OrbitPointsCount = 30;
    public double maxDistance = 500d;
    public LineRenderer lrReference;

    private OrbitMover _moverReference;
    private Vector3d[] _orbitPoints;

#if UNITY_EDITOR
    [Header("Gizmo")]
    public bool ShowGizmo = true;
    public Color OrbitColor = Color.green;
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

        Vector3 currentPosition = transform.position;
        Gizmos.DrawLine(currentPosition, currentPosition + velocity.ToVector3());
    }

    private void ShowOrbit() 
    {
        Vector3d posGravitySource = new Vector3d(_moverReference.AttractorSettings.AttractorObject.transform.position);
        _moverReference.orbitData.GetOrbitPoints(ref _orbitPoints, OrbitPointsCount, posGravitySource, maxDistance);
        Gizmos.color = OrbitColor;
        for (int i = 0; i < _orbitPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(_orbitPoints[i].ToVector3(), _orbitPoints[i + 1].ToVector3());
        }
    }
#endif
}
