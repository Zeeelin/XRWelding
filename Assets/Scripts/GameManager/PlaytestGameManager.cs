using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaytestGameManager : MonoBehaviour
{

    public static PlaytestGameManager instance;

    public GameState state;

    [Header("UI")]
    public GameObject TVScreen;
    public Sprite M1Screen, FreeBuildScreen;

    [Header("Tutorial")]
    public bool hasPickUpMask;
    public GameObject weldMask;
    public bool inMainRoom;


    [Header("Mission 1")]
    public GameObject[] ChangedLight;
    public GameObject BrokenFuelCellVisual;
    public GameObject FuelCellWeldable;
    public Animator mainCOunterAnimation;
    public bool IsFuelCellAvailable;
    public Material sirenNormal;
    public Material sirenWarning;
    public GameObject sirenLight;

    [Header("Mission 2")]
    public GameObject endCollider;

    [Header("UI")]
    public SubtitleData[] subtitles;
    public SubtitleData newsubtitle;
    public bool isStart;
    public bool isPowerStart;
    public bool isTutorialWelded;
    public bool isMission1Welded;

    public Material glowMat;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        state = GameState.Tutorial;
        isPowerStart = false;
        hasPickUpMask = false;
        inMainRoom = false;
        endCollider.SetActive(false);

        IsFuelCellAvailable = false;
        startTutorial();
    }


    public void startTutorial() 
    {
        StartCoroutine(PlayIntroAudio());

    }

    public void startMission1()
    {
        state = GameState.Mission1;
        BrokenFuelCellVisual.SetActive(false);
        FuelCellWeldable.SetActive(true);
        isMission1Welded = true;
        IsFuelCellAvailable = true;
        TVScreen.GetComponent<Image>().sprite = M1Screen;
        SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.StartMission1);
        SubtitlePlayer.Instance.StopAllSubtitles();
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[6]);
        sirenLight.GetComponent<MeshRenderer>().material = sirenWarning;
        SoundManager.Instance.StopBackground();
        SoundManager.Instance.PlayAmbient();
    }

    public void startFreeBuild() {
        StartCoroutine(PlayFreeBuildAudio());
        SoundManager.Instance.StopAmbient();
        SoundManager.Instance.PlayBackground();
        state = GameState.FreeBuild;
        sirenLight.GetComponent<MeshRenderer>().material = sirenNormal;
        TVScreen.GetComponent<Image>().sprite = FreeBuildScreen;
        foreach (var i in ChangedLight)
        {
            i.SetActive(false);
        }
    }
    private IEnumerator PlayFreeBuildAudio() {
        float duration = SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.PlayerTurnFuelTank);
        SubtitlePlayer.Instance.StopAllSubtitles();
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[3]);
        yield return new WaitForSeconds(duration+2f);
        duration = SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.FreebuildIntro);
        mainCOunterAnimation.SetTrigger("IsOpen");
        SubtitlePlayer.Instance.StopAllSubtitles();
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[4]);
        yield return new WaitForSeconds(duration);
        yield return new WaitForSeconds(2f);
        SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.Ending);
        MissionManager.Instance.UpdateBriefing("Whenever you are finished with your art, return to the hallway to be rescued.");
        SubtitlePlayer.Instance.StopAllSubtitles();
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[5]);
        endCollider.SetActive(true);
        state = GameState.End;    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator PlayIntroAudio() {
        yield return new WaitForSecondsRealtime(1f);

        float clipDuration = SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.IntroVO);
        SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[0]);
        yield return new WaitForSecondsRealtime(clipDuration);
        yield return new WaitForSecondsRealtime(0.2f);
        if (!inMainRoom)
        {
            clipDuration = SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.CollisionVO);
            SubtitlePlayer.Instance.StopAllSubtitles();
            SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[1]);
            yield return new WaitForSecondsRealtime(clipDuration);
        }
        if (!inMainRoom)
        {
            clipDuration = SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.HallwayVO);
            SubtitlePlayer.Instance.StopAllSubtitles();
            SubtitlePlayer.Instance.PlaySubtitleBlock(subtitles[2]);
            yield return new WaitForSecondsRealtime(clipDuration);
        }

        yield return new WaitForSecondsRealtime(10f);
        if (!hasPickUpMask) {
            MeshRenderer[] allRenderers = weldMask.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in allRenderers)
            {
                List<Material> currentMaterials = new List<Material>(renderer.sharedMaterials);
                currentMaterials.Add(glowMat);
                renderer.sharedMaterials = currentMaterials.ToArray();
            }
        }
    }
    public enum GameState
    {
        Menu,
        Tutorial,
        Mission1,
        FreeBuild,
        End
    }

}
