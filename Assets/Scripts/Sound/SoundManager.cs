using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Helper classes for Inspector mapping 
[System.Serializable]
public class Sound
{
    public SoundManager.SfxClip sfxType;
    public AudioClip[] audioClips;
}

[System.Serializable]
public class Robot
{
    public SoundManager.RobotClip RobotType;
    public AudioClip audioClip;
}

[System.Serializable]
public class MC
{
    public SoundManager.MCClip MCType;
    public AudioClip[] audioClips;
}

[System.Serializable]
public class RepeatingSFX
{
    public SoundManager.RepeatingSfxClip repeatSFXType;
    public AudioClip audioClip;
}

[System.Serializable]
public class BackgroundMusic
{
    public SoundManager.BackgroundClip backgroundType;
    public AudioClip audioClip;
}


public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    #region Enums
    /// <summary>
    /// Clips that is only played once
    /// </summary>
    public enum SfxClip
    {
        DoorOpen,
        DoorClose,
        SwitchOn,
        SwitchOff
    }

    /// <summary>
    /// Clips that replays until a stop is called
    /// </summary>
    public enum RepeatingSfxClip
    {
        WeldFail,
        WeldSuccess
    }

    public enum BackgroundClip
    {
        BackgroundMusic
    }

    public enum RobotClip
    {
        IntroVO,
        CollisionVO,
        HallwayVO,
        PerparationVO,
        MaskPickupVO,
        PlayerStartWelding,
        PlayerWeldTwoPiecesTogether,
        PlayerTurnsInNothing,
        PlayerTurnsInSuccessfully,
        StartMission1,
        FuelTankPickUp,
        FirstCrack,
        SecondCrack,
        ThirdCrack,
        PlayerTurnFuelTank,
        FreebuildIntro,
        Ending
    }

    public enum MCClip
    {
        FlashedVO
    }
    #endregion

    [Header("Audio Clip Source")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource robotSource;
    [SerializeField] private AudioSource mcSource;
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Repeating SFX Sources")]
    [SerializeField] private AudioSource WeldFailSource;
    [SerializeField] private AudioSource WeldSuccessSource;

    [Header("Audio Clip Lists")]
    [SerializeField] private Sound[] sfxClipList;
    [SerializeField] private Robot[] robotClipList;
    [SerializeField] private MC[] mcClipList;
    [SerializeField] private RepeatingSFX[] repeatingSfxClipList;

    private Dictionary<SfxClip, AudioClip[]> _sfxClips;
    private Dictionary<RobotClip, AudioClip> _robotClips;
    private Dictionary<MCClip, AudioClip[]> _mcClips;
    private Dictionary<RepeatingSfxClip, AudioClip> _repeatSfxClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxClips = new Dictionary<SfxClip, AudioClip[]>();
        foreach (var sound in sfxClipList)
        {
            _sfxClips.Add(sound.sfxType, sound.audioClips);
        }

        _robotClips = new Dictionary<RobotClip, AudioClip>();
        foreach (var line in robotClipList)
        {
            _robotClips.Add(line.RobotType, line.audioClip);
        }

        _mcClips = new Dictionary<MCClip, AudioClip[]>();
        foreach (var line in mcClipList)
        {
            _mcClips.Add(line.MCType, line.audioClips);
        }

        _repeatSfxClips = new Dictionary<RepeatingSfxClip, AudioClip>();
        foreach (var audio in repeatingSfxClipList)
        {
            _repeatSfxClips.Add(audio.repeatSFXType, audio.audioClip);
        }
    }
    private void Start()
    {

    }

    /// <summary>
    /// Play a short audio clip for sound effect, if one audio list have more than 
    /// one audio clip, randomly pick the line to play
    /// </summary>
    public void PlaySFX(SfxClip clip)
    {
        if (_sfxClips.TryGetValue(clip, out AudioClip[] clips))
        {
            if (clips.Length > 0)
            {
                AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
                sfxSource.PlayOneShot(clipToPlay);
            }
        }
    }

    /// <summary>
    /// Play Selected line from the delivery man
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    public float PlayRobotLine(RobotClip clip)
    {
        if (_robotClips.TryGetValue(clip, out AudioClip audioClip))
        {
            robotSource.Stop();
            robotSource.clip = audioClip;
            robotSource.Play();
            return audioClip.length;
        }
        return 0f;
    }

    /// <summary>
    /// Play a dialogue from the signer, if one audio list have more than 
    /// one audio clip, randomly pick the line to play
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    public float PlayMCLine(MCClip clip)
    {
        if (_mcClips.TryGetValue(clip, out AudioClip[] clips))
        {
            if (clips.Length > 0)
            {
                AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];
                mcSource.PlayOneShot(clipToPlay);
                return clipToPlay.length;
            }
        }
        return 0f;
    }

    /// <summary>
    /// Play a SFX repeatedly until it is triggered to stop.
    /// </summary>
    public void PlayRepeatingSFX(RepeatingSfxClip clip)
    {
        if (_repeatSfxClips.TryGetValue(clip, out AudioClip clipToPlay))
        {
            switch (clip)
            {
                case RepeatingSfxClip.WeldFail:
                    if (!WeldFailSource.isPlaying)
                    {
                        WeldFailSource.clip = clipToPlay;
                        WeldFailSource.loop = true;
                        WeldFailSource.Play();
                    }
                    break;
                case RepeatingSfxClip.WeldSuccess:
                    if (!WeldSuccessSource.isPlaying)
                    {
                        WeldSuccessSource.clip = clipToPlay;
                        WeldSuccessSource.loop = true;
                        WeldSuccessSource.Play();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Stop playing the repeating sound effects.
    /// </summary>
    public void StopRepeatingSFX(RepeatingSfxClip clip)
    {
        switch (clip)
        {
            case RepeatingSfxClip.WeldFail:
                WeldFailSource.Stop();
                WeldFailSource.clip = null;
                break;
            case RepeatingSfxClip.WeldSuccess:
                WeldSuccessSource.Stop();
                WeldSuccessSource.clip = null;
                break;
        }
    }

    public void StopBackground()
    {
        backgroundSource.Stop();
    }

    public void PlayBackground()
    {
        backgroundSource.Play();
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
    }

    public void PlayAmbient()
    {
        ambientSource.Play();
    }

    /// <summary>
    /// reset all music and looping sequence
    /// </summary>
    public void ResetSound()
    {
        // Stop all sources
        sfxSource.Stop();
        robotSource.Stop();
        mcSource.Stop();

        WeldFailSource.Stop();
        WeldFailSource.clip = null;

        WeldSuccessSource.Stop();
        WeldSuccessSource.clip = null;
    }

}
