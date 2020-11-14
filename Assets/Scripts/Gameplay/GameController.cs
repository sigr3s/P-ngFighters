using UnityEngine;

public class GameController : MonoBehaviour {

    public Transform Player1Spawn;
    public Transform Player2Spawn;   

    public PlayerController PlayerPrefab;

    private PlayerController player1;
    private PlayerController player2;

    private void Start() {
        InstantiatePlayers();
    }

    public virtual void InstantiatePlayers(){
        player1 = Instantiate(PlayerPrefab, Player1Spawn);
        player1.Initialize(PlayerID.Player1, true);
        
        player2 = Instantiate(PlayerPrefab, Player2Spawn);
        player2.Initialize(PlayerID.Player2, true);
    }
}