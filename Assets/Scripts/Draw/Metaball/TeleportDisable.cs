using Oculus.Interaction;
using UnityEngine;

public class TeleportDisable : MonoBehaviour
{

    [Header("Grab Interaction")]
    public Grabbable grabbable;
    public GameObject teleportObject;

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
        if (evt.Type == PointerEventType.Select)
        {
            teleportObject.SetActive(false);
        }
        if (evt.Type == PointerEventType.Select)
        {
            teleportObject.SetActive(true);
        }
    }
}
