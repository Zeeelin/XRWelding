using Oculus.Interaction;
using UnityEngine;

public class PerparationVOTrigger : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HandInteraction"))
        {
            PlaytestGameManager.instance.inMainRoom = true;
            SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.PerparationVO);
            MissionManager.Instance.UpdateBriefing("Retrieve and put on the welding mask.");
            Destroy(this.gameObject);
        }
    }
}
