using Oculus.Interaction;
using UnityEngine;

public class WelderPowerSwitch : MonoBehaviour
{
    public Transform powerPositionOff;
    public Transform powerPositionOn;
    public float speed;
    public float timeCount = 0.0f;

    [Header("Indication Light")]
    public bool isPowerOff = true;
    public Material powerOffMat;
    public Material powerOnMat;
    public GameObject powerIndicator;

    public Grabbable grabbable;

    private bool isSnaping;
    private Transform targetMaskRotation;


    void Start()
    {

        grabbable.WhenPointerEventRaised += HandlePointerEvent;
        isSnaping = false;
        targetMaskRotation = powerPositionOff;
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

    public Transform SnapPointTransform()
    {
        if (this.transform.eulerAngles.z - targetMaskRotation.eulerAngles.z < 0)
        {
            return powerPositionOff;
        }
        else
        {
            return powerPositionOn;
        }
    }

    private void Update()
    {
        if (targetMaskRotation == powerPositionOff)
        {
            isPowerOff = true;
            powerIndicator.GetComponent<Renderer>().material = powerOffMat;
            PlaytestGameManager.instance.isPowerStart = false;
            SoundManager.Instance.PlaySFX(SoundManager.SfxClip.SwitchOff);
        }
        else
        {
            isPowerOff = false;
            powerIndicator.GetComponent<Renderer>().material = powerOnMat;
            PlaytestGameManager.instance.isPowerStart = true;
            SoundManager.Instance.PlaySFX(SoundManager.SfxClip.SwitchOn);
        }

        if (isSnaping)
        {
            //this.transform.position = Vector3.Lerp(this.transform.position, SnapPointTransform().position, timeCount * speed);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetMaskRotation.rotation, timeCount * speed);
            timeCount = timeCount + Time.deltaTime;
        }

        if (timeCount > 10f)
        {
            isSnaping = false;
            timeCount = 0;
        }

    }
}
