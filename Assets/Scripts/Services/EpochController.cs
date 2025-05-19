using System;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EpochController : MonoBehaviour, ITimeScalable
{
    [Header("UI Inputs")]
    public TMP_InputField YearField;
    public TMP_InputField DayField;
    public TMP_InputField HourField;
    public TMP_InputField MinuteField;
    public TMP_InputField SecondField;
    public Slider EpochSlider;
    public Button ApplyButton;

    [Header("UI Labels")]
    public TextMeshProUGUI YearLabel;
    public TextMeshProUGUI DayLabel;
    public TextMeshProUGUI HourLabel;
    public TextMeshProUGUI MinuteLabel;
    public TextMeshProUGUI SecondLabel;
    public TextMeshProUGUI WarpSpeedLabel;
    public TextMeshProUGUI PlanetInfoLabel;

    public float TimeScale = 1f;

    private const double SecondsPerYear = 365 * 24 * 3600;
    private const double SecondsPerDay = 24 * 3600;

    public static EpochController Instance { get; private set; }

    private readonly List<IEpochReceiver> epochListeners = new();

    private double _currentEpochSeconds = 0.0;
    public double CurrentEpoch => _currentEpochSeconds;

    private void Awake()
    {
        _currentEpochSeconds = 0.0;

        if (ApplyButton != null)
            ApplyButton.onClick.AddListener(OnApplyClicked);

        if (EpochSlider != null)
            EpochSlider.onValueChanged.AddListener(OnSliderValueChanged);

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if (TimeWarpManager.Instance != null)
        {
            TimeWarpManager.Instance.Register(this);
        }

        Instance = this;
        SetEpoch(_currentEpochSeconds);
    }

    public void OnTimeScaleChanged(float scale)
    {
        TimeScale = scale;
    }

    private void OnSliderValueChanged(float sliderValue)
    {
        double newEpoch = sliderValue;
        double delta = newEpoch - _currentEpochSeconds;
        if (Math.Abs(delta) < double.Epsilon) return;
        _currentEpochSeconds = newEpoch;
        SetEpoch(delta);
    }

    private void OnApplyClicked()
    {
        if (!int.TryParse(YearField.text, out int year) ||
            !int.TryParse(DayField.text, out int day) ||
            !int.TryParse(HourField.text, out int hour) ||
            !int.TryParse(MinuteField.text, out int minute) ||
            !int.TryParse(SecondField.text, out int second))
        {
            Debug.LogWarning("Wrong epoch time input");
            return;
        }

        double newEpochSec = (double)year * 365 * 24 * 3600 + (double)day * 24 * 3600 + (double)hour * 3600 + minute * 60 + second;
        double delta = newEpochSec - _currentEpochSeconds;
        if (Math.Abs(delta) < double.Epsilon) return;
        SetEpoch(delta);

        _currentEpochSeconds = newEpochSec;
        Debug.Log($"Epoch updated: delta = {delta} sec, total = {_currentEpochSeconds} sec");
    }

    public void SetEpoch(double time)
    {
        foreach (var receiver in epochListeners)
            receiver.UpdateEpoch(time);
    }

    private void Update()
    {
        _currentEpochSeconds += Time.deltaTime * TimeScale;
        if (EpochSlider != null)
            EpochSlider.SetValueWithoutNotify((float)_currentEpochSeconds);
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        double remaining = _currentEpochSeconds;

        int years = (int)(remaining / SecondsPerYear);
        remaining %= SecondsPerYear;

        int days = (int)(remaining / SecondsPerDay);
        remaining %= SecondsPerDay;

        int hours = (int)(remaining / 3600);
        remaining %= 3600;

        int minutes = (int)(remaining / 60);
        double seconds = remaining % 60;

        YearLabel.text = $"Year: {years}";
        DayLabel.text = $"Day: {days}";
        HourLabel.text = $"Hour: {hours}";
        MinuteLabel.text = $"Min: {minutes}";
        SecondLabel.text = $"Sec: {seconds:F2}";
        WarpSpeedLabel.text = $"x: {TimeWarpManager.Instance.CurrentTimeScale:F2}";
        UpdatePlanetInfo();

    }

    public void UpdatePlanetInfo()
    {
        var planetTarget = Camera.main.GetComponent<CameraController>().target;
        var planetMover = planetTarget.GetComponent<OrbitMover>();
        var planetTargetInfo = planetMover.orbitData;

        PlanetInfoLabel.text = $"Planet: {planetTarget.name}\n" +
                               $"Apoapsis: {planetTargetInfo.Apoapsis.magnitude:F2}\n" +
                               $"Periapsis: {planetTargetInfo.Periapsis.magnitude:F2}\n" +
                               $"Eccentricity: {planetTargetInfo.Eccentricity:F2}\n" +
                               $"MeanAnomaly: {planetTargetInfo.MeanAnomaly:F2}\n" +
                               $"Velocity: {planetTargetInfo.velocityRelativeToAttractor.magnitude:F2}\n" +
                               $"Period: {planetTargetInfo.Period / SecondsPerDay:F2}";
    }

    public void Register(IEpochReceiver listener) => epochListeners.Add(listener);
}