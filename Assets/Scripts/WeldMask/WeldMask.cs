using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System;


public class WeldMask : MonoBehaviour
{
    public Transform maskPositionUp;
    public Transform maskPositionDown;
    public float speed;
    public float timeCount = 0.0f;

    [Header("Safty Score")]
    public bool isMaskDown = false;

    public Grabbable grabbable;

    private bool isSnaping;
    private Transform targetMaskRotation;

    void Start()
    {

        grabbable.WhenPointerEventRaised += HandlePointerEvent;
        isSnaping = false;
        targetMaskRotation = maskPositionUp;
        this.transform.rotation = targetMaskRotation.rotation;
    }

    private void OnDestroy()
    {

        grabbable.WhenPointerEventRaised -= HandlePointerEvent;

    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect)
        {
            targetMaskRotation = SnapPointTransform();
            isSnaping = true;

        }
    }

    public Transform SnapPointTransform() {
        /*
        float distUp = Math.Abs(this.gameObject.transform.position.y - maskPositionUp.position.y);
        float distDown = Math.Abs(this.gameObject.transform.position.y - maskPositionDown.position.y);
        if (distUp < distDown)
        {
            return maskPositionUp;
        }
        else {
            return maskPositionDown;
        }
        */
        Debug.Log(this.transform.eulerAngles.x);
        Debug.Log(this.gameObject.name);
        if (this.transform.eulerAngles.x - targetMaskRotation.eulerAngles.x < 0) {
            return maskPositionUp;
        }
        else
        {
            return maskPositionDown;
        }
    }

    private void Update()
    {
        if (targetMaskRotation == maskPositionDown)
        {
            isMaskDown = true;
        }
        else {
            isMaskDown = false;
        }

        if (isSnaping)
        {
            //this.transform.position = Vector3.Lerp(this.transform.position, SnapPointTransform().position, timeCount * speed);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetMaskRotation.rotation, timeCount * speed);
            timeCount = timeCount + Time.deltaTime;
        }

        if (timeCount > 10f) {
            isSnaping = false;
            timeCount = 0;
        }
        
    }


}
