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
    public float lrMinWidth = 1f;
    public float lrMaxWidth = 1f;

    private OrbitMover _moverReference;
    private Vector3d[] _orbitPoints;

#if UNITY_EDITOR
    [Header("Gizmo")]
    public bool ShowGizmo = true;
    public Color OrbitColor = Color.green;
    public bool ShowOrbitGizmo = true;
    public bool ShowVelocityGizmo = true;
    public bool ShowSOI = true;
#endif

    private void OnEnable()
    {
        _moverReference = GetComponent<OrbitMover>();
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!ShowGizmo || _moverReference == null)
            return;

        if (_moverReference.AttractorSettings == null || _moverReference.AttractorSettings.AttractorObject == null)
            return;

        if (ShowVelocityGizmo)
        {
            ShowVelocity();
        }

        if (ShowOrbitGizmo)
        {
            ShowOrbit();
        }

        if (ShowSOI)
        {
            ShowSphereOfInfluence();
        }
    }

    /// <summary>
    /// Візуалізує поточну швидкість об'єкта на гізмо.
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

        if (ShowOrbitGizmo)
        {
            Gizmos.color = OrbitColor;
            for (int i = 0; i < _orbitPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(_orbitPoints[i].ToVector3(), _orbitPoints[i + 1].ToVector3());
            }
        }

        if (lrReference != null)
        {
            lrReference.positionCount = _orbitPoints.Length;
            for (int i = 0; i < _orbitPoints.Length; i++)
            {
                lrReference.SetPosition(i, _orbitPoints[i].ToVector3());
            }
            lrReference.startColor = OrbitColor;
            lrReference.endColor = OrbitColor;

            Camera cam = Camera.main;
            if (cam != null)
            {
                float distance = Vector3.Distance(cam.transform.position, transform.position);
                float width = Mathf.Clamp(0.1f / distance, lrMinWidth, lrMaxWidth);
                lrReference.startWidth = width;
                lrReference.endWidth = width;
            }
        }
    }

    private void ShowSphereOfInfluence()
    {
        double soiRadius = _moverReference.orbitData.SphereOfInfluenceRadius;
        if (soiRadius <= 0) return;

        Vector3 soiCenter = _moverReference.transform.position;
        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.1f);
        Gizmos.DrawSphere(soiCenter, (float)soiRadius);
    }

#endif
}
