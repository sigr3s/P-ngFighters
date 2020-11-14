using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TestPhotonIntegration : MonoBehaviourPunCallbacks
{
    string roomName = "TestRoom";
    string gameScene = "Game";
    void Start() 
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Room name must be setted in inspector.");
        }
        else
        {
            RoomOptions roomOptions = new RoomOptions(){ IsVisible = true, IsOpen = true, MaxPlayers = (byte)4 };
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }
        
    #region Photon methods
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Connected to server");      
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed");
        Debug.Log("Failed to create room, tying again.");
        CreateRoom();
    }

    public override void OnJoinedRoom() 
    {
        Debug.Log("OnJoinedRoom");
        PhotonNetwork.LoadLevel(gameScene);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
    }
    #endregion
}
