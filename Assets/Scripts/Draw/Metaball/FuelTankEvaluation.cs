using UnityEngine;

public class FuelTankEvaluation : MonoBehaviour
{
    public bool isWelded;
    private bool hasDetected;
    private void Start()
    {
        isWelded = false;
        hasDetected = false;
    }
    void OnTriggerStay(Collider other)
    {
        if ( !hasDetected && other.name.StartsWith("WeldStroke_"))
        {
            isWelded = true;
            hasDetected = true;
        }
    }
}
