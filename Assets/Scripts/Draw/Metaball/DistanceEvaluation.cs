using UnityEngine;

public class DistanceEvaluation : MonoBehaviour
{
    public WeldingController controller;
    public bool isTopDistance;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (controller.isEventRunning && other.transform.root.CompareTag("Weldable")) {
            if (isTopDistance) {
                controller.isTopColliding = true;
            }
            else
            {
                controller.isBottomColliding = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (controller.isEventRunning && other.transform.root.CompareTag("Weldable"))
        {
            if (isTopDistance)
            {
                controller.isTopColliding = false;
            }
            else {
                controller.isBottomColliding = false;
            }
        }
    }
}
