using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NightTransition : MonoBehaviour
{
    [Header("References")]
    public Image fadePanel;
    public TextMeshProUGUI fadeText;
    private DayNightCycle timeManager;
    private AudioSource audioSource;

    [Header("Audio")]
    public AudioClip textAppearanceSound; // Sound when text pops up

    [Header("Timing Settings")]
    public float panelFadeDuration = 1.0f; // How fast screen goes black
    public float textFadeDuration = 1.0f;  // How fast text appears
    public float stayDuration = 2.0f;      // How long to stay on the screen

    private bool hasTriggeredToday = false;

    void Start()
    {
        timeManager = Object.FindObjectOfType<DayNightCycle>();
        audioSource = gameObject.AddComponent<AudioSource>();

        // Start completely invisible
        SetAlpha(fadePanel, 0);
        SetAlpha(fadeText, 0);
    }

    void Update()
    {
        if (timeManager == null) return;

        // Check for 8 AM
        if (Mathf.FloorToInt(timeManager.currentHour) == 8 && !hasTriggeredToday)
        {
            StartCoroutine(DoNightEndSequence());
            hasTriggeredToday = true;
        }

        // Reset trigger at midday
        if (timeManager.currentHour > 12 && hasTriggeredToday)
        {
            hasTriggeredToday = false;
        }
    }

    IEnumerator DoNightEndSequence()
    {
        // 1. Fade Panel to Black ONLY
        yield return StartCoroutine(FadeGraphic(fadePanel, 1, panelFadeDuration));

        // 2. Small delay before text
        yield return new WaitForSeconds(0.5f);

        // 3. Play Sound and Fade Text In
        if (textAppearanceSound != null) audioSource.PlayOneShot(textAppearanceSound);
        yield return StartCoroutine(FadeGraphic(fadeText, 1, textFadeDuration));

        // 4. Wait for player to read
        yield return new WaitForSeconds(stayDuration);

        // 5. Fade both out together
        float timer = 0;
        while (timer < panelFadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / panelFadeDuration);
            SetAlpha(fadePanel, alpha);
            SetAlpha(fadeText, alpha);
            yield return null;
        }

        SetAlpha(fadePanel, 0);
        SetAlpha(fadeText, 0);
    }

    // Generic fade function that works for both Images and Text
    IEnumerator FadeGraphic(Graphic graphic, float targetAlpha, float duration)
    {
        float startAlpha = graphic.color.a;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            SetAlpha(graphic, newAlpha);
            yield return null;
        }
        SetAlpha(graphic, targetAlpha);
    }

    void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}