using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiptoBlackUI : MonoBehaviour
{

    public Image fadeImage;

    public float fadeDuration = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp the alpha from 1 (black) to 0 (transparent)
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

    }
}
