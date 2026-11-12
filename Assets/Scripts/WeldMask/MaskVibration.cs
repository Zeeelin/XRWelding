using System.Collections;
using UnityEngine;

public class MaskVibration : MonoBehaviour
{
    public bool isVibrating = false;
    public float vibrationLength = 0.25f;
    void Start()
    {
    }

    void Update()
    {       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.CompareTag("Player") && !isVibrating)
        {
            isVibrating = true;
            StartCoroutine(VibrationStart());
        }

    }

    public IEnumerator VibrationStart() {
        OVRInput.SetControllerVibration(frequency: 1f, amplitude: 0.7f, OVRInput.Controller.RTouch);
        yield return new WaitForSecondsRealtime(vibrationLength);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        //yield return new WaitForSecondsRealtime(1f);
        isVibrating = false;
    }
}
