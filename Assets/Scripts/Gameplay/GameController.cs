using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;

public class GameController : MonoBehaviour {

    [Header("Always")]
    public Transform Player1Spawn;
    public Transform Player2Spawn;   
    private PlayerController player1;
    private PlayerController player2;

    [Header("Network")]
    string pathRelativeToResources = "PhotonPrefabs";
    string prefabName => "PhotonDummyPlayer";

    [Header("Local")]
    public GameObject PlayerPrefab;

    // UI
    [Header("UI")]
    [SerializeField] private Image player1HealthImage = null;
    [SerializeField] private Image player1SuperImage = null;
    [SerializeField] private Image player2HealthImage = null;
    [SerializeField] private Image player2SuperImage = null;

    // Round
    private int currentRound = 1; // We asume a best of 3
    private int player1WonRounds = 0;
    private int player2WonRounds = 0;

    private void Start()
    {
        InstantiatePlayers();
        ResetHUD();
        Debug.Log("Round 1, Fight!");
    }

    private void OnDestroy()
    {
        player1.OnUIShouldUpdate -= OnUIShouldUpdate;
        player2.OnUIShouldUpdate -= OnUIShouldUpdate;
    }

    public virtual void InstantiatePlayers()
    {
        if(DataUtility.gameData.isNetworkedGame){
            Debug.LogWarning("NETWORK SPAWN PLAYERS GOES HERE!");
            PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), Vector3.zero, Quaternion.identity);
        }
        else {
            PlayerInput player1Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 0, splitScreenIndex: -1,
            controlScheme: null, pairWithDevice: DataUtility.gameData.player1Device);

            player1 = player1Input.GetComponentInChildren<PlayerController>();
            player1.Initialize(PlayerID.Player1, true);
            player1.transform.position = Player1Spawn.transform.position;
            player1.OnUIShouldUpdate += OnUIShouldUpdate;

            PlayerInput player2Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 1, splitScreenIndex: -1,
                controlScheme: null, pairWithDevice: DataUtility.gameData.player2Device);
                    
            player2 = player2Input.GetComponentInChildren<PlayerController>();
            player2.Initialize(PlayerID.Player2, true);
            player2.transform.position = Player2Spawn.transform.position;
            player2.OnUIShouldUpdate += OnUIShouldUpdate;
        }
    }

    public virtual void StartNewRound()
    {
        currentRound += 1;
        ResetHUD();
        // TODO - Reset healths, scene, etc
        player1.health = 100.0f;
        player2.health = 100.0f;
        Debug.Log("Round "+currentRound+", Fight!");
    }

    public virtual void EndRound()
    {
        if (player1.health <= 0.0f) {
            player2WonRounds++;
            Debug.Log("Player 2 won the round!");
        } else if (player2.health <= 0.0f) {
            player1WonRounds++;
            Debug.Log("Player 1 won the round!");
        }
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
        Debug.Log("Player "+(winnerId+1)+" won the game!!!");
    }

    public virtual void OnUIShouldUpdate()
    {
        player1HealthImage.fillAmount = player1.health / 100.0f;
        player2HealthImage.fillAmount = player2.health / 100.0f;
        player1SuperImage.fillAmount = player1.super / 100.0f;
        player2SuperImage.fillAmount = player2.super / 100.0f;

        if (player1.health <= 0.0f || player2.health <= 0.0f) {
            EndRound();
        }
    }
}