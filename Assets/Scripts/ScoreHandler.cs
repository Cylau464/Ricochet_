using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    public static ScoreHandler current;

    [SerializeField] private int _awardForThrows = 100;
    public int AwardForThrows
    {
        get { return _awardForThrows; }
    }
    [SerializeField] private int _numberOfThrows = 3;
    public static int Throws
    { 
        get { return current._numberOfThrows; }
    }
    [SerializeField] private int _extraThrows = 1;
    public static int ExtraThrows
    {
        get { return current._extraThrows; }
    }
    public static int curNumberOfThrows;
    public static float timeSpentOnLevel;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;

        curNumberOfThrows = _numberOfThrows + _extraThrows;
    }

    private void Start()
    {
        timeSpentOnLevel = 0f;
        GameManager.levelStart.Invoke();
        //Debug.Log("LEVEL START " + LevelManager.levelCount);
    }

    private void Update()
    {
        timeSpentOnLevel += Time.deltaTime;
    }
}
