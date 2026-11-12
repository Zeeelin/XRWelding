using Oculus.Interaction;
using UnityEngine;

public class TutorialMaskPickUp : MonoBehaviour
{
    public GameObject cameraMask;

    public Grabbable grabbable;

    public GameObject curtain;
    void Start()
    {
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void OnDestroy()
    {

        grabbable.WhenPointerEventRaised -= HandlePointerEvent;

    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect)
        {
            SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.MaskPickupVO);
            MissionManager.Instance.UpdateBriefing("Grab two pieces of metal, bring them to the booth, turn on power and weld them together.");
            cameraMask.SetActive(true);
            curtain.SetActive(false);
            Destroy(this.gameObject);

        }
        if (evt.Type == PointerEventType.Select)
        {
            if (!PlaytestGameManager.instance.hasPickUpMask)
            {
                ControlTutorial.instance.StopGrabTutorial();
                MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>(true);
                int modifiedCount = 0;

                foreach (MeshRenderer renderer in allRenderers)
                {
                    if (renderer.sharedMaterials.Length > 1)
                    {
                        Material firstMaterial = renderer.sharedMaterials[0];
                        Material[] singleMaterialArray = new Material[] { firstMaterial };
                        renderer.sharedMaterials = singleMaterialArray;
                        modifiedCount++;
                    }
                }
            }
            PlaytestGameManager.instance.hasPickUpMask = true;
        }
    }
}
