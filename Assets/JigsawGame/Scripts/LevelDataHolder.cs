using UnityEngine;

public class LevelDataHolder : MonoBehaviour
{
    public static LevelDataHolder Instance;

    public LevelInfo SelectedLevelInfo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
