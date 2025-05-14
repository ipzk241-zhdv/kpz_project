using System.Collections.Generic;
using UnityEngine;

public class TimeWarpManager : MonoBehaviour
{
    public static TimeWarpManager Instance { get; private set; }

    [Header("Time Warp Settings")]
    public float[] warpSpeeds = { 0.03125f, 0.0625f, 0.125f, 0.25f, 0.5f, 1f, 2f, 5f, 10f, 50f, 100f, 200f, 500f, 1000f, 2000f };
    private int currentIndex = 5;

    [SerializeField] private double gravityConstant = 6.6743e-11;
    [SerializeField] private float timeScale = 1f;

    private readonly List<ITimeScalable> timeScaleListeners = new();
    private readonly List<IGravityConstantReceiver> gravityReceivers = new();

    public float CurrentTimeScale => timeScale;
    public double CurrentG => gravityConstant;

    private float lerpDuration = 1f;
    private float lerpStartTime;
    private float startTimeScale;
    private float targetTimeScale;
    private bool isLerping = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetTimeScale(warpSpeeds[currentIndex]);
        SetGravityConstant(gravityConstant);
    }

    private void OnValidate()
    {
        if (Instance == this)
        {
            SetTimeScale(timeScale);
            SetGravityConstant(gravityConstant);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma)) ChangeWarp(-1);
        else if (Input.GetKeyDown(KeyCode.Period)) ChangeWarp(1);

        if (isLerping)
        {
            float t = (Time.time - lerpStartTime) / lerpDuration;
            if (t >= 1f)
            {
                t = 1f;
                isLerping = false;
            }

            float newScale = Mathf.Lerp(startTimeScale, targetTimeScale, t);
            SetTimeScale(newScale);
        }
    }

    private void ChangeWarp(int direction)
    {
        int newIndex = Mathf.Clamp(currentIndex + direction, 0, warpSpeeds.Length - 1);
        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;

            startTimeScale = timeScale;
            targetTimeScale = warpSpeeds[currentIndex];
            lerpStartTime = Time.time;
            isLerping = true;

            Debug.Log($"Target Time Warp: x{targetTimeScale}");
        }
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
        foreach (var listener in timeScaleListeners)
            listener.OnTimeScaleChanged(timeScale);
    }

    public void SetGravityConstant(double g)
    {
        gravityConstant = g;
        foreach (var receiver in gravityReceivers)
            receiver.OnGravityConstantChanged(g);
    }

    public void Register(ITimeScalable listener) => timeScaleListeners.Add(listener);
    public void Register(IGravityConstantReceiver receiver) => gravityReceivers.Add(receiver);
}
