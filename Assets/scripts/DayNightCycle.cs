using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthInMinutes = 1.0f; // Real-world minutes for a full day
    [Range(0, 24)] public float currentHour = 12f; // Starts at 12 PM

    [Header("Sky Overlay Settings")]
    public Graphic nightOverlay; // Assign your black UI Image here
    [Range(0f, 1f)] public float maxNightAlpha = 0.8f; // How dark it gets at midnight

    private void Update()
    {
        UpdateTime();
        UpdateOverlayAlpha();
    }

    void UpdateTime()
    {
        // Calculate how many in-game hours pass per real-world second
        float hoursPerSecond = 24f / (dayLengthInMinutes * 60f);
        currentHour += Time.deltaTime * hoursPerSecond;

        // Reset day after 24 hours
        if (currentHour >= 24f) currentHour = 0f;
    }

    void UpdateOverlayAlpha()
    {
        if (nightOverlay == null) return;

        // Logic: 
        // 12 PM (Hour 12) should be Alpha 0
        // 12 AM (Hour 0/24) should be Max Alpha
        
        // Use a Sine wave to create a smooth transition
        // We shift the curve so that the peak (1.0) is at Hour 0 and the trough (0.0) is at Hour 12
        float timeToAlpha = (currentHour / 24f) * Mathf.PI * 2;
        float alphaPercentage = (Mathf.Cos(timeToAlpha) + 1f) / 2f;

        Color color = nightOverlay.color;
        color.a = alphaPercentage * maxNightAlpha;
        nightOverlay.color = color;
    }

    // Optional: Helper to get a readable time string for your UI
    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentHour);
        int minutes = Mathf.FloorToInt((currentHour - hours) * 60);
        string period = hours >= 12 ? "PM" : "AM";
        int displayHour = hours % 12;
        if (displayHour == 0) displayHour = 12;

        return string.Format("{0:00}:{1:00} {2}", displayHour, minutes, period);
    }
}