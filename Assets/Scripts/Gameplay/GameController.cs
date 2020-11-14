using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public Transform Player1Spawn;
    public Transform Player2Spawn;   

    public GameObject PlayerPrefab;

    private PlayerController player1;
    private PlayerController player2;

    // UI
    [Header("UI")]
    [SerializeField] private Image player1HealthImage = null;
    [SerializeField] private Image player1SuperImage = null;
    [SerializeField] private Image player2HealthImage = null;
    [SerializeField] private Image player2SuperImage = null;

    // Round
    private int currentRound = 0; // We asume a best of 3
    private int player1WonRounds = 0;
    private int player2WonRounds = 0;

    private void Start()
    {
        InstantiatePlayers();
        ResetHUD();
    }

    public virtual void InstantiatePlayers()
    {
        if(DataUtility.gameData.isNetworkedGame){
            Debug.LogWarning("NETWORK SPAWN PLAYERS GOES HERE!");
        }
        else {
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

    public virtual void StartNewRound()
    {
        currentRound += 1;
        ResetHUD();
        // TODO - Reset healths, scene, etc
    }

    public virtual void EndRound()
    {
        // TODO - Check for round winner
        if (player1WonRounds >= 2) {
            GameFinished(0);
        }
        else if (player2WonRounds >= 2) {
            GameFinished(1);
        }
        else {
            // TODO - Round transitions
            StartNewRound();
        }
    }

    public virtual void ResetHUD()
    {
        player1HealthImage.fillAmount = 1.0f;
        player2HealthImage.fillAmount = 1.0f;
    }

    public virtual void GameFinished(int winnerId)
    {
        // TODO - Winner announcement and end sequence
    }
}