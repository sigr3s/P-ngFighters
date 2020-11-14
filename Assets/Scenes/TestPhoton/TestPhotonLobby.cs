using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TestPhotonLobby : MonoBehaviourPunCallbacks
{
    public string roomName;
    string gameScene = "TestGame";
    public Button joinRandomRoomButton, createOrJoinCustomRoomButton, closeButton;
    public TMP_InputField createOrJoinCustomRoomInputField;
    public GameObject waitingLabel;
    public CanvasGroup lobbyUi;
    bool joiningRandomRoom;

    void Awake() 
    {
        waitingLabel.SetActive(false);
        joinRandomRoomButton.onClick.AddListener(JoinRandomRoom);
        createOrJoinCustomRoomButton.onClick.AddListener(CreateCustomRoom);
        closeButton.onClick.AddListener(CancelJoin);
        lobbyUi.alpha = 0;
        lobbyUi.interactable = lobbyUi.blocksRaycasts = false;
    }

    void Start() 
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    void JoinRandomRoom()
    {
        lobbyUi.interactable = lobbyUi.blocksRaycasts = false;  
        joiningRandomRoom = true;
        roomName = Guid.NewGuid().ToString();  
        PhotonNetwork.JoinRandomRoom();
    }

    void CreateCustomRoom()
    {
        joiningRandomRoom = false;
        roomName = createOrJoinCustomRoomInputField.text;   
        CreateRoom(false); 
    }

    void CancelJoin() 
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            StopCoroutine(WaitForOtherPlayer());
            PhotonNetwork.LeaveRoom();
            waitingLabel.SetActive(false);
        }
    }

    void CreateRoom(bool visible = true)
    {
        if(!string.IsNullOrEmpty(roomName))
        {            
            lobbyUi.interactable = lobbyUi.blocksRaycasts = false;  
            RoomOptions roomOptions = new RoomOptions(){ IsVisible = visible, IsOpen = true, MaxPlayers = (byte)2 };
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }
        
    #region Photon methods
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();  
        lobbyUi.alpha = 1;
        lobbyUi.interactable = lobbyUi.blocksRaycasts = true;  
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        if(joiningRandomRoom)
        {
            CreateRoom();
        }
        else
        {
            Debug.Log("[OnJoinRandomFailed]This room name is taken already");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if(joiningRandomRoom)
        {
            Debug.Log("[OnCreateRoomFailed]Failed to create room, tying again.");
            CreateRoom();
        }
        else
        {
            Debug.Log("[OnCreateRoomFailed]This room name is taken already");
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public override void OnJoinedRoom() 
    {
        StartCoroutine(WaitForOtherPlayer());
    }

    IEnumerator WaitForOtherPlayer()
    {        
        waitingLabel.SetActive(true);
        lobbyUi.interactable = lobbyUi.blocksRaycasts = true;  
        PhotonNetwork.AutomaticallySyncScene = true;
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == 2);
        waitingLabel.SetActive(false);
        PhotonNetwork.LoadLevel(gameScene);
    }
    #endregion
}
