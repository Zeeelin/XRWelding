using UnityEngine;

public class BoothZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<RotatingObject>() != null) {
            other.GetComponentInParent<RotatingObject>().isInBooth = true;
        }
        else if (other.CompareTag("Player")) { 
            SoundManager.Instance.StopBackground();
            SoundManager.Instance.PlayAmbient();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<RotatingObject>() != null)
        {
            other.GetComponentInParent<RotatingObject>().isInBooth = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<RotatingObject>() != null)
        {
            other.GetComponentInParent<RotatingObject>().isInBooth = false;
        }
        else if (other.CompareTag("Player") && PlaytestGameManager.instance.state != PlaytestGameManager.GameState.Mission1)
        {
            SoundManager.Instance.PlayBackground();
            SoundManager.Instance.StopAmbient();
        }
    }
}
