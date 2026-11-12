using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Oculus.Interaction;

public class SubmissionSwitch: MonoBehaviour
{
    public SubmissionArea subArea;

    public Animator SubmitAreaAnimator;

    public float duration = 5f;
    public float elapsedTime = 0f;
    public bool waitForCurrentSubmission;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elapsedTime = 0;
        waitForCurrentSubmission = false;
    }

    public void ToggleLight()
    {
        if (PlaytestGameManager.instance.isTutorialWelded || PlaytestGameManager.instance.isMission1Welded)
        {
            subArea.submitWeldable();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Weldable") && !waitForCurrentSubmission && PlaytestGameManager.instance.state != PlaytestGameManager.GameState.Mission1)
        {
            waitForCurrentSubmission = true;
            //other.gameObject.transform.root.GetComponent<Grabbable>().enabled = false;
            StartCoroutine(WaitForSubmission());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Weldable"))
        {
            elapsedTime = 0;
        }
    }

    private IEnumerator WaitForSubmission() {
        yield return new WaitForSecondsRealtime(2f);
        SubmitAreaAnimator.SetBool("IsSubmitting", true);
        yield return new WaitForSecondsRealtime(3f);
        ToggleLight();
        yield return new WaitForSecondsRealtime(3f);
        SubmitAreaAnimator.SetBool("IsSubmitting", false);
        yield return new WaitForSecondsRealtime(5f);
        waitForCurrentSubmission = false;

    }

}
