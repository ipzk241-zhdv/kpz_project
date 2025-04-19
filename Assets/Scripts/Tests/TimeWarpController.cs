using UnityEngine;

public class TimeWarpController : MonoBehaviour
{
    [Header("Time Warp Settings")]
    public float[] warpSpeeds = { 0.25f, 0.5f, 1f, 2f, 5f, 10f, 50f };
    private int currentIndex = 2; // ������ ��� 1x (��������� ��������)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma)) // < ��� ,
        {
            ChangeWarp(-1);
        }
        else if (Input.GetKeyDown(KeyCode.Period)) // > ��� .
        {
            ChangeWarp(1);
        }
    }

    void ChangeWarp(int direction)
    {
        currentIndex = Mathf.Clamp(currentIndex + direction, 0, warpSpeeds.Length - 1);
        Time.timeScale = warpSpeeds[currentIndex];
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // ��� ������ ��������� �������� ����
        Debug.Log($"Time Warp: x{warpSpeeds[currentIndex]}");
    }
}
