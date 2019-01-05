using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class connectToServer : Photon.PunBehaviour
{
    string _gameVersion = "1";
    public PhotonLogLevel loglevel = PhotonLogLevel.ErrorsOnly;
    public byte maxPlayersPerRoom = 4;

    bool isConnecting;

    public GameObject canvas;

    private string playerName;

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.logLevel = loglevel;
    }

    void Start()
    {
        Connect();
    }

    public void Connect()
    {
        isConnecting = true;

        if (PhotonNetwork.connected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
            PhotonNetwork.autoCleanUpPlayerObjects = false;
        }
    }

    public override void OnConnectedToMaster()
    {
        print("connected to master");
        if (isConnecting)
        {
            PhotonNetwork.JoinOrCreateRoom("whiteboardVR", new RoomOptions(), TypedLobby.Default);
        }
    }

    public override void OnDisconnectedFromPhoton()
    {
        print("No internet connection");
    }

    public override void OnJoinedRoom()
    {
        //if you are 1st
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel("main");
            canvas.SetActive(false);
        }
    }
}