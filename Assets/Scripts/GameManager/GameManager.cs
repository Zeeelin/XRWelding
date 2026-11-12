using UnityEngine;
/*
public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public GameState state;

    [Header("Start Game UI")]
    public GameObject MainMenuUIButton;


    [Header("Mission 1 UI")]
    public GameObject M1Panel;
    public GameObject M1Retry;
    public GameObject M1Next;
    public GameObject[] Mission1GO;

    public GameObject WatchUI;

    [Header("Mission 2 UI")]
    public GameObject M2Panel;
    public GameObject[] Mission2GO;

    public bool isStart;

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

        state = GameState.Menu;
        HandleMenu();
        foreach (GameObject i in Mission1GO) { 
            i.SetActive(false);
        }
        foreach (GameObject i in Mission2GO)
        {
            i.SetActive(false);
        }
    }

    /*public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.Menu:
                HandleMenu();
                break;

            case GameState.Mission:
                break;

            case GameState.End:
                break;

        }
    }*/
/*
    void HandleMenu() 
    {

        MainMenuUIButton.SetActive(true);
        Time.timeScale = 0f;

    }

    public void startGame() // Start Mission1
    {

        MainMenuUIButton.SetActive(false);
        Time.timeScale = 1f;
        state = GameState.Mission;
        foreach (GameObject items in Mission1GO)
        {
            items.SetActive(true);
        }
        M1Panel.SetActive(true);
        SoundManager.Instance.PlayRobotLine(SoundManager.RobotClip.IntroVO);
        WatchUI.SetActive(true);

    }

    public void startMission2()
    {

        M1Panel.SetActive(false);
        foreach (GameObject i in Mission1GO)
        {
            if (i != null)
            {
                i.SetActive(false);
            }
        }
        foreach (GameObject i in Mission2GO)
        {
            i.SetActive(true);
        }
        M2Panel.SetActive(true);

    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
*/
/*
public enum GameState
{
    Menu,
    Mission,
    End
}
*/