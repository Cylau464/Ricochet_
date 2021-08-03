using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelSpawner : MonoBehaviour
{
    [SerializeField] private int _horizontalChildsCount = 3;

    [SerializeField] private LevelArea[] _levelAreas = null;
    [SerializeField] private GameObject _levelAreaPrefab = null;

    private void Start()
    {
        foreach(LevelArea area in _levelAreas)
        {
            GameObject inst = Instantiate(_levelAreaPrefab, transform.position, Quaternion.identity, transform);
            Image image = inst.GetComponentInChildren<Image>();
            image.sprite = area.backgroundImage;
            var levels = LevelManager.Levels.Where(level => level.gameMode == area.gameMode);
            List<GameObject> buttons = new List<GameObject>();
            GridLayoutGroup grid = inst.GetComponentInChildren<GridLayoutGroup>();

            for (int j = 1; j <= area.levelsCount; j++)
            {
                string levelName = area.gameMode.ToString() + "_Level_" + area.areaIndex + "_" + j;
                Level level = levels.FirstOrDefault(item => item.levelName == levelName);
                buttons.Add(Instantiate(area.levelButtonPrefab, Vector3.zero, Quaternion.identity));
                buttons.Last().transform.SetParent(image.transform, false);
                buttons.Last().GetComponent<LevelButton>().SetLevel(level);
            }

            StartCoroutine(CentralizeIncompleteLine(buttons, area.levelsCount, grid));
        }

        Invoke(nameof(ScrollToTop), Time.deltaTime);
    }

    private void ScrollToTop()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector2(rectTransform.localPosition.x, -rectTransform.sizeDelta.y / 2f);
    }

    private IEnumerator CentralizeIncompleteLine(List<GameObject> buttons, int levelsCount, GridLayoutGroup grid)
    {
        yield return new WaitForEndOfFrame();

        int remainder = levelsCount % _horizontalChildsCount;

        if (_horizontalChildsCount == 3 && remainder == 1)
        {
            grid.enabled = false;
            RectTransform rect = buttons.Last().GetComponent<RectTransform>();
            rect.position = new Vector3(buttons[1].transform.position.x, rect.position.y, rect.position.z);
        }
        else if(_horizontalChildsCount == 4 && (remainder == 1 || remainder == 2))
        {
            grid.enabled = false;
            RectTransform rect = buttons[buttons.Count - 2].GetComponent<RectTransform>();
            rect.position = new Vector3(buttons[1].transform.position.x, rect.position.y, rect.position.z);
            RectTransform rect2 = buttons.Last().GetComponent<RectTransform>();
            rect2.position = new Vector3(buttons[2].transform.position.x, rect2.position.y, rect2.position.z);
        }
    }
}
