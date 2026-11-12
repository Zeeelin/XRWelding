using UnityEngine;
using Oculus.Interaction;

public class DrawingManager : MonoBehaviour
{

    [Header("TorchProperties")]
    public Transform tip;
    public Material drawingMaterial;
    public Material tipMaterial;
    [Range(0.01f, 0.1f)]
    public float torchWidth = 0.01f;
    public Color[] penColors;

    [Header("Grab Interaction")]
    public Grabbable grabbable;

    private LineRenderer currentDrawing;
    private int index;
    private int currentColorIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        currentColorIndex = 0;
        tipMaterial.color = penColors[currentColorIndex];

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
            if (currentDrawing != null)
            {
                currentDrawing = null;
            }
        }
    }

    void Update()
    {
        // Check if grabbed and trigger is pressed
        if (grabbable.SelectingPointsCount > 0)
        {
            
            bool isTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ||
                                   OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);

            if (isTriggerPressed)
            {
                Draw();
            }
            else if (currentDrawing != null)
            {
                currentDrawing = null;
            }
        }

        // Color switching
        /*if (OVRInput.GetDown(OVRInput.Button.One))
        {
            SwitchColor();
        }*/
    }

    private void Draw()
    {
        if (currentDrawing == null)
        {
            index = 0;
            currentDrawing = new GameObject("DrawnLine").AddComponent<LineRenderer>();
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = penColors[currentColorIndex];
            currentDrawing.startWidth = currentDrawing.endWidth = torchWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.SetPosition(0, tip.position);
        }
        else
        {
            var currentPos = currentDrawing.GetPosition(index);
            if (Vector3.Distance(currentPos, tip.position) > 0.01f)
            {
                index++;
                currentDrawing.positionCount = index + 1;
                currentDrawing.SetPosition(index, tip.position);
            }
        }
    }

    private void SwitchColor()
    {
        if (currentColorIndex == penColors.Length - 1)
        {
            currentColorIndex = 0;
        }
        else
        {
            currentColorIndex++;
        }
        tipMaterial.color = penColors[currentColorIndex];
    }
}