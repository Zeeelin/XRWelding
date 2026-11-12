using System.Collections.Generic;
using System.Collections;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Drives the marching-cubes weld system with SDF concept
/// 
/// Live preview:  MarchingCubes rebuilds the preview mesh whenever a sphere is added
///                OR any live sphere's radius crosses the growth threshold.
///
/// Static bake:   A sphere bakes to a permanent frozen mesh when it is evicted from
///                the active stack (rolling window of maxActiveStack), or when the
///                stroke ends (all remaining live spheres freeze together).
///
/// Sphere data is stored in WORLD SPACE and passed to MarchingCubes which handles
/// the world?local conversion internally.
/// </summary>
public class WeldingController : MonoBehaviour
{
    [Header("Welding Settings")]
    public Transform spawnPoint;
    public float growthRate = 0.5f;
    public float maxRadius = 0.05f;

    [Header("Dwell Time")]
    [Tooltip("Seconds the tip must hold in one grid cell before a sphere is created.")]
    public float dwellTimeRequired = 0.2f;

    [Header("Blob Blending")]
    [Tooltip("Smooth-min blend radius. Larger = softer blobs. Also used as boundsPadding ")]
    public float blobBlendDistance = 0.05f;

    [Header("Rebuild Threshold")]
    [Tooltip("Rebuild Thereshold, higher for better performance")]
    public float growthRebuildThreshold = 0.01f;

    [Header("Active Stack")]
    [Tooltip("Max number of spheres kept live at once")]
    public int maxActiveStack = 4;

    [Header("Mesh System")]
    [Tooltip("The MarchingCubes component that owns the live preview mesh and bakes statics.")]
    public MarchingCubes marchingCubes;

    [Header("Mask")]
    public Material normalMaterial;
    public Material darkenMaterial;
    public MeshRenderer MaskObject;
    public MeshRenderer HelmetObject;
    public GameObject autoDarkenLayer;
    public WeldMask weldMask;

    [Header("VFX")]
    public GameObject VFXSpark;

    public Grabbable grabbable;

    [Header("Grid System")]
    public Vector2 areaSize = new Vector2(2.0f, 2.0f);
    public Vector2Int gridResolution = new Vector2Int(5, 20);
    public Transform gridCenter;

    [Header("Evaluation")]
    public float eventDuration = 0f;
    public bool isEventRunning = false;
    public TextMeshProUGUI evaluationText;
    public GameObject evaluationPanel;
    public bool isTopColliding;
    public bool isBottomColliding;
    public float tooHighTime = 0f;
    public float tooLowTime = 0f;
    public float normalTime = 0f;
    public TextMeshProUGUI realtimeEval;

    public GameObject teleportObject;

    [Header("Safety")]
    public FlashVFX SaftyVFX;

    // Gamemanager Check Boollean
    public bool isWelding = false;

    // Sphere storage: xyz = world position, w = radius
    private Vector4[] sphereData = new Vector4[128];
    private int sphereCount = 0;

    // Tracks which radius each live sphere had the last time rebuilt the preview
    private float[] lastBuiltRadius = new float[128];

    private struct ActiveCell
    {
        public Vector2Int gridCoord;
        public int sphereIndex;
    }

    private List<ActiveCell> activeStack = new List<ActiveCell>();

    // Dwell
    private Vector2Int lastGridPos;
    private float dwellTimer = 0f;
    private bool saftyScore = true;

    private List<CombineInstance> currentStrokeChunks = new List<CombineInstance>();

    private Grabbable lockedWorkpiece;
    private bool hasTutorialPlayed;

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select && !hasTutorialPlayed)
        {

            ControlTutorial.instance.PlayTriggerTutorial();
        }
        if (evt.Type == PointerEventType.Unselect)
        {
            if (!hasTutorialPlayed) {
                ControlTutorial.instance.StopTriggerTutorial();
            }
            // Placeholder for Unselect Pointer Event
            EndStroke();
        }
    }
    void Start()
    {
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
        MaskObject.material = normalMaterial;
        HelmetObject.material = normalMaterial;
        autoDarkenLayer.SetActive(false);
        saftyScore = true;
        hasTutorialPlayed = false;

        // Initialise sphereData to something harmless (far-away, zero radius)
        for (int i = 0; i < sphereData.Length; i++)
            sphereData[i] = new Vector4(0f, -1000f, 0f, 0f);
    }

    private void OnDestroy()
    {
        grabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    void Update()
    {
        HandleInput();

        if (isWelding)
            ProcessWeldingFrame();

        if (isEventRunning)
        {
            eventDuration += Time.deltaTime;

            if (!isBottomColliding)
            {
                tooHighTime += Time.deltaTime;
                realtimeEval.text = "Too Close";
                SoundManager.Instance.StopRepeatingSFX(SoundManager.RepeatingSfxClip.WeldSuccess);
                SoundManager.Instance.PlayRepeatingSFX(SoundManager.RepeatingSfxClip.WeldFail);
            }
            else if (isTopColliding)
            {
                tooLowTime += Time.deltaTime;
                realtimeEval.text = "Too Far";
                SoundManager.Instance.StopRepeatingSFX(SoundManager.RepeatingSfxClip.WeldSuccess);
                SoundManager.Instance.PlayRepeatingSFX(SoundManager.RepeatingSfxClip.WeldFail);
            }
            else
            {
                normalTime += Time.deltaTime;
                realtimeEval.text = "Nice!";
                SoundManager.Instance.PlayRepeatingSFX(SoundManager.RepeatingSfxClip.WeldSuccess);
                SoundManager.Instance.StopRepeatingSFX(SoundManager.RepeatingSfxClip.WeldFail);
            }

            if (!weldMask.isMaskDown)
            {
                saftyScore = false;
                SaftyVFX.TriggerFlash(1f);
            }
        }
    }

    void HandleInput()
    {
        bool down = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)
                 || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        bool held = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)
                 || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        bool up = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger)
                 || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);

        bool toolHeld = grabbable.SelectingPointsCount > 0;

        if (toolHeld && PlaytestGameManager.instance.isPowerStart)
        {
            //teleportObject.SetActive(false);

            if (down)
            {
                StartStroke();
                VFXSpark.SetActive(true);
                MaskObject.material = darkenMaterial;
                HelmetObject.material = darkenMaterial;
                autoDarkenLayer.SetActive(true);
                isEventRunning = true;
                if (!hasTutorialPlayed)
                {
                    ControlTutorial.instance.StopTriggerTutorial();
                    hasTutorialPlayed = true;
                }
            }

            if (held)
                OVRInput.SetControllerVibration(1f, 0.5f, OVRInput.Controller.RTouch);

            if (up)
                TriggerRelease();
        }
        else
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
            if (isWelding) TriggerRelease();
        }
    }

    private void TriggerRelease()
    {
        
        SoundManager.Instance.StopRepeatingSFX(SoundManager.RepeatingSfxClip.WeldSuccess);
        SoundManager.Instance.StopRepeatingSFX(SoundManager.RepeatingSfxClip.WeldFail);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        MaskObject.material = normalMaterial;
        HelmetObject.material = normalMaterial;
        autoDarkenLayer.SetActive(false);
        VFXSpark.SetActive(false);

        if (lockedWorkpiece != null)
        {
            lockedWorkpiece.enabled = true;
            lockedWorkpiece = null;
        }

        isEventRunning = false;
        EndStroke();
    }

    //  STROKE MANAGEMENT

    void StartStroke()
    {
        isWelding = true;
        activeStack.Clear();
        currentStrokeChunks.Clear(); // Ensure combined list is empty
        ResetDwell();

        float searchRadius = 0.1f;
        Collider[] hitColliders = Physics.OverlapSphere(spawnPoint.position, searchRadius);

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Weldable"))
            {
                // Get the Grabbable component from the object or its parent
                Grabbable targetGrabbable = hit.GetComponentInParent<Grabbable>();
                if (targetGrabbable != null)
                {
                    lockedWorkpiece = targetGrabbable;
                    // Force the other hand to drop it if they are already holding it
                    lockedWorkpiece.enabled = false;
                    Debug.Log($"Locking {lockedWorkpiece.name} to prevent movement during weld.");
                    break;
                }
            }
        }
    }

    void EndStroke()
    {
        isWelding = false;

        // Bake the remaining live tip of the weld
        if (activeStack.Count > 0)
        {
            BakeGroup(activeStack);
            activeStack.Clear();
        }

        marchingCubes.ClearLivePreview();
        ResetDwell();

        // Combine all the baked chunks into ONE final GameObject
        if (currentStrokeChunks.Count > 0)
        {
            Mesh finalStrokeMesh = new Mesh();
            finalStrokeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            // Combine all chunks into one single mesh structure
            finalStrokeMesh.CombineMeshes(currentStrokeChunks.ToArray(), true, false);

            // Create exactly ONE GameObject for the entire stroke
            GameObject strokeObj = new GameObject("WeldStroke_" + Time.frameCount);

            strokeObj.transform.SetParent(marchingCubes.transform, false);
            strokeObj.transform.localPosition = Vector3.zero;
            strokeObj.transform.localRotation = Quaternion.identity;
            strokeObj.transform.localScale = Vector3.one;
            strokeObj.tag = "Weldable";

            bool foundValidWorkpiece = false;
            Transform targetParent = null;
            float searchRadius = 0.1f;
            Collider[] hitColliders = Physics.OverlapSphere(spawnPoint.position, searchRadius);

            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Weldable"))
                {
                    Transform rootTransform = hit.transform.root;
                    Transform visualTransform = rootTransform.Find("Visual");

                    // Parent to the "Visual" child if it exists, otherwise the root
                    targetParent = visualTransform != null ? visualTransform : rootTransform;
                    foundValidWorkpiece = true;
                    break;
                }
            }

            var mf = strokeObj.AddComponent<MeshFilter>();
            var mr = strokeObj.AddComponent<MeshRenderer>();
            mf.sharedMesh = finalStrokeMesh;
            mr.sharedMaterial = marchingCubes.weldMaterial;

            // 2. Decide: Stick or Fade?
            if (foundValidWorkpiece)
            {
                strokeObj.transform.SetParent(targetParent, true);
                strokeObj.tag = "Weldable";
                var mc = strokeObj.AddComponent<MeshCollider>();
                mc.sharedMesh = finalStrokeMesh;
                Debug.Log("[WeldingController] Weld stuck to workpiece.");
            }
            else
            {
                Debug.Log("[WeldingController] No workpiece found. Fading out weld.");
                StartCoroutine(FadeAndDestroy(strokeObj, mr));
            }

            currentStrokeChunks.Clear();
        }

        // This ensures the next stroke starts completely fresh and doesn't waste
        // CPU trying to blend with this finished, frozen stroke.
        sphereCount = 0;

        //teleportObject.SetActive(true);
    }

    private IEnumerator FadeAndDestroy(GameObject obj, MeshRenderer renderer)
    {
        float duration = 1.5f;
        float elapsed = 0f;

        Material mat = renderer.material;

        // Customed alpha change for URP
        mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color startColor = mat.GetColor("_BaseColor");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            mat.SetColor("_BaseColor", new Color(startColor.r, startColor.g, startColor.b, alpha));

            yield return null;
        }

        Destroy(obj);
    }

    void ResetDwell()
    {
        dwellTimer = 0f;
        lastGridPos = new Vector2Int(int.MinValue, int.MinValue);
    }

    void ProcessWeldingFrame()
    {
        if (sphereCount >= 128) { 
            EndStroke();
            return;
        }

        Vector2Int currentCell = WorldToGrid(spawnPoint.position);

        // Dwell
        if (currentCell == lastGridPos)
            dwellTimer += Time.deltaTime;
        else
        {
            lastGridPos = currentCell;
            dwellTimer = 0f;
        }

        if (dwellTimer < dwellTimeRequired) return;

        // Find existing sphere for this cell
        int foundIndex = -1;
        int stackPos = -1;
        for (int i = 0; i < activeStack.Count; i++)
        {
            if (activeStack[i].gridCoord == currentCell)
            {
                foundIndex = activeStack[i].sphereIndex;
                stackPos = i;
                break;
            }
        }

        if (foundIndex != -1)
        {
            // Grow existing sphere and check threshold
            bool rebuilt = GrowSphere(foundIndex);

            // Promote to top of stack (most-recently-used)
            var cell = activeStack[stackPos];
            activeStack.RemoveAt(stackPos);
            activeStack.Add(cell);

            if (rebuilt)
                PushLiveMeshRebuild();
        }
        else
        {
            CreateNewSphere(currentCell);
        }
    }

    // Sphere helpers

    /// <summary>
    /// Grows a live sphere. Returns true if it crossed the rebuild threshold.
    /// </summary>
    bool GrowSphere(int index)
    {
        float prev = sphereData[index].w;
        float next = Mathf.Clamp(prev + growthRate * Time.deltaTime, 0f, maxRadius);
        sphereData[index] = new Vector4(
            sphereData[index].x, sphereData[index].y, sphereData[index].z, next);

        // Check if we've grown past the threshold since the last rebuild
        if (next - lastBuiltRadius[index] >= growthRebuildThreshold ||
            Mathf.Approximately(next, maxRadius))
        {
            lastBuiltRadius[index] = next;
            return true;
        }
        return false;
    }

    void CreateNewSphere(Vector2Int cell)
    {
        // Assign world-space position
        sphereData[sphereCount] = new Vector4(
            spawnPoint.position.x,
            spawnPoint.position.y,
            spawnPoint.position.z,
            0.001f);

        lastBuiltRadius[sphereCount] = 0.01f;

        activeStack.Add(new ActiveCell { gridCoord = cell, sphereIndex = sphereCount });
        sphereCount++;

        // Evict oldest if stack is full
        if (activeStack.Count > maxActiveStack)
        {
            var evicted = activeStack[0];
            activeStack.RemoveAt(0);
            BakeSingle(evicted.sphereIndex);
        }
        marchingCubes.GetComponent<EmissionController>().TriggerGlow();

        // New sphere always triggers a rebuild
        PushLiveMeshRebuild();
    }

    /// <summary>
    /// Sends the current live sphere set to MarchingCubes for a preview rebuild.
    /// Only live (active-stack) spheres are included ?baked ones are already frozen.
    /// </summary>
    void PushLiveMeshRebuild()
    {
        if (marchingCubes == null) return;

        int count = activeStack.Count;
        if (count == 0)
        {
            marchingCubes.ClearLivePreview();
            return;
        }

        // The target box contains ONLY the active stack
        Vector4[] liveTargets = new Vector4[count];
        for (int i = 0; i < count; i++)
            liveTargets[i] = sphereData[activeStack[i].sphereIndex];

        // pass all sphere for this stroke
        marchingCubes.MarkDirty(liveTargets, count, sphereData, sphereCount, blobBlendDistance);
    }

    void BakeSingle(int sphereIndex)
    {
        if (marchingCubes == null) return;

        Mesh chunkMesh = marchingCubes.BakeStaticMesh(
            new Vector4[] { sphereData[sphereIndex] }, 1,
            sphereData, sphereCount,
            blobBlendDistance);

        // Instead of a GameObject, save the mesh into our combine list
        if (chunkMesh != null)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = chunkMesh;
            ci.transform = Matrix4x4.identity;
            currentStrokeChunks.Add(ci);
        }
    }

    void BakeGroup(List<ActiveCell> cells)
    {
        if (marchingCubes == null || cells.Count == 0) return;

        Vector4[] batchTargets = new Vector4[cells.Count];
        for (int i = 0; i < cells.Count; i++)
            batchTargets[i] = sphereData[cells[i].sphereIndex];

        Mesh chunkMesh = marchingCubes.BakeStaticMesh(
            batchTargets, cells.Count,
            sphereData, sphereCount,
            blobBlendDistance);

        if (chunkMesh != null)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = chunkMesh;
            ci.transform = Matrix4x4.identity;
            currentStrokeChunks.Add(ci);
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 origin = gridCenter != null ? gridCenter.position : transform.position;
        float relX = worldPos.x - (origin.x - areaSize.x * 0.5f);
        float relZ = worldPos.z - (origin.z - areaSize.y * 0.5f);
        int x = Mathf.FloorToInt((relX / areaSize.x) * gridResolution.x);
        int z = Mathf.FloorToInt((relZ / areaSize.y) * gridResolution.y);
        return new Vector2Int(x, z);
    }


    public void ClearAllWelds()
    {
        sphereCount = 0;
        activeStack.Clear();
        ResetDwell();
        eventDuration = 0f;
        tooHighTime = 0f;
        tooLowTime = 0f;
        normalTime = 0f;
        saftyScore = true;

        if (evaluationText != null) evaluationText.text = "";

        for (int i = 0; i < sphereData.Length; i++)
            sphereData[i] = new Vector4(0f, -1000f, 0f, 0f);

        if (marchingCubes != null)
            marchingCubes.ClearLivePreview();

        // Destroy all baked child meshes under the MarchingCubes object
        if (marchingCubes != null)
        {
            Transform mcT = marchingCubes.transform;
            for (int i = mcT.childCount - 1; i >= 0; i--)
                Destroy(mcT.GetChild(i).gameObject);
        }

        Debug.Log("[WeldingController] All welds cleared.");
    }

    public void DisplayEvaluation()
    {
        if (evaluationPanel != null) evaluationPanel.SetActive(true);

        string msg = eventDuration < 10f
            ? $"Total time: {eventDuration:F1}s\nTry to weld slower."
            : eventDuration < 40f
                ? $"Total time: {eventDuration:F1}s\nGreat job!"
                : $"Total time: {eventDuration:F1}s\nTry to weld faster.";

        msg += $"\n\nPerfect: {normalTime:F1}s | Too high: {tooHighTime:F1}s | Too low: {tooLowTime:F1}s";
        msg += saftyScore
            ? "\n Safety protocol followed."
            : "\n Mask was not worn the entire time. Be careful next time.";

        if (evaluationText != null) evaluationText.text = msg;
    }
}

public class SphereDebug : MonoBehaviour
{
    public Transform spawnPoint;
    public float searchRadius = 0.05f;

    private void OnDrawGizmos()
    {
        if (spawnPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint.position, searchRadius);
    }
}
