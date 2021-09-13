using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public enum SceneName
{
    Menu,
    Game
}

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen, lobbyScreen;

    [Header("Main Screen")]
    public Button btn_createRoom, btn_joinRoom;

    [Header("Lobby Screen")]
    public TextMeshProUGUI txt_playerList;
    public Button btn_startGame;

    private void Start()
    {
        btn_createRoom.interactable = false;
        btn_joinRoom.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        btn_createRoom.interactable = true;
        btn_joinRoom.interactable = false;
    }

    public void ActivateScreen(GameObject screen)
    {
        // Deactivate all screens first
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        // Then activate the one we want
        screen.SetActive(true);
    }

    public void OnCreateRoomClicked(TMP_InputField roomNameInput)
    {
        NetworkManager.Instance.CreateRoom(roomNameInput.text);
    }
    
    public void OnJoinRoomClicked(TMP_InputField roomNameInput)
    {
        NetworkManager.Instance.JoinRoom(roomNameInput.text);
    }

    public void OnRoomNameUpdate(TMP_InputField roomNameInput)
    {
        if(NetworkManager.Instance.RoomExists(roomNameInput.text))
            btn_joinRoom.interactable = true;
    }

    public void OnPlayerNameUpdate(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnJoinedRoom()
    {
        ActivateScreen(lobbyScreen);

        // Should inform everyone when joining a room with a RemoteProcedureCall
        photonView.RPC(nameof(UpdateLobbyUI), RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();

        // No need for an RPC. This method is called for all clients in the room
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        txt_playerList.text = "";

        // Display all the players currently in the lobby
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            txt_playerList.text += player.NickName + "\n";
        }

        // Only the host can start the game!
        btn_startGame.interactable = PhotonNetwork.IsMasterClient;
    }

    public void OnLeaveLobbyClicked()
    {
        NetworkManager.Instance.LeaveRoom();
        ActivateScreen(mainScreen);
    }

    public void OnStartGameClicked()
    {
        NetworkManager.Instance.photonView.RPC(nameof(NetworkManager.ChangeScene), RpcTarget.All, SceneName.Game.ToString());
    }
}
