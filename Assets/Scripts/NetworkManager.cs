using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    private List<string> cachedRoomList = new List<string>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(gameObject);
        else if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();    
    }
  
    public override void OnCreatedRoom()
    {
        Debug.Log($"Created room {PhotonNetwork.CurrentRoom.Name}");
        photonView.RPC(nameof(AddRoomToList), RpcTarget.AllBufferedViaServer, PhotonNetwork.CurrentRoom.Name);        
    }

    [PunRPC]
    public void AddRoomToList(string roomName)
    {
        Debug.Log($"Adding room {roomName} to list");
        cachedRoomList.Add(roomName);
    }

    [PunRPC]
    public void RemoveRoomFromList(string roomName)
    {
        Debug.Log($"Removing room {roomName}");
        cachedRoomList.Remove(roomName);
    }

    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }
    
    public bool RoomExists(string roomName)
    {

        return cachedRoomList.Contains(roomName);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    
    public void LeaveRoom()
    {
        Debug.Log($"Leaving room {PhotonNetwork.CurrentRoom.Name}, with players {PhotonNetwork.CountOfPlayersInRooms}");
        if (PhotonNetwork.CountOfPlayersInRooms <= 1)
            photonView.RPC(nameof(RemoveRoomFromList), RpcTarget.AllBufferedViaServer, PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LeaveRoom();
    }    
    
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
