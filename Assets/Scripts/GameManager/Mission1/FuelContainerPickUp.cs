using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class FuelContainerPickUp : MonoBehaviour
{

    public Grabbable grabbable;

    public bool isWeldDone;
    public bool inAreaToSubmit;

    public FuelTankEvaluation[] evaluationList;

    public bool hasPickUp;
    public Material glowMat;
    public bool isOneWelded, isTwoWelded, isThreeWelded;

    public SubtitleData[] subtitles;

    private bool startMission1 = false;

    private bool hasPlayed = false;

    void Start()
    {
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
        isWeldDone = false;
        inAreaToSubmit = false;
        hasPickUp = false;
        isOneWelded = false;
        isTwoWelded = false;
        isThreeWelded = false;
    }

    private void Update()
    {
        if (!startMission1 && PlaytestGameManager.instance.state == PlaytestGameManager.GameState.Mission1) {
            startMission1 = true;
            StartCoroutine(WaitIfPlayerNeedHint());
        }
        bool cracksWelded = true;
        float count = 0;
        if (!isWeldDone) {
            foreach (var cracks in evaluationList) {
                if (!cracks.isWelded)
                {
                    cracksWelded = false;
                }
                else { count++; }
            }

            if (cracksWelded && !isThreeWelded) {
                SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.ThirdCrack);
                MissionManager.Instance.UpdateBriefing("Return the fuel cell to its casing.");
                SubtitlePlayer.Instance.StopAllSubtitles();
                SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[2]);
                isWeldDone =true;
                isThreeWelded=true;
            }
            if (count == 1 && !isOneWelded)
            {
                SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.FirstCrack);
                SubtitlePlayer.Instance.StopAllSubtitles();
                SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[0]);
                isOneWelded = true;
            }
            else if (count == 2 && isOneWelded && !isTwoWelded)
            {
                SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.SecondCrack);
                SubtitlePlayer.Instance.StopAllSubtitles();
                SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[1]);
                isTwoWelded = true;
            }
        }
    }
    private void OnDestroy()
    {

        grabbable.WhenPointerEventRaised -= HandlePointerEvent;

    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select && !hasPlayed)
        {
            SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.FuelTankPickUp);
            MissionManager.Instance.UpdateBriefing("Carefully bring the fuel cell to the welding booth and weld over any cracks. ");
            SubtitlePlayer.Instance.StopAllSubtitles();
            SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[3]);
            hasPlayed = true;

        }
        if (evt.Type == PointerEventType.Unselect)
        {
            inAreaToSubmit = true;

        }
        else if (evt.Type == PointerEventType.Select){
            if (!hasPickUp)
            {
                MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>(true);
                int modifiedCount = 0;

                foreach (MeshRenderer renderer in allRenderers)
                {
                    if (renderer.sharedMaterials.Length > 1)
                    {
                        Material firstMaterial = renderer.sharedMaterials[0];
                        Material[] singleMaterialArray = new Material[] { firstMaterial };
                        renderer.sharedMaterials = singleMaterialArray;
                        modifiedCount++;
                    }
                }
            }

            hasPickUp =true;

            inAreaToSubmit = true;
        }
    }
    private IEnumerator WaitIfPlayerNeedHint() {
        yield return new WaitForSecondsRealtime(10f);
        HintMaterial();
    }
    public void HintMaterial() {
        if (!hasPickUp)
        {
            MeshRenderer[] allRenderers = this.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allRenderers)
            {
                List<Material> currentMaterials = new List<Material>(renderer.sharedMaterials);
                currentMaterials.Add(glowMat);
                renderer.sharedMaterials = currentMaterials.ToArray();
            }
        }
    }

}
