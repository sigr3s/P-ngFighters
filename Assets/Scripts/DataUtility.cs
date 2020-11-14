using System;
using UnityEngine;

public class DataUtility : MonoBehaviour {
    [SerializeField] private GameData m_gameData;


    public static GameData gameData = default(GameData);

    public static Color GetColorFor(PlayerID playerID)
    {
        switch(playerID){
            case PlayerID.NP:
                return gameData.NoPlayerColor;
            case PlayerID.Player1:
                return gameData.Player1Color;
            case PlayerID.Player2:
                return gameData.Player2Color;
        }

        return Color.magenta;
    }


    private void Awake() {
        if(DataUtility.gameData == null) {
            UpdateData();
        }
    }

    private void UpdateData(){
        DataUtility.gameData = m_gameData;
    }

#if UNITY_EDITOR
    private void OnValidate() {
        UpdateData();
    }
#endif
}