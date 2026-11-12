using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TextDisplayMode
{
    Instant,     // Displays the whole sentence at once
    ByCharacter, // Types out letter by letter
    ByWord       // Types out word by word
}

[System.Serializable]
public class SubtitleLine
{
    public string text;
    public float displayTime = 3.0f;
}

[System.Serializable]
public class SubtitleData
{
    public List<SubtitleLine> sentences = new List<SubtitleLine>();
    public TextDisplayMode displayMode = TextDisplayMode.Instant;

    // Only applicable to By Character or By Word
    public float typingSpeed = 0.05f;
    public TMP_Text targetTextWindow;
}

public class SubtitlePlayer: MonoBehaviour
{
    public static SubtitlePlayer Instance { get; private set; }

    private Dictionary<TMP_Text, Coroutine> activeSubtitles = new Dictionary<TMP_Text, Coroutine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySubtitleBlock(SubtitleData data)
    {
        if (data.targetTextWindow == null)
        {
            return;
        }

        StopSubtitle(data.targetTextWindow);
        activeSubtitles[data.targetTextWindow] = StartCoroutine(PlayBlockRoutine(data));
    }


    public void StopSubtitle(TMP_Text targetWindow)
    {
        if (targetWindow != null && activeSubtitles.ContainsKey(targetWindow))
        {
            if (activeSubtitles[targetWindow] != null)
            {
                StopCoroutine(activeSubtitles[targetWindow]);
            }

            targetWindow.text = "";
            targetWindow.gameObject.SetActive(false);
            activeSubtitles.Remove(targetWindow);
        }
    }

    public void StopAllSubtitles()
    {
        foreach (var kvp in activeSubtitles)
        {
            if (kvp.Value != null) StopCoroutine(kvp.Value);
            if (kvp.Key != null)
            {
                kvp.Key.text = "";
                kvp.Key.gameObject.SetActive(false);
            }
        }
        activeSubtitles.Clear();
    }

    private IEnumerator PlayBlockRoutine(SubtitleData data)
    {
        data.targetTextWindow.gameObject.SetActive(true);

        foreach (SubtitleLine line in data.sentences)
        {
            data.targetTextWindow.text = "";
            float timeSpentTyping = 0f;

            if (data.displayMode == TextDisplayMode.Instant)
            {
                data.targetTextWindow.text = line.text;
            }
            else if (data.displayMode == TextDisplayMode.ByCharacter)
            {
                foreach (char c in line.text.ToCharArray())
                {
                    data.targetTextWindow.text += c;
                    timeSpentTyping += data.typingSpeed;
                    yield return new WaitForSeconds(data.typingSpeed);
                }
            }
            else if (data.displayMode == TextDisplayMode.ByWord)
            {
                string[] words = line.text.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    data.targetTextWindow.text += words[i] + (i < words.Length - 1 ? " " : "");
                    timeSpentTyping += data.typingSpeed;
                    yield return new WaitForSeconds(data.typingSpeed);
                }
            }

            float remainingTime = line.displayTime - timeSpentTyping;

            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }
            else if (remainingTime < 0)
            {
                Debug.LogWarning($"SubtitlePlayer Typing took longer expected");
            }
        }

        data.targetTextWindow.text = "";
        data.targetTextWindow.gameObject.SetActive(false);
        activeSubtitles.Remove(data.targetTextWindow);
    }


}
