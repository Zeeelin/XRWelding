/*using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroMenu : MonoBehaviour
{
    public GameObject[] IntroMenuScene;
    public GameObject[] tutorialObject;

    public void LoadNextScene()
    {
        StartCoroutine(PlayTutorial());
    }

    private IEnumerator PlayTutorial() {
        foreach (var i in IntroMenuScene) { 
            i.gameObject.SetActive(false);
        }
        foreach (var i in tutorialObject)
        {
            i.gameObject.SetActive(true);
        }
        yield return new WaitForSecondsRealtime(8f);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

}
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // You need this namespace for VideoPlayer!

public class IntroMenu : MonoBehaviour
{
    public GameObject[] IntroMenuScene;
    public GameObject[] tutorialObject;

    [Header("Video Settings")]
    public VideoPlayer tutorialVideo; // Drag your VideoPlayer here in the Inspector
    public GameObject tutorialVideoPlayer;

    private void Start()
    {
        tutorialVideoPlayer.SetActive(false);
    }
    public void LoadNextScene()
    {
        // 1. Swap the active objects
        foreach (var i in IntroMenuScene)
        {
            i.gameObject.SetActive(false);
        }
        foreach (var i in tutorialObject)
        {
            i.gameObject.SetActive(true);
        }
        StartCoroutine(WaitForFiveSec());


    }

    private IEnumerator WaitForFiveSec() {
        yield return new WaitForSecondsRealtime(8);
        if (tutorialVideo != null)
        {
            tutorialVideoPlayer.SetActive(true);
            // Tell the script to run "OnVideoFinished" when the video hits the end
            tutorialVideo.loopPointReached += OnVideoFinished;
            tutorialVideo.Play();
        }
        else
        {
            Debug.LogWarning("No VideoPlayer assigned! Skipping straight to the next scene.");
            GoToNextScene();
        }
    }

    // This method is automatically called when the video finishes
    private void OnVideoFinished(VideoPlayer vp)
    {
        // Always unsubscribe from the event to prevent memory leaks!
        vp.loopPointReached -= OnVideoFinished;

        GoToNextScene();
    }

    private void GoToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}