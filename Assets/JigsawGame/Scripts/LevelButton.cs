using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour
{
    public Image Image;

    public void Setup(Material mat)
    {
        Image.material = mat;
    }
}

