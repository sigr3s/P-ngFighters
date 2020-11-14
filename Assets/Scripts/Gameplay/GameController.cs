using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour {

    
    public InputDevice player1Device;
    public InputDevice player2Device;

    public Transform Player1Spawn;
    public Transform Player2Spawn;   

    public GameObject PlayerPrefab;

    private PlayerController player1;
    private PlayerController player2;

    private void Start() {
        InstantiatePlayers();
    }

    public virtual void InstantiatePlayers(){
        var devices = InputSystem.devices.Where(x => x is Gamepad || x is Keyboard );
        
        if(devices.Count() > 1){
            player1Device = devices.ElementAt(0);
            player2Device = devices.ElementAt(1);

            Debug.Log(player1Device.name);
            Debug.Log(player2Device.name);
        }
        else{
            player1Device = devices.ElementAt(0);
            player2Device = null;
        }
        
        PlayerInput player1Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 0, splitScreenIndex: -1,
                controlScheme: null, pairWithDevice: player1Device);

        player1 = player1Input.GetComponentInChildren<PlayerController>();
        player1.Initialize(PlayerID.Player1, true);
        player1.transform.position = Player1Spawn.transform.position;

        PlayerInput player2Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 1, splitScreenIndex: -1,
               controlScheme: null, pairWithDevice: player2Device);
                
        player2 = player2Input.GetComponentInChildren<PlayerController>();
        player2.Initialize(PlayerID.Player2, true);
        player2.transform.position = Player2Spawn.transform.position;
    }
}