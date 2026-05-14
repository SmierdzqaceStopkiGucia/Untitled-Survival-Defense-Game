using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClockUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform clockHand;
    public Image clockDial;
    private DayNightCycle timeManager;
    private AudioSource audioSource;

    [Header("Sprites")]
    public Sprite dayDialSprite;
    public Sprite nightDialSprite;

    [Header("Hourly Audio")]
    public AudioClip hourlySoundEffect;
    [Tooltip("List of hours (0-23) when the sound should play")]
    public List<int> triggerHours = new List<int> { 8, 12, 22 };

    private int lastProcessedHour = -1;

    private void Start()
    {
        timeManager = Object.FindObjectOfType<DayNightCycle>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (timeManager == null || clockHand == null) return;

        int currentHourInt = Mathf.FloorToInt(timeManager.currentHour);
        float hour = timeManager.currentHour;
        float progress = 0f;

        if (currentHourInt != lastProcessedHour)
        {
            if (triggerHours.Contains(currentHourInt))
            {
                PlayHourlySound();
            }
            lastProcessedHour = currentHourInt;
        }

        if (hour >= 8f && hour < 22f)
        {
            if (clockDial != null && dayDialSprite != null) clockDial.sprite = dayDialSprite;
            progress = (hour - 8f) / 14f;
        }
        else
        {
            if (clockDial != null && nightDialSprite != null) clockDial.sprite = nightDialSprite;
            float nightElapsed = (hour >= 22f) ? (hour - 22f) : (hour + 2f);
            progress = nightElapsed / 10f;
        }

        float angle = Mathf.Lerp(90f, -90f, progress);
        clockHand.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void PlayHourlySound()
    {
        if (hourlySoundEffect != null)
        {
            audioSource.PlayOneShot(hourlySoundEffect);
        }
    }
}