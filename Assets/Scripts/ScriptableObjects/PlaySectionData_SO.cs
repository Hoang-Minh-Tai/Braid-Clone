using UnityEngine;

[CreateAssetMenu(fileName = "PlaySectionData_SO", menuName = "Scriptable Objects/PlaySectionData_SO")]
public class PlaySectionData_SO : ScriptableObject
{
    public bool newGame = true;

    public int collectedPuzzlesLevel1 = 0;
    public int collectedPuzzlesLevel2 = 0;
    public int collectedPuzzlesLevel3 = 0;
    
}
