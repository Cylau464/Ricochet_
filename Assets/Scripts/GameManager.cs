using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float _levelCompletedDelay = 1f;
    private int _enemyCount;
    public int EnemyCount
    {
        get { return _enemyCount; }
    }
    private bool _isVictory;

    public static UnityEvent levelCompletedEvent, gameOverEvent, levelStart, levelLeave, levelRestart;
    public static UnityEvent moneyChangeEvent = new UnityEvent();
    public static GameManager current;
    private static int moneyAmount;
    public static int MoneyAmount
    {
        get { return moneyAmount; }
        set
        {
            moneyAmount = value;
            moneyChangeEvent.Invoke();
        }
    }

    float deltaTime = 0.0f;

    [Header("Debug")]
    [SerializeField] private bool _showFPS = false;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        _enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        DontDestroyOnLoad(gameObject);

        levelStart = new UnityEvent();
        levelCompletedEvent = new UnityEvent();
        levelCompletedEvent.AddListener(Victory);
        gameOverEvent = new UnityEvent();
        moneyChangeEvent = new UnityEvent();
        levelLeave = new UnityEvent();
        levelRestart = new UnityEvent();

        if (Application.isEditor == false && Application.isMobilePlatform == true)
        {
            Application.targetFrameRate = 60;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            SaveSystem.DeleteSave();
        }

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (_showFPS == true)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;

            style.normal.textColor = fps > 60f ? Color.green : Color.red;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        _enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        current._isVictory = false;
        current.CancelInvoke(nameof(LevelCompleted));
    }

    public static void EnemyIsDead()
    {
        current._enemyCount--;

        if (current._enemyCount <= 0)
            current.Invoke(nameof(LevelCompleted), current._levelCompletedDelay * Time.timeScale);
    }

    private void LevelCompleted()
    {
        levelCompletedEvent.Invoke();
    }

    private void Victory()
    {
        current._isVictory = true;
    }

    public static void GameOver()
    {
        if (current._isVictory == true) return;

        gameOverEvent.Invoke();
    }

    private void OnApplicationQuit()
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 1)
            levelLeave.Invoke();
    }
}
