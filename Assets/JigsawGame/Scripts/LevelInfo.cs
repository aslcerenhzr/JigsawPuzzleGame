using UnityEngine;

//[CreateAssetMenu(fileName = "NewScriptableObjectScript", menuName = "Scriptable Objects/NewScriptableObjectScript")]

[System.Serializable]
public class LevelInfo
{
    public string levelName;
    public int size;
    public Material puzzleMaterial;
    public bool isUnlocked;
}
