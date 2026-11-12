using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using System.Collections;

public class WeldingConnection : MonoBehaviour
{
    public WeldingController controller;
    public Transform weldTip;
    public float weldRadius = 0.5f;
    public GameObject assemblyRootPrefab;
    public SubtitleData subtitleData;

    private bool hasWeld = false;

    private static readonly HashSet<GameObject> mergedRoots = new HashSet<GameObject>();

    void Start()
    {

    }

    void Update()
    {
        if (controller.isWelding && !hasWeld)
        {
            hasWeld = true;
            PointWeldAttempt();
        }
        else if (!controller.isWelding)
        {
            hasWeld = false;
        }
    }

    /// <summary>
    /// Test if the weld point collides with both weldable parts
    /// </summary>
    public void PointWeldAttempt()
    {
        Collider[] hitColliders = Physics.OverlapSphere(weldTip.position, weldRadius);

        HashSet<GameObject> objectsToWeld = new HashSet<GameObject>();
        int objectCount = 0;

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Weldable"))
            {
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
    public void MergeObjects(List<GameObject> parts, Transform weldTipTrans)
    {
        if (parts == null || parts.Count < 2) return;

        GameObject hostRoot = DetermineHostRoot(parts);
        Transform hostVisual = FindVisual(hostRoot.transform);

        // Fallback: If for some reason the chosen host doesn't have a Visual folder,
        // we use the original logic to spawn a new prefab.
        if (hostVisual == null)
        {
            Debug.LogWarning("No 'Visual' folder found on host root. Falling back to assembly prefab.");
            hostRoot = Instantiate(assemblyRootPrefab, Vector3.zero, Quaternion.identity);
            hostVisual = FindVisual(hostRoot.transform);
        }

        foreach (GameObject part in parts)
        {
            if (part == hostRoot) continue;

            mergedRoots.Add(part);

            Transform partVisual = FindVisual(part.transform);

            if (partVisual == null)
            {
                Debug.Log($"PointWeld: Skipping '{part.name}' — no Visual child found.");
                continue;
            }

            MoveVisualChildren(partVisual, hostVisual);

            // Destroy the old part root since its visual children are now in the hostRoot
            Destroy(part);
        }

        // Track the host root so re-entry triggers are ignored
        mergedRoots.Add(hostRoot);
    }

    /// <summary>
    /// Checks the list of parts and returns the best one to act as the primary root.
    /// </summary>
    private GameObject DetermineHostRoot(List<GameObject> parts)
    {
        // Priority 1: Keep the Fuel Container alive
        foreach (GameObject part in parts)
        {
            if (part.GetComponent<FuelContainerPickUp>() != null)
                return part;
        }

        // Priority 2: If no Fuel Container, keep the one that is Grabbable
        foreach (GameObject part in parts)
        {
            if (part.GetComponent<Grabbable>() != null)
                return part;
        }

        // Priority 3: If neither exist, just use the first object in the list
        return parts[0];
    }

    /// <summary>
    /// Find the visual folder in grabbable object
    /// </summary>
    public Transform FindVisual(Transform root)
    {
        return root.Find("Visual");
    }

    void MoveVisualChildren(Transform fromVisual, Transform toVisual)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in fromVisual) children.Add(child);

        if (PlaytestGameManager.instance.state == PlaytestGameManager.GameState.Tutorial)
        {
            PlaytestGameManager.instance.isTutorialWelded = true;
            StartCoroutine(WaitForPlayerToFinishWeld());
        }

        foreach (Transform child in children)
        {
            var rb = child.GetComponentInChildren<Rigidbody>();
            if (rb) Destroy(rb);

            child.SetParent(toVisual, true);
        }
    }

    public IEnumerator WaitForPlayerToFinishWeld() {
        yield return new WaitForSecondsRealtime(3f);
        SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.PlayerWeldTwoPiecesTogether);
        MissionManager.Instance.UpdateBriefing("Place the welded material in the submission receptacle, then step back and wait a few seconds.");
        SubtitlePlayer.Instance.StopAllSubtitles();
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitleData);
    }
}
