using UnityEngine;
using TMPro; 

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI briefingText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void UpdateBriefing(string newText)
    {
        if (briefingText != null)
        {
            briefingText.text = newText;
        }
    }
}