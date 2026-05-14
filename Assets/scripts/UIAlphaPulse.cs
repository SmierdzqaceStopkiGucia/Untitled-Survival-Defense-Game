using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAlphaPulse : MonoBehaviour
{
    [Header("Target UI")]
    public Graphic targetGraphic;

    [Header("Alpha Settings")]
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float alphaStep = 0.05f;

    [Header("Timing")]
    public float interval = 0.1f;
    public float waitAtZero = 1f;
    public float waitAtMax = 1f;

    [Header("Repeat")]
    public int repeatCount = 3;

    private bool increasing = true;

    void Start()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        Color color = targetGraphic.color;
        color.a = 0f;
        targetGraphic.color = color;

        StartCoroutine(AlphaLoop());
    }

    IEnumerator AlphaLoop()
    {
        int completedCycles = 0;

        while (completedCycles < repeatCount)
        {
            Color color = targetGraphic.color;

            if (increasing)
            {
                color.a += alphaStep;

                if (color.a >= maxAlpha)
                {
                    color.a = maxAlpha;
                    targetGraphic.color = color;

                    yield return new WaitForSeconds(waitAtMax);

                    increasing = false;
                }
            }
            else
            {
                color.a -= alphaStep;

                if (color.a <= 0f)
                {
                    color.a = 0f;
                    targetGraphic.color = color;

                    completedCycles++;

                    if (completedCycles >= repeatCount)
                        yield break;

                    yield return new WaitForSeconds(waitAtZero);

                    increasing = true;
                }
            }

            targetGraphic.color = color;
            yield return new WaitForSeconds(interval);
        }
    }
}