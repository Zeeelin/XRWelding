using UnityEngine;

public class TutorialGrab : MonoBehaviour
{
    private bool hasTutorialPlayed = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!hasTutorialPlayed && other.transform.root.CompareTag("Player"))
        {
            ControlTutorial.instance.PlayGrabTutorial();
            hasTutorialPlayed = true;
        }
    }
}
