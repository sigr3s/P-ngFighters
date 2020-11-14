using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "P@ngFighters/GameData", order = 0)]
public class GameData : ScriptableObject {
    public Color NoPlayerColor = Color.white;
    public Color Player1Color = Color.red;
    public Color Player2Color = Color.blue;
}