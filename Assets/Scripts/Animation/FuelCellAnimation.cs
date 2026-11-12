using UnityEngine;

public class FuelCellAnimation : MonoBehaviour
{
    public Animator fuelCellAnimator;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && PlaytestGameManager.instance.IsFuelCellAvailable) 
        {
            fuelCellAnimator.SetBool("IsClose", true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && PlaytestGameManager.instance.IsFuelCellAvailable)
        {
            if (!fuelCellAnimator.GetBool("IsClose")) {
                fuelCellAnimator.SetBool("IsClose", true);
            }
            
        }
    }

    private void OnTriggerExit(Collider other )
    {
        if (other.transform.root.CompareTag("Player") && PlaytestGameManager.instance.IsFuelCellAvailable)
        {
            fuelCellAnimator.SetBool("IsClose", false);
        }
    }
}
