using Unity.VisualScripting;
using UnityEngine;

public class FuelCellMatSwipe : MonoBehaviour
{
    public FuelContainerPickUp fuelContainerPickUp;
    public Material repairedTexture;

    private MeshRenderer fuelTankMat;
    private bool hasSwapped = false;
    void Start()
    {
        fuelTankMat = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasSwapped && fuelContainerPickUp != null) {
            if (fuelContainerPickUp.isWeldDone) {
                fuelTankMat.material = repairedTexture;
                hasSwapped = true;
            }
        }    
    }
}
