using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameEnd : MonoBehaviour
{
    public Volume globalVolume;
    public float fadeDuration = 4f;
    public float blackExposure = -15f;
    
    // This prevents the player from accidentally triggering the quit sequence twice
    private bool isQuitting = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && !isQuitting)
        {
            isQuitting = true;
            Debug.Log("Try quit application");
            StartCoroutine(FadeAndQuitRoutine());
        }
    }

    private IEnumerator FadeAndQuitRoutine()
    {
        if (globalVolume != null && globalVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            float startExposure = colorAdjustments.postExposure.value;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;

                colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, blackExposure, t);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Global Volume missing on GameEnd. Waiting without fading.");
            yield return new WaitForSeconds(fadeDuration);
        }

        Debug.Log("Fade complete. Quitting game.");

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);

        /*
                Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif*/
    }
  
}
