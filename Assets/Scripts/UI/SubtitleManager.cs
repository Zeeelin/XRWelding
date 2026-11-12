using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entry shown in the Inspector: an ID string paired with a .txt file.
/// Your game manager calls Play("this_id") to trigger that subtitle.
[Serializable]
public class SubtitleEntry
{
    [Tooltip("ID your game manager uses to trigger this subtitle")]
    public string id;

    [Tooltip("Drag a .txt file here — lines separated by --- or blank lines")]
    public TextAsset file;
}

/// <summary>
/// Singleton subtitle manager for a single-panel setup.
/// 

public class SubtitleManager : MonoBehaviour
{
    [Header("Panel Reference")]
    [Tooltip("Drag your subtitle panel (with SubtitleController) here")]
    [SerializeField] private SubtitleController panel;

    [Header("Subtitle Entries")]
    [Tooltip("Each entry maps an ID to a .txt subtitle file")]
    [SerializeField] private SubtitleEntry[] entries;

    // Runtime cache: id → parsed lines
    private readonly Dictionary<string, string[]> cache = new();

    // Singleton
    public static SubtitleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildCache();
    }

    /// <summary>
    /// Play a subtitle by its ID. The ID must match an entry in the Inspector.
    public void Play(string id)
    {
        if (panel == null)
        {
            Debug.LogWarning("[Subtitle] Panel not assigned in Inspector!");
            return;
        }

        var key = string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();

        if (!cache.TryGetValue(key, out var lines) || lines == null || lines.Length == 0)
        {
            Debug.LogWarning($"[Subtitle] Unknown id '{id}'. Check your entries.");
            return;
        }

        panel.BeginSub(lines);
    }

    /// <summary>
    /// If typing → finish line instantly. If line done → advance to next.
    public void Skip()
    {
        if (panel == null) return;
        panel.SkipOrNext();
    }

    /// <summary>
    /// Stop subtitle immediately and hide the panel.
    public void Cancel()
    {
        if (panel == null) return;
        panel.CancelSub();
    }

    /// <summary>
    /// True if the subtitle panel is currently visible.
    public bool IsActive => panel != null && panel.gameObject.activeSelf;


    private void BuildCache()
    {
        cache.Clear();

        if (entries == null) return;

        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.file == null)
            {
                continue;
            }

            var id = e.id.Trim();

            if (cache.ContainsKey(id))
            {
                Debug.LogWarning($"[Subtitle] Duplicate id '{id}' — skipped.");
                continue;
            }

            var lines = ParseText(e.file.text);

            if (lines.Length == 0)
            {
                Debug.LogWarning($"[Subtitle] '{id}' produced no lines — check the file.");
                continue;
            }

            cache.Add(id, lines);
        }
    }


    /// <summary>
    /// Splits raw text into subtitle pages.
    /// Priority: --- delimiter first, then double blank lines, then single block.
    private static string[] ParseText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        // Normalize line endings and strip BOM
        string s = raw.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
        s = s.TrimStart('\uFEFF');

        // Try --- delimiter first
        string[] byRule = s.Split(new[] { "\n---\n" }, StringSplitOptions.None);
        if (byRule.Length > 1)
        {
            return TrimAll(byRule);
        }

        // Fall back to double blank lines
        string[] byBlank = s.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (byBlank.Length > 0)
        {
            return TrimAll(byBlank);
        }

        return new[] { s };
    }

    private static string[] TrimAll(string[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = array[i].Trim();
        }

        return array;
    }
}
