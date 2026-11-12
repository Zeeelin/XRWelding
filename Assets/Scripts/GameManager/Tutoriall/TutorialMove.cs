using Unity.XR.CoreUtils;
using UnityEngine;

public class TutorialMove : MonoBehaviour
{
    private bool hasTutorialPlayed = false;
    private void OnTriggerExit(Collider other)
    {
        if (!hasTutorialPlayed && other.transform.root.CompareTag("Player"))
        {
            ControlTutorial.instance.StopMoveTutorial();
            hasTutorialPlayed = true;
        }
    }
}
