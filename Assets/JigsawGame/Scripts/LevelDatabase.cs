using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Puzzle/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelInfo> levels;
}
