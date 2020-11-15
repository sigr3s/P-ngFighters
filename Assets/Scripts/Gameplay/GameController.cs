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
    [SerializeField] private Image roundInfoImage = null;
    [SerializeField] private TMP_Text upperText = null;
    [SerializeField] private TMP_Text lowerText = null;

    [SerializeField] private Image P1CloserRoundImage = null;
    [SerializeField] private Image P2CloserRoundImage = null;
    [SerializeField] private Image CenterRoundImage = null;

    // Round
    private int currentRound = 0; // We asume a best of 3
    private int player1WonRounds = 0;
    private int player2WonRounds = 0;
    PlayerController player;

    bool ongoingRound = false;


    private void Start()
    {
        SceneManager.LoadScene("Environment", LoadSceneMode.Additive);
        InstantiatePlayers();
        ResetHUD();
        roundInfoImage.gameObject.SetActive(true);
        upperText.text = "Round 1";
        lowerText.text = "3";
        if(!DataUtility.gameData.isNetworkedGame){
            StartCoroutine(StartNewRound());
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
            player.Initialize(PhotonNetwork.IsMasterClient ? PlayerID.Player1 : PlayerID.Player2, true, hazardSpawner);
            photonView.RPC("RPC_SendTeam", RpcTarget.OthersBuffered, PhotonNetwork.IsMasterClient ? PlayerID.Player1 : PlayerID.Player2);
        }
        else {
            PlayerInput player1Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 0, splitScreenIndex: -1,
            controlScheme: null, pairWithDevice: DataUtility.gameData.player1Device);

            player1 = player1Input.GetComponentInChildren<PlayerController>();
            player1.Initialize(PlayerID.Player1, true, hazardSpawner);
            player1.transform.position = Player1Spawn.transform.position;
            player1.OnUIShouldUpdate += OnUIShouldUpdate;

            PlayerInput player2Input = PlayerInput.Instantiate(PlayerPrefab, playerIndex: 1, splitScreenIndex: -1,
                controlScheme: null, pairWithDevice: DataUtility.gameData.player2Device);
                    
            player2 = player2Input.GetComponentInChildren<PlayerController>();
            player2.Initialize(PlayerID.Player2, true, hazardSpawner);
            player2.transform.position = Player2Spawn.transform.position;
            player2.OnUIShouldUpdate += OnUIShouldUpdate;
        }
    }

    public virtual IEnumerator StartNewRound()
    {
        currentRound += 1;
        ResetHUD();
        player1.health = 100.0f;
        player2.health = 100.0f;
        Debug.Log("Round "+currentRound+", Fight!");
        roundInfoImage.gameObject.SetActive(true);
        upperText.text = "Round "+currentRound;
        lowerText.text = "3";
        yield return new WaitForSeconds(1.0f);
        lowerText.text = "2";
        yield return new WaitForSeconds(1.0f);
        lowerText.text = "1";
        yield return new WaitForSeconds(1.0f);
        lowerText.text = "RUMBLE!";
        hazardSpawner.StartRound();
        yield return new WaitForSeconds(1.0f);
        upperText.text = "";
        lowerText.text = "";
        roundInfoImage.gameObject.SetActive(false);

        ongoingRound = true;
    }

    public virtual IEnumerator EndRound()
    {
        ongoingRound = false;
        Debug.Log("Clear?");
        hazardSpawner.CleanAll();

        if (player1.health <= 0.0f) {
            player2WonRounds++;
            Debug.Log("Player 2 won the round!");
            roundInfoImage.gameObject.SetActive(true);
            upperText.text = "Blue Player";
            lowerText.text = "wins the Round!";
        } else if (player2.health <= 0.0f) {
            player1WonRounds++;
            Debug.Log("Player 1 won the round!");
            roundInfoImage.gameObject.SetActive(true);
            upperText.text = "Red Player";
            lowerText.text = "wins the Round!";
        }

        P1CloserRoundImage.color = player1WonRounds > 0 ? DataUtility.GetColorFor(PlayerID.Player1) : Color.black;
        P2CloserRoundImage.color = player2WonRounds > 0 ? DataUtility.GetColorFor(PlayerID.Player2) : Color.black;

        if (player1WonRounds >= 2) {
            roundInfoImage.gameObject.SetActive(true);
            upperText.text = "Finished!";
            lowerText.text = "Red Player Victory!";
            CenterRoundImage.color = DataUtility.GetColorFor(PlayerID.Player1);
            GameFinished(0);
        }
        else if (player2WonRounds >= 2) {
            roundInfoImage.gameObject.SetActive(true);
            upperText.text = "Finished!";
            lowerText.text = "Blue Player Victory!";
            CenterRoundImage.color = DataUtility.GetColorFor(PlayerID.Player2);
            GameFinished(1);
        }
        else {
            // TODO - Round transitions
            yield return new WaitForSeconds(3.0f);
            StartCoroutine(StartNewRound());
        }
    }

    public virtual void ResetHUD()
    {
        player1HealthImage.fillAmount = 1.0f;
        player2HealthImage.fillAmount = 1.0f;
        upperText.text = "";
        lowerText.text = "";
        roundInfoImage.gameObject.SetActive(false);
    }

    public virtual void GameFinished(int winnerId)
    {
        Debug.Log("Player "+(winnerId+1)+" won the game!!!");

        StartCoroutine(LeaveGame());
    }

    private IEnumerator LeaveGame(){
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LeaveRoom(true);
        SceneManager.LoadScene(0);
        SceneManager.UnloadSceneAsync("Environment");
    }

    public virtual void OnUIShouldUpdate()
    {
        player1HealthImage.fillAmount = player1.health / 100.0f;
        player2HealthImage.fillAmount = player2.health / 100.0f;
        player1SuperImage.fillAmount = player1.super / 100.0f;
        player2SuperImage.fillAmount = player2.super / 100.0f;

        if (player1.health <= 0.0f || player2.health <= 0.0f) {
            if(ongoingRound){
                StartCoroutine(EndRound());
            }
        }
    }  
    
    [PunRPC]
    void RPC_SendTeam(int team)
    {        
        foreach (var playerController in FindObjectsOfType<PlayerController>())
        {
            if(playerController != player)
            {
                playerController.Initialize((PlayerID)team, false, hazardSpawner);
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
            StartCoroutine(StartNewRound());
        }
    }
}