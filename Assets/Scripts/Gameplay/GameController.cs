using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using Photon.Realtime;
using TMPro;

public class GameController : MonoBehaviourPunCallbacks {

    [Header("Always")]
    public Transform Player1Spawn;
    public Transform Player2Spawn;   
    private PlayerController player1;
    private PlayerController player2;
    [SerializeField] private HazardSpawner hazardSpawner = null;

    [Header("Network")]
    string pathRelativeToResources = "PhotonPrefabs";
    string prefabName => "PunPlayer";

    [Header("Local")]
    public GameObject PlayerPrefab;

    // UI
    [Header("UI")]
    [SerializeField] private Image player1HealthImage = null;
    [SerializeField] private Image player1SuperImage = null;
    [SerializeField] private Image player2HealthImage = null;
    [SerializeField] private Image player2SuperImage = null;
    [SerializeField] private TMP_Text upperText = null;
    [SerializeField] private TMP_Text lowerText = null;

    // Round
    private int currentRound = 0; // We asume a best of 3
    private int player1WonRounds = 0;
    private int player2WonRounds = 0;
    PlayerController player;


    private void Start()
    {
        InstantiatePlayers();
        ResetHUD();

        if(!DataUtility.gameData.isNetworkedGame){
            StartNewRound();
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameFinished(player1 != null ? 0 : 1);
    }


    private void OnDestroy()
    {
        player1.OnUIShouldUpdate -= OnUIShouldUpdate;
        player2.OnUIShouldUpdate -= OnUIShouldUpdate;
    }

    public virtual void InstantiatePlayers()
    {
        if(DataUtility.gameData.isNetworkedGame)
        {            
            Vector3 pos = PhotonNetwork.IsMasterClient ? Player1Spawn.transform.position : Player2Spawn.transform.position;            
            player = PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), pos, Quaternion.identity).GetComponentInChildren<PlayerController>();
            player.Initialize(PhotonNetwork.IsMasterClient ? PlayerID.Player1 : PlayerID.Player2, true);
            photonView.RPC("RPC_SendTeam", RpcTarget.OthersBuffered, PhotonNetwork.IsMasterClient ? PlayerID.Player1 : PlayerID.Player2);
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
        hazardSpawner.StartRound();
    }

    public virtual void EndRound()
    {
        hazardSpawner.CleanAll();

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
        upperText.text = "";
        lowerText.text = "";
    }

    public virtual void GameFinished(int winnerId)
    {
        Debug.Log("Player "+(winnerId+1)+" won the game!!!");

        StartCoroutine(LeaveGame());
    }

    private IEnumerator LeaveGame(){
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
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
    
    [PunRPC]
    void RPC_SendTeam(int team)
    {        
        foreach (var playerController in FindObjectsOfType<PlayerController>())
        {
            if(playerController != player)
            {
                playerController.Initialize((PlayerID)team, false);
            }

            if(playerController.playerID == PlayerID.Player1)
            {
                player1 = playerController;
            }
            
            if(playerController.playerID == PlayerID.Player2)
            {
                player2 = playerController;
            }
        }

        if(player1 != null && player2 != null){
            player1.OnUIShouldUpdate += OnUIShouldUpdate;
            player2.OnUIShouldUpdate += OnUIShouldUpdate; 
            StartNewRound();
        }
    }
}