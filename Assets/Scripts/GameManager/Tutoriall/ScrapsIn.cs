using UnityEngine;

public class ScrapsIn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weldable")) {
            SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.PlayerStartWelding);
            Destroy(this.gameObject);
        }
    }
}
