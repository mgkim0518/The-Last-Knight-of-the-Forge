using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IronGainView : MonoBehaviour
{
    [SerializeField] private Text text;
    [Header("결과 표시")]
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private float riseDistance = 20f;

    private bool isPreviewing;
    private Coroutine result_co;
    private Vector2 basePos;

    private void Awake()
    {
        basePos = text.rectTransform.anchoredPosition;
    }

    public void ShowPreview(int val)
    {
        StopResult();

        text.text = $"(+{val})";
        text.enabled = true;
        isPreviewing = true;
    }

    public void HidePreview()
    {
        if (!isPreviewing)
        {
            return;
        }

        text.enabled = false;
        isPreviewing = false;
    }

    public void ShowResult(int val)
    {
        StopResult();
        isPreviewing = false;

        text.text = "+" + val;
        Color text_c = text.color;
        text_c.a = 1;
        text.color = text_c;
        text.enabled = true;

        result_co = StartCoroutine(Fade_co());
    }

    private IEnumerator Fade_co()
    {
        Color start = text.color;

        yield return new WaitForSeconds(holdTime);

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);

            Color c = start;
            c.a = Mathf.Lerp(start.a, 0f, t);
            text.color = c;
            text.rectTransform.anchoredPosition = basePos + Vector2.up * (riseDistance * t);

            yield return null;
        }

        text.enabled = false;
        text.rectTransform.anchoredPosition = basePos;
        result_co = null;
    }

    private void StopResult()
    {
        if (result_co != null)
        {
            StopCoroutine(result_co);
            result_co = null;
        }

        Color text_c = text.color;
        text_c.a = 1;
        text.color = text_c;

        text.rectTransform.anchoredPosition = basePos;
    }
}
