using System.Collections;
using Oculus.Interaction;
using UnityEngine;

public class DoorOpenClose : MonoBehaviour
{

    public Animator doorAnimator;
    public bool isLockedFirst;
    public GameObject doNotCrossSign;

    private bool allowPlaySound = true;

    public void Update()
    {
        if (isLockedFirst) {
            if (PlaytestGameManager.instance.state == PlaytestGameManager.GameState.FreeBuild) {
                doNotCrossSign.SetActive(false);
                isLockedFirst = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLockedFirst && other.transform.root.CompareTag("Player"))
        {
            doorAnimator.SetBool("IsClose", true);
            if (allowPlaySound)
            {
                allowPlaySound = false;
                SoundManager.Instance.PlaySFX(SoundManager.SfxClip.DoorOpen);
                StartCoroutine(WaitForSoundToPlay());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLockedFirst && other.transform.root.CompareTag("Player"))
        {
            doorAnimator.SetBool("IsClose", false);
        }
    }
    private IEnumerator WaitForSoundToPlay() {
        yield return new WaitForSecondsRealtime(3f);
        allowPlaySound = true;
    }

}
