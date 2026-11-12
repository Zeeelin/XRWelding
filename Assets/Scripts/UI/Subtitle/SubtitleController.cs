using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attach to a UI panel that has a TextMeshProUGUI child.
/// Handles typewriter-style subtitle display with skip, cancel, and auto-hide.
/// Controlled entirely by SubtitleManager — no Update loop needed.
public class SubtitleController : MonoBehaviour
{
    [Header("TMP Reference")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Typing Speed (characters per second)")]
    [SerializeField] private float textSpeed = 30f;

    [Header("Seconds to wait after last line finishes before hiding")]
    [SerializeField] private float autoHideDelay = 5f;

    // Internal state
    private string[] lines;
    private int index;
    private Coroutine typingRoutine;
    private Coroutine hideRoutine;

    private void Start()
    {
        textComponent.text = string.Empty;
    }

    /// <summary>
    /// Activate the panel and begin typing the provided lines.
    /// Interrupts any subtitle already in progress.
    /// </summary>
    public void BeginSub(string[] newLines)
    {
        if (newLines == null || newLines.Length == 0)
        {
            return;
        }

        lines = newLines;
        index = 0;

        StopAllActive();
        gameObject.SetActive(true);

        // Delay one frame on first activation so TMP initializes properly
        typingRoutine = StartCoroutine(BeginThenType());
    }

    /// <summary>
    /// If currently typing ? finish the line instantly.
    /// If line is fully shown ? advance to next line.
    /// </summary>
    public void SkipOrNext()
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        bool lineFullyRevealed =
            textComponent.maxVisibleCharacters >= lines[index].Length;

        if (lineFullyRevealed)
        {
            NextLine();
        }
        else
        {
            // Finish current line instantly
            StopTyping();
            textComponent.maxVisibleCharacters = lines[index].Length;

            if (index >= lines.Length - 1)
            {
                ScheduleHide();
            }
        }
    }

    /// <summary>
    /// Immediately stop everything and deactivate the panel.
    /// </summary>
    public void CancelSub()
    {
        StopAllActive();
        textComponent.text = string.Empty;
        textComponent.maxVisibleCharacters = 0;
        gameObject.SetActive(false);
    }


    /// <summary>
    /// Waits one frame for TMP to initialize, then starts typing.
    /// Avoids the "missing first character" issue on first panel activation.
    /// </summary>
    private IEnumerator BeginThenType()
    {
        textComponent.text = string.Empty;
        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate(true);
        yield return null; // Wait one frame for TMP layout

        typingRoutine = StartCoroutine(TypeLine());
    }

    /// <summary>
    /// Reveals the current line one character at a time using maxVisibleCharacters.
    /// No string allocations — TMP handles the reveal natively.
    /// </summary>
    private IEnumerator TypeLine()
    {
        float speed = textSpeed > 0f ? textSpeed : 30f;

        // Set full text but hide all characters
        textComponent.text = lines[index];
        textComponent.maxVisibleCharacters = 0;

        int totalChars = lines[index].Length;

        for (int i = 1; i <= totalChars; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(1f / speed);
        }

        typingRoutine = null;

        // Auto-hide after last line finishes
        if (index >= lines.Length - 1)
        {
            ScheduleHide();
        }
    }

    // ?????????????????????????????????????????????
    //  Line Advancement & Auto-Hide
    // ?????????????????????????????????????????????

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            StopTyping();
            CancelHide();
            typingRoutine = StartCoroutine(TypeLine());
        }
        else
        {
            // Already on last line — schedule hide if not already scheduled
            ScheduleHide();
        }
    }

    private void ScheduleHide()
    {
        CancelHide();
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        textComponent.text = string.Empty;
        textComponent.maxVisibleCharacters = 0;
        gameObject.SetActive(false);
        hideRoutine = null;
    }

    // ?????????????????????????????????????????????
    //  Helpers
    // ?????????????????????????????????????????????

    private void StopTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }
    }

    private void CancelHide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
    }

    private void StopAllActive()
    {
        StopTyping();
        CancelHide();
    }
}
