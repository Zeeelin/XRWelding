using UnityEngine;
using System.Collections;
using Oculus.Interaction;

public class FuelCellSubmit : MonoBehaviour
{
    public FuelContainerPickUp fuelContainerPickUp;
    public Animator fuelCellAnimator;
    public Transform fuelTankTransform;
    public GameObject RepairedFuelCellVisual;

    private bool hasSubmit;
    void Start()
    {
        hasSubmit = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
    }
    private void OnTriggerStay(Collider other)
    {
        if (hasSubmit) return;

        FuelContainerPickUp collidedContainer = other.GetComponentInParent<FuelContainerPickUp>();
        if (collidedContainer != null)
        {
            if (collidedContainer.isWeldDone && collidedContainer.inAreaToSubmit)
            {
                hasSubmit = true;
                StartCoroutine(WaitForM1Submission(other));
            }
            else if (collidedContainer.inAreaToSubmit)
            {
                other.transform.root.position = fuelTankTransform.position;
                other.transform.root.rotation = fuelTankTransform.rotation;
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
    }

    private IEnumerator WaitForM1Submission(Collider other)
    {
        yield return new WaitForSecondsRealtime(2f);
        other.transform.root.gameObject.SetActive(false);
        RepairedFuelCellVisual.SetActive(true);
        PlaytestGameManager.instance.IsFuelCellAvailable = false;
        MissionManager.Instance.UpdateBriefing("Use the materials in the sculpture room to create whatever you’d like!");
        fuelCellAnimator.SetBool("IsClose", false);
        PlaytestGameManager.instance.startFreeBuild();
    }
}
