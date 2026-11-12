using Oculus.Interaction;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [Header("Floating Settings")]
    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();
    private float floatTimer = 0f;

    [Header("Grab Interaction")]
    public Grabbable grabbable;

    public bool isActivate;
    public bool isInBooth;

    void Start()
    {
        posOffset = transform.position;
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
        isActivate = false;
    }

    private void OnDestroy()
    {

        grabbable.WhenPointerEventRaised -= HandlePointerEvent;

    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect)
        {
            posOffset = transform.position;
            floatTimer = 0f;
        }
        if (evt.Type == PointerEventType.Select)
        {
            isActivate = true;
        }
    }

    void Update()
    {
        if (grabbable.SelectingPointsCount > 0 || isInBooth)
        {
        }
        else if (isActivate & !isInBooth)
        {
            float noise = Mathf.PerlinNoise(Time.time, 0.0f);
            float noiseX = (Mathf.PerlinNoise(Time.time, 0.0f) - 0.5f) * 0.7f;
            float noiseY = (Mathf.PerlinNoise(0.0f, Time.time) - 0.5f) * 0.7f;
            float noiseZ = (Mathf.PerlinNoise(Time.time, Time.time) - 0.5f) * 0.7f;
            transform.Rotate(new Vector3(noiseX, noiseY, noiseZ) * degreesPerSecond * Time.deltaTime);

            tempPos = posOffset;
            tempPos.y += Mathf.Sin(floatTimer * Mathf.PI * frequency) * amplitude;
           

            transform.position = tempPos;
        }
    }
}
