using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Enums;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    //private GameMode _gameMode = GameMode.Classic;

    private static Level[] _levels;
    public static Level[] Levels
    {
        get { return _levels; }
    }
    public static Level currentLevel;
    public static Level nextLevel;
    public static int levelCount; // How many levels the player has played in all the time
    public static int levelLoop = 1; // The number of the passage of all levels
    public static int levelCompleted; // How many levels the player has completed in all the time

    private static bool _activateNextScene;

    private static LevelManager current;

    public static UnityEvent switchLevelEvent;

    private void Awake()
    {
        if(current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        switchLevelEvent = new UnityEvent();

        int sceneCount = SceneManager.sceneCountInBuildSettings - 2; // -2 bacause exluded start scene and main menu (0 and 1 index)
        _levels = new Level[sceneCount];
        string[] gameModes = System.Enum.GetNames(typeof(GameMode));
        string[] sceneNames = new string[sceneCount];

        for(int i = 0; i < sceneCount; i++)
        {
            string pathToScene = SceneUtility.GetScenePathByBuildIndex(i + 2); // +2 bacause exluded start scene and main menu (0 and 1 index)
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(pathToScene);
        }

        int sceneIndex = 0;
        int levelNumber = 1;

        foreach(string gameMode in gameModes)
        {
            var scenes = sceneNames.Where(scene => scene.Contains(gameMode));
            bool locked = true;

            // Unlock first classic level
            if (gameMode.Contains(GameMode.Classic.ToString()))
                locked = false;

            foreach (string scene in scenes)
            {
                _levels[sceneIndex] = new Level(sceneIndex + 2, scene, levelNumber, (GameMode)System.Enum.Parse(typeof(GameMode), gameMode), locked);
                levelNumber++;
                sceneIndex++;
                locked = true;
            }

            levelNumber = 1;
        }


        currentLevel = nextLevel = _levels[0];

        SaveData saveData = SaveSystem.LoadData();

        if (saveData != null)
        {
            Level[] matchingLevels = _levels;
            int length;

            if (_levels.Length < saveData.levels.Length)
                length = _levels.Length;
            else
                length = saveData.levels.Length;

            // So that when adding/removing/edit levels there are no matching conflicts
            for (int i = 0; i < length; i++)
            {
                matchingLevels[i].completed = saveData.levels[i].completed;
                matchingLevels[i].locked = saveData.levels[i].locked;
                matchingLevels[i].result = saveData.levels[i].result;
            }

            _levels = matchingLevels;
            currentLevel = nextLevel = saveData.lastLevel;
            levelCount = saveData.levelCount;
            levelCompleted = saveData.levelCompleted;
            levelLoop = saveData.levelLoop;
            GameManager.MoneyAmount = saveData.moneyAmount;

            RateUs.isDisabled = saveData.rateUsDisabled;
            RateUs.nextShowDatetime = saveData.rateUsNextShowDatetime;

            AdManager.noAdsPurchased = saveData.noAdsPurchased;
        }

        SceneManager.LoadScene(currentLevel.levelName);
    }

    private void Start()
    {
        // Invoke for other listeners which wanna be check _currentlevel before it change
        GameManager.levelCompletedEvent.AddListener(() => Invoke(nameof(LevelCompleted), Time.deltaTime));
        GameManager.levelStart.AddListener(() => {
            levelCount++;
            SaveSystem.SaveData();
            AppMetricaEvents.Instance.LevelStart();
        });
    }

    private void LevelCompleted()
    {
        if(RateUs.isDisabled == false)
        {
            if (RateUs.nextShowDatetime == default)
            {
                if (currentLevel.levelNumber == RateUs.firstShowLevelNumber)
                    UIMain.ShowRateUsPopup();
            }
            else if(RateUs.nextShowDatetime <= System.DateTime.Now)
            {
                UIMain.ShowRateUsPopup();
            }
        }

        currentLevel.Complete(Mathf.Clamp(ScoreHandler.curNumberOfThrows, 0, 3));
        string[] split = currentLevel.levelName.Split("_"[0]);
        int levelIndex = int.Parse(split.Last());
        int areaIndex = int.Parse(split[split.Length - 2]);
        //int levelIndex = int.Parse(Regex.Replace(currentLevel.levelName, "[^0-9]", string.Empty));
        levelIndex++;
        string levelName = Regex.Replace(currentLevel.levelName, @"[\d-]", string.Empty);
        levelName = levelName.Remove(levelName.Length - 1);
        string fullLevelName = levelName + areaIndex.ToString() + "_" + levelIndex.ToString();
        nextLevel = _levels.FirstOrDefault(i => i.levelName == fullLevelName);
        levelCompleted++;

        if (nextLevel == null)
        {
            areaIndex++;
            fullLevelName = levelName + areaIndex.ToString() + "_1";
            nextLevel = _levels.FirstOrDefault(i => i.levelName == fullLevelName);

            if (nextLevel == null)
            {
                Debug.LogWarning("Last level");
                fullLevelName = levelName + "1_1";
                nextLevel = _levels.FirstOrDefault(i => i.levelName == fullLevelName);
                
                if(levelCompleted >= _levels.Length * levelLoop)
                    levelLoop++;
            }
        }

        nextLevel?.Unlock();
        current.StartCoroutine(LoadScene());
        SaveSystem.SaveData();

        if(ApplovinInterstitialsAd.isEnabled == true)
        {
            if (MaxSdk.IsInterstitialReady(ApplovinInterstitialsAd.adUnitId))
            {
                MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialDisplayedEvent;
                MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += OnInterstitialAdFailedToDisplayEvent;

                AppMetricaEvents.Instance.AdAvailable(AdType.Interstitial, "level_finish", "success");
                AdManager.BannerCannotBeViewed();
                MaxSdk.ShowInterstitial(ApplovinInterstitialsAd.adUnitId);
            }
            else
            {
                AppMetricaEvents.Instance.AdAvailable(AdType.Interstitial, "level_finish", "not_available");
            }
        }
    }

    public static void ActivateNextLevel()
    {
        currentLevel = nextLevel;
        _activateNextScene = true;
        switchLevelEvent.Invoke();
    }

    private IEnumerator LoadScene()
    {
        AsyncOperation loadingLevel = SceneManager.LoadSceneAsync(nextLevel.ID);
        loadingLevel.allowSceneActivation = false;

        while (!loadingLevel.isDone)
        {
            // Check if the load has finished
            if (loadingLevel.progress >= 0.9f)
            {
                if (_activateNextScene)
                {
                    //Activate the Scene
                    loadingLevel.allowSceneActivation = true;
                    _activateNextScene = false;
                }
            }

            yield return null;
        }
    }

    public static void Restart()
    {
        GameManager.levelRestart.Invoke();
        nextLevel = currentLevel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ToMainMenu()
    {
        GameManager.levelLeave.Invoke();
        currentLevel = nextLevel;
        SceneManager.LoadScene(1);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId)
    {
        AppMetricaEvents.Instance.AdStarted(AdType.Interstitial, "level_finish", "start");
        AppMetricaEvents.Instance.AdWatch(AdType.Interstitial, "level_finish", "watched");

        MaxSdkCallbacks.OnInterstitialDisplayedEvent -= OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent -= OnInterstitialAdFailedToDisplayEvent;
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        AppMetricaEvents.Instance.AdStarted(AdType.Interstitial, "level_finish", "not_available");

        MaxSdkCallbacks.OnInterstitialDisplayedEvent -= OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent -= OnInterstitialAdFailedToDisplayEvent;
    }

    public static void OpenAllLevels()
    {
        foreach(Level level in _levels)
        {
            level.locked = false;
        }
    }
}
