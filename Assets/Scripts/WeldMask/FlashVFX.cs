using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FlashVFX : MonoBehaviour
{
    public GameObject SaftyCanvas;
    public Image flashImage;
    public Volume globalVolume;
    public float maxExposure = 15f;
    public float fadeDuration = 5f;

    private ColorAdjustments colorAdjustments;
    private Coroutine flashCoroutine;
    private float originalExposure;

    void Start()
    {
        if (globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            originalExposure = colorAdjustments.postExposure.value;
        }
        else
        {
            Debug.LogError("No Color Adjustments found on the Volume!");
        }
    }
    public void TriggerFlash(float duration)
    {
        //SaftyCanvas.SetActive(true);
        //StartCoroutine(FlashRoutine(duration));
        if (colorAdjustments == null) return;

        // If a flash is already happening, stop it so they don't overlap and fight each other
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashAndFadeRoutine());
    }

    private IEnumerator FlashAndFadeRoutine()
    {

        colorAdjustments.postExposure.value = maxExposure;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / fadeDuration;
            colorAdjustments.postExposure.value = Mathf.Lerp(maxExposure, originalExposure, t);

            yield return null;
        }

        colorAdjustments.postExposure.value = originalExposure;
    }

    /*
    private IEnumerator FlashRoutine(float duration)
    {
        // Instant white
        flashImage.color = new Color(1, 1, 1, 1);

        // Hold for the duration
        yield return new WaitForSeconds(duration);

        float fadeTime = 0.5f;
        float elapsed = 0;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            flashImage.color = new Color(1, 1, 1, 1 - (elapsed / fadeTime));
            yield return null;
        }
        flashImage.color = new Color(1, 1, 1, 0);
        //Position might need to be changed, is called repeativly
        SoundManager.Instance.PlayMCLine(SoundManager.MCClip.FlashedVO);
        SaftyCanvas.SetActive(false);
    }
    */
}
