using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public LevelDatabase levelDatabase;
    public GameObject levelButtonPrefab;
    public Transform buttonContainer;

    private void Start()
    {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        for (int i = 0; i < levelDatabase.levels.Count; i++)
        {
            LevelInfo info = levelDatabase.levels[i];

            GameObject buttonObj = Instantiate(levelButtonPrefab, buttonContainer);

            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();


            levelButton.Setup(info);
        }
    }
}

