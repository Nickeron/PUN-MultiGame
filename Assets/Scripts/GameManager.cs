using UnityEngine;

using System.Linq;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields
    [Header("Stats")]
    public bool gameEnded = false;
    public float timeToWin;
    public float invincibleDuration;

    [Header("Players")]    
    public Transform spawnGroup;
    public PlayerController[] players;
    public int playerWithHat;

    static public GameManager Instance;

    #endregion

    #region Private Fields

    private GameObject _instance;

    [Tooltip("The prefab to use for representing the player")]
    [SerializeField]
    private GameObject _playerPrefab;
    private Vector3[] _spawnPoints;
    private int _playersInGame;
    private float _hatPickupTime;
    private const string playerPrefabLocation = "Player";
    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _spawnPoints = spawnGroup.GetComponentsInChildren<Transform>().Select(t => t.position).ToArray();
    }

    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC(nameof(ImInGame), RpcTarget.AllBuffered);
    }

    void Update()
    {
        // "back" button of phone equals "Escape". quit app if that's pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }
    }

    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    #endregion

    #region Private Methods
    [PunRPC]
    void ImInGame()
    {
        _playersInGame++;
        if (_playersInGame == PhotonNetwork.PlayerList.Length) SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Spawn the player object. Get the PlayerController. Initialize it.
        PhotonNetwork.Instantiate(playerPrefabLocation, _spawnPoints[Random.Range(0, _spawnPoints.Length)], Quaternion.identity)
                     .GetComponent<PlayerController>()
                     .photonView.RPC(nameof(PlayerController.Initialize), RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerID)
    {
        return players.FirstOrDefault(x => x.id == playerID);
    }
    
    public PlayerController GetPlayer(GameObject playerObj)
    {
        return players.FirstOrDefault(x => x.gameObject == playerObj);
    }

    // When a player tags another and takes the hat
    [PunRPC]
    public void TakeHat(int playerID, bool initialGive)
    {
        // Remove hat from current player
        if (!initialGive) GetPlayer(playerWithHat)?.SetHat(false);

        // Give hat to new player
        playerWithHat = playerID;
        GetPlayer(playerID)?.SetHat(true);

        _hatPickupTime = Time.time;
    }

    public bool CanGetHat()
    {
        return Time.time > _hatPickupTime + invincibleDuration;
    }

    [PunRPC]
    internal void WinGame(int playerID)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerID);
        GameUI.Instance.SetWinText(player.photonPlayer.NickName);

        // Go back to menu after 3 seconds of winning the game
        Invoke(nameof(GoBackToMenu), 3.0f);
    }

    void GoBackToMenu()
    {
        NetworkManager.Instance.LeaveRoom();
        NetworkManager.Instance.ChangeScene(SceneName.Menu.ToString());
        Destroy(NetworkManager.Instance.gameObject);
    }
    #endregion

}