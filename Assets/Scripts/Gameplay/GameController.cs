using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour {

    public Transform Player1Spawn;
    public Transform Player2Spawn;   

    public GameObject PlayerPrefab;

    private PlayerController player1;
    private PlayerController player2;

    private void Start() {
        InstantiatePlayers();
    }

    public virtual void InstantiatePlayers(){

        if(DataUtility.gameData.isNetworkedGame){
            Debug.LogWarning("NETWORK SPAWN PLAYERS GOES HERE!");
        }
        else{
            PlayerInput player1Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 0, splitScreenIndex: -1,
            controlScheme: null, pairWithDevice: DataUtility.gameData.player1Device);

            player1 = player1Input.GetComponentInChildren<PlayerController>();
            player1.Initialize(PlayerID.Player1, true);
            player1.transform.position = Player1Spawn.transform.position;

            PlayerInput player2Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 1, splitScreenIndex: -1,
                controlScheme: null, pairWithDevice: DataUtility.gameData.player2Device);
                    
            player2 = player2Input.GetComponentInChildren<PlayerController>();
            player2.Initialize(PlayerID.Player2, true);
            player2.transform.position = Player2Spawn.transform.position;
        }
    }
}