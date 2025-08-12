using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public Image Image;
    public LevelInfo myLevelInfo;

    public void Setup(LevelInfo info)
    {
        myLevelInfo = info;
        if (info.puzzleMaterial != null && info.puzzleMaterial.mainTexture is Texture2D tex)
        {
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
            Image.sprite = sprite;
        }

        if (info.isUnlocked)
        {
            Image.color = Color.white; // Normal görünüm
        }
        else
        {
            Image.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Daha koyu görünüm
        }
    }

    public void OnClick()
    {
        if (myLevelInfo.isUnlocked)
        {
            LevelDataHolder.Instance.SelectedLevelInfo = myLevelInfo;
            SceneManager.LoadScene("MainLevel"); 
        }
    }

}

