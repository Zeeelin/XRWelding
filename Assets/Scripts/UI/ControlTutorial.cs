using System.Collections;
using UnityEngine;

public class ControlTutorial : MonoBehaviour
{
    public static ControlTutorial instance;
    public Animator tutorialAnimator;
    public bool startTutorial;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }


    void Start()
    {
        startTutorial = false;
        StartCoroutine(WaitForIntroToFinish());
    }

    // Update is called once per frame
    void Update()
    {
        if (startTutorial) {
            startTutorial = false;
            tutorialAnimator.SetBool("HasMove", true);
        }
    }
    public void StopMoveTutorial()
    {
        tutorialAnimator.SetBool("HasMove", false);
    }

    private IEnumerator WaitForIntroToFinish() {
        yield return new WaitForSecondsRealtime(10f);
        startTutorial=true;
    }

    public void PlayGrabTutorial() {
        tutorialAnimator.SetBool("HasGrab", true);
    }

    public void StopGrabTutorial()
    {
        tutorialAnimator.SetBool("HasGrab", false);
    }

    public void PlayTriggerTutorial()
    {
        tutorialAnimator.SetBool("HasTrigger", true);
    }
    public void StopTriggerTutorial()
    {
        tutorialAnimator.SetBool("HasTrigger", false);
    }


}
