using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class PointWeld : MonoBehaviour
{

    public Transform weldTip;
    public GameObject weldJointSpherePrefab;
    public GameObject assemblyRootPrefab;

    [Header("Grab Interaction")]
    public Grabbable grabbable;

    public float weldRadius = 0.5f;

    void Start()
    {

        grabbable.WhenPointerEventRaised += HandlePointerEvent;

    }

    private void OnDestroy()
    {

        grabbable.WhenPointerEventRaised -= HandlePointerEvent;

    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect)
        {
            // Placeholder for Unselect Pointer Event
        }
    }
    void Update()
    {
        if (grabbable.SelectingPointsCount > 0)
        {

            bool isTriggerPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                                   OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

            if (isTriggerPressed)
            {
                PointWeldAttempt();
                OVRInput.SetControllerVibration(0.05f, 0.2f, OVRInput.Controller.RTouch);
            }
        }
    }

    /// <summary>
    /// Test if the weld point coolides with both weldable parts
    /// </summary>
    public void PointWeldAttempt() {

        Collider[] hitColliders = Physics.OverlapSphere(weldTip.position, weldRadius);

        HashSet<GameObject> objectsToWeld = new HashSet<GameObject>();

        // Account for object that has already merged
        int objectCount = 0;
        foreach (var hit in hitColliders) {
            if (hit.CompareTag("Weldable")) {
                objectsToWeld.Add(hit.transform.root.gameObject);
                objectCount++;
            }
        }

        if (objectsToWeld.Count >= 2 || objectCount >= 2)
        {
            MergeObjects(new List<GameObject>(objectsToWeld), weldTip);
        }

    }

    /// <summary>
    /// Merge two or multiple parts together if intersect with the joint weld sphere
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="weldTipTrans"></param>
    public void MergeObjects(List<GameObject> parts, Transform weldTipTrans) {

        //Transform assemblyVisual = FindVisual(assemblyRoot.transform);
        GameObject assemblyRoot = Instantiate(assemblyRootPrefab, Vector3.zero, Quaternion.identity);
        Transform assemblyVisual = FindVisual(assemblyRoot.transform);
        
        GameObject weldJointSphere = Instantiate(weldJointSpherePrefab, weldTipTrans.position, Quaternion.identity);
        weldJointSphere.transform.SetParent(assemblyVisual, true);
        

        foreach (GameObject part in parts) {
            Transform partVisual = FindVisual(part.transform);
            MoveVisualChildren(partVisual, assemblyVisual);

            if (part != assemblyRoot)
            {
                Destroy(part);
            }

        }
    }

    /// <summary>
    /// Find the visual folder in grabbable object
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public Transform FindVisual(Transform root)
    {
        var t = root.Find("Visual");
        if (t == null) Debug.LogError($"No Visual child found under {root.name}");
        return t;
    }

    /// <summary>
    /// Move all visual (mesh & colliders) to the visual fold of the assembly object
    /// </summary>
    /// <param name="fromVisual"></param>
    /// <param name="toVisual"></param>
    void MoveVisualChildren(Transform fromVisual, Transform toVisual)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in fromVisual) children.Add(child);

        foreach (Transform child in children)
        {
            var rb = child.GetComponentInChildren<Rigidbody>();
            if (rb) Destroy(rb);

            child.SetParent(toVisual, true);
        }
    }



}
