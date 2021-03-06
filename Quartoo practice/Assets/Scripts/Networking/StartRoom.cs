﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class StartRoom : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    #region Variables

    public static StartRoom room;

    public GameObject CreateGameButton;
    public GameObject JoinGameButton;
    public GameObject CreateOrJoinBackButton;
    public GameObject RoomLobbyBackButton;
    public GameObject WaitingLoadingBackButton;
    public GameObject FindGamesButton;
    public GameObject roomListingPrefab;
    public GameObject roomListingPanel;
    public GameObject StartButton;

    public Canvas CreateOrJoinCanvas;
    public Canvas RoomLobbyCanvas;
    public Canvas WaitingLoadingCanvas;
    public Canvas LoadingCanvas;

    public Text StatusText;
    
    public Transform roomsPanel;

    public string roomName;

    #endregion

    #region AwakeStartUpdate

    private void Awake()
    {
        room = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        else
            PhotonNetwork.ConnectUsingSettings();   // -> OnConnectedToMaster
    }

    #endregion

    #region PunCallbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster succesfully entered");

        CreateOrJoinCanvas.gameObject.SetActive(true);        

        PhotonNetwork.JoinLobby();  // -> OnJoinedLobby
    }

    public override void OnJoinedLobby()
    {
        CreateGameButton.SetActive(true);
        JoinGameButton.SetActive(true);
        CreateOrJoinBackButton.SetActive(true);
        LoadingCanvas.gameObject.SetActive(false);
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;    // -> OnJoinedRoom
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        CreateOrJoinCanvas.gameObject.SetActive(false);
        RoomLobbyCanvas.gameObject.SetActive(false);

        WaitingLoadingCanvas.gameObject.SetActive(true);
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StatusText.text = "Connected to room, waiting for host to start game...";
        }

        WaitingLoadingBackButton.gameObject.SetActive(true);
        StartButton.gameObject.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected function succesfully entered");
        base.OnDisconnected(cause);
        //Thread.Sleep(3000);

        Debug.Log("PhotonNetwork.ConnectUsingSettings called");

        PhotonNetwork.ConnectUsingSettings();
    }    

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        RemoveRoomListings();

        foreach (RoomInfo room in roomList)
        {
            ListRoom(room);
        }
    }   

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                StatusText.text = "Player joined, ready to Start Game...";
                StartButton.SetActive(true);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                StatusText.text = "Host has left, press Back to leave...";
            }
            else
            {
                StatusText.text = "Player left, waiting for new player to join...";
                StartButton.SetActive(false);
                PhotonNetwork.CurrentRoom.IsOpen = true;
                PhotonNetwork.CurrentRoom.IsVisible = true;
            }
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left room.");

        WaitingLoadingCanvas.gameObject.SetActive(false);
        CreateOrJoinCanvas.gameObject.SetActive(true);

        CreateGameButton.SetActive(false);
        JoinGameButton.SetActive(false);
        CreateOrJoinBackButton.SetActive(false);
        LoadingCanvas.gameObject.SetActive(true);

        Debug.Log("PhotonNetwork.Disconnect() called");

        PhotonNetwork.Disconnect();        
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }    

    #endregion

    #region Functions

    public void CreateRoom()
    {
        RoomOptions roomOps = new RoomOptions()
        {
            EmptyRoomTtl = 1,
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2
        };

        roomName = GameInfo.username;
        PhotonNetwork.CreateRoom(roomName, roomOps);    // -> OnCreatedRoom / OnCreateRoomFailed
        
    }

    public void RemoveRoomListings()
    {
        while (roomsPanel.childCount != 0)
        {
            Destroy(roomsPanel.GetChild(0).gameObject);
        }
    }

    public void ListRoom(RoomInfo room)
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject tempListing = Instantiate(roomListingPrefab, roomsPanel);
            RoomButton tempButton = tempListing.GetComponent<RoomButton>();
            tempButton.roomName = room.Name;
            tempButton.SetRoom();
        }
    }


    #endregion

    #region Buttons

    public void OnCreateGameButtonClicked()
    {
        CreateRoom();

        GameInfo.selectPieceAtStart = 1;
    }

    public void OnJoinGameButtonClicked()
    {
        CreateOrJoinCanvas.gameObject.SetActive(false);
        RoomLobbyCanvas.gameObject.SetActive(true);

        FindGamesButton.gameObject.SetActive(true);
        roomListingPanel.gameObject.SetActive(true);
  
        GameInfo.selectPieceAtStart = 2;
    }

    public void OnCreateOrJoinBackButtonClicked()
    {
        PhotonNetwork.Disconnect();
        Initiate.Fade("MainMenu", Color.black, 4.0f);
    }

    public void OnRoomLobbyBackButtonClicked()
    {    
        RoomLobbyCanvas.gameObject.SetActive(false);
        CreateOrJoinCanvas.gameObject.SetActive(true);

        CreateGameButton.SetActive(true);
        JoinGameButton.SetActive(true);
        CreateOrJoinBackButton.SetActive(true);
        
        // don't need to dc here
    }

    public void OnWaitingLoadingBackButtonClicked()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LeaveRoom();  // -> OnLeftRoom  

            Debug.Log("PhotonNetwork.LeaveRoom() called");

            //WaitingLoadingCanvas.gameObject.SetActive(false);
            //CreateOrJoinCanvas.gameObject.SetActive(true);

            //CreateGameButton.SetActive(false);
            //JoinGameButton.SetActive(false);
            //CreateOrJoinBackButton.SetActive(false);

            //LoadingCanvas.gameObject.SetActive(true);

            //PhotonNetwork.Disconnect();

            //PhotonNetwork.ConnectUsingSettings();


        }
        else
        {
            PhotonNetwork.LeaveRoom();  // -> OnLeftRoom    

            //WaitingLoadingCanvas.gameObject.SetActive(false);
            //CreateOrJoinCanvas.gameObject.SetActive(true);

            //CreateGameButton.SetActive(false);
            //JoinGameButton.SetActive(false);
            //CreateOrJoinBackButton.SetActive(false);

            //LoadingCanvas.gameObject.SetActive(true);

            //CreateGameButton.SetActive(true);
            //JoinGameButton.SetActive(true);
            //CreateOrJoinBackButton.SetActive(true);

            //LoadingCanvas.gameObject.SetActive(false);

            //PhotonNetwork.Disconnect();

            //PhotonNetwork.ConnectUsingSettings();

            
        }
    }

    public void OnStartButtonClicked()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    #endregion
}
