using Oculus.Interaction;
using UnityEngine;

public class PinTool : MonoBehaviour
{
    public Transform rayOrigin; 
    public float weldRadius = 0.05f;
    public float weldSpeed = 0.5f; 
    public float maxBeadHeight = 0.2f;

    public Grabbable grabbable;

    public LayerMask weldLayer;

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
            // Placeholder for Unselect Pointer Event
        }
    }

    void Update()
    {
        if (grabbable.SelectingPointsCount > 0)
        {
            bool isTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ||
                                   OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
            if (isTriggerPressed)
            {
                PerformWeld();
            }
        }
    }

    void PerformWeld()
    {

        RaycastHit hit;
        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * 10f, Color.red);
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, 10f, weldLayer))
        {
            SurfaceDrawing surface = hit.collider.GetComponent<SurfaceDrawing>();

            if (surface != null)
            {
                surface.DeformMesh(hit.point, weldRadius, weldSpeed, maxBeadHeight);

            }
        }
    }
}
