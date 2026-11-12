using Oculus.Interaction;
using UnityEngine;

public class HandPoseController : MonoBehaviour
{

    [Header("Hand State")]
    public GameObject RnormalHand;
    public GameObject RgripHand;
    public GameObject LnormalHand;
    public GameObject LgripHand;


    public bool isRightHand;

    public OVRInput.Controller controller = OVRInput.Controller.Touch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // Index trigger
        bool RtriggerValue = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
        bool LtriggerValue = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        if (isRightHand) {
            if (RtriggerValue) {
                RnormalHand.SetActive(false);
                RgripHand.SetActive(true);
            }
            else
            {
                RnormalHand.SetActive(true);
                RgripHand.SetActive(false);
            }
        }
        if (LtriggerValue)
        {
            LnormalHand.SetActive(false);
            LgripHand.SetActive(true);
        }
        else
        {
            LnormalHand.SetActive(true);
            LgripHand.SetActive(false);
        }
    }
}
