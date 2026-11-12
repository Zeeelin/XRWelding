using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SubmissionArea : MonoBehaviour
{
    public GameObject[] ChangedLight;
    public CameraShake cameraShake;

    public SubtitleData subtitleData;

    [Header("Dissolve Shader")]
    public Material dissolveMat;
    public string shaderPropertyName = "_CutOffHeight";
    private Material clonedMaterial;

    private bool AllowSubmission;

    private void Start()
    {
        AllowSubmission = false;
        foreach (var i in ChangedLight)
        {
            i.SetActive(false);
        }
    }
    private void Update()
    {
        
    }

    public void submitWeldable()
    {
        BoxCollider submitionCollider = GetComponent<BoxCollider>();
        Vector3 worldCenter = transform.TransformPoint(submitionCollider.center);

        Vector3 worldHalfExtents = Vector3.Scale(submitionCollider.size, transform.lossyScale) * 0.5f;

        Collider[] hits = Physics.OverlapBox(worldCenter, worldHalfExtents, transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit == submitionCollider) continue;

            if (hit.transform.root.CompareTag("Weldable") )
            {
                AllowSubmission = true;

                StartCoroutine(WaitForObjectToDissolve(hit.transform.root.gameObject));
            }
        }

        if (AllowSubmission)
        {
            if (PlaytestGameManager.instance.isTutorialWelded && PlaytestGameManager.instance.state == PlaytestGameManager.GameState.Tutorial)
            {
                StartCoroutine(WaitToStartMission1());
                PlaytestGameManager.instance.isTutorialWelded = false;
            }
            AllowSubmission = false;
        }

    }

    private IEnumerator WaitForObjectToDissolve(GameObject submitItem)
    {
        float duration = 5f;
        float elapsedTime = 0f;

        float startValue = 1.5f;
        float endValue = -0.5f;

        MeshRenderer[] allRenderers = submitItem.GetComponentsInChildren<MeshRenderer>(true);
        int propertyID = Shader.PropertyToID(shaderPropertyName);
        Material[] instancedMats = new Material[allRenderers.Length];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            instancedMats[i] = new Material(dissolveMat);
            allRenderers[i].material = instancedMats[i];
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);

            foreach (Material mat in instancedMats)
            {
                mat.SetFloat(propertyID, currentValue);
            }

            yield return null;
        }
        yield return new WaitForSecondsRealtime(2f);
        Destroy(submitItem.gameObject);
    }

    private IEnumerator WaitToStartMission1()
    {
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.PlayerTurnsInSuccessfully);
        MissionManager.Instance.UpdateBriefing("Please wait for further instructions.");
        SubtitlePlayer.Instance.StopAllSubtitles();
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitleData);
        // Play Audio
        yield return new WaitForSeconds(15f);
        cameraShake.shake = true;
        yield return new WaitForSeconds(1f);
        foreach (var i in ChangedLight) { 
            i.SetActive(true);
        }
        
        PlaytestGameManager.instance.startMission1();
        MissionManager.Instance.UpdateBriefing("Retrieve the damaged fuel cell from its casing by the airlock.\r\n");
        yield return new WaitForSeconds(1f);
        cameraShake.shake = false;
    }
}
