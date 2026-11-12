using UnityEngine;
using UnityEngine.Video;

public class FinalEnd : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer tutorialVideo;
    public VideoPlayer creditVideo; // Changed from GameObject to VideoPlayer

    [Header("UI/Container Objects")]
    public GameObject tutorialVideoContainer;
    public GameObject creditVideoContainer;

    private void Start()
    {
        // Setup initial state
        if (tutorialVideo != null)
        {
            if (tutorialVideoContainer != null) tutorialVideoContainer.SetActive(true);
            if (creditVideoContainer != null) creditVideoContainer.SetActive(false);

            tutorialVideo.loopPointReached += OnTutorialVideoFinished;
            tutorialVideo.Play();
        }
        else
        {
            Debug.LogWarning("No Tutorial VideoPlayer assigned!");
            HandleQuit();
        }
    }

    private void OnTutorialVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnTutorialVideoFinished;

        // Hide tutorial, show credits
        if (tutorialVideoContainer != null) tutorialVideoContainer.SetActive(false);

        if (creditVideo != null)
        {
            if (creditVideoContainer != null) creditVideoContainer.SetActive(true);

            creditVideo.loopPointReached += OnCreditVideoFinished;
            creditVideo.Play();
        }
        else
        {
            HandleQuit();
        }
    }

    private void OnCreditVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnCreditVideoFinished;
        HandleQuit();
    }

    private void HandleQuit()
    {
        Debug.Log("Exiting Application...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}