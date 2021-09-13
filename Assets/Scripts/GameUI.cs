using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Linq;

public class GameUI : MonoBehaviour
{    
    public TextMeshProUGUI winText;
    public GameObject playerStats;
    public Transform playerList;

    public static GameUI Instance;

    private List<PlayerUIStats> gUIContainers = new List<PlayerUIStats>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {        
        InitializePlayerUI();
    }

    private void Update()
    {
        UpdatePlayerUI();
    }

    void InitializePlayerUI()
    {
        foreach (PlayerController player in GameManager.Instance.players)
        {
            if (player != null)
            {
                PlayerUIStats newStats = Instantiate(playerStats, playerList).GetComponent<PlayerUIStats>();
                newStats.obj.SetActive(true);
                newStats.ID = player.id;
                newStats.txtName.text = player.photonPlayer.NickName;
                newStats.hatTimeSlider.maxValue = GameManager.Instance.timeToWin;

                gUIContainers.Add(newStats);
            }
        }
    }

    void UpdatePlayerUI()
    {
        foreach (PlayerUIStats playerSO in gUIContainers)
        {
            if(playerSO != null) playerSO.hatTimeSlider.value = GameManager.Instance.players.FirstOrDefault( pl => pl.id == playerSO.ID).curHatTime;
        }
    }

    public void SetWinText(string winnerName)
    {
        winText.gameObject.SetActive(true);
        winText.text = $"{winnerName} won the game!";        
    }
}
