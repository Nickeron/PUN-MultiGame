using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Player photonPlayer;

    Rigidbody body;
    const string HORIZONTAL = "Horizontal", VERICAL = "Vertical";

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckWinConditions();

        if (!photonView.IsMine) return;

        Move();
        TryJump();
        TrackHatPossessionTime();
    }

    private void TrackHatPossessionTime()
    {
        if(hatObject.activeInHierarchy) curHatTime += Time.deltaTime;
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.Instance.players[id - 1] = this;

        // Shouldn't control other players
        if (!photonView.IsMine) body.isKinematic = true;

        // First player takes the hat
        if (id == 1) GameManager.Instance.TakeHat(id, true);
    }

    void CheckWinConditions()
    {
        // Only MasterClient will do this check
        if (!PhotonNetwork.IsMasterClient) return;

        // Possess the hat for the required time and the game has not ended
        if (curHatTime < GameManager.Instance.timeToWin || GameManager.Instance.gameEnded) return;

        GameManager.Instance.gameEnded = true;
        GameManager.Instance.photonView.RPC(nameof(GameManager.WinGame), RpcTarget.All, id);
    }

    void Move()
    {
        float x = Input.GetAxis(HORIZONTAL) * moveSpeed;
        float z = Input.GetAxis(VERICAL) * moveSpeed;

        body.velocity = new Vector3(x, body.velocity.y, z);
    }

    void TryJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 0.7f))
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        // Is it a player?
        if (!collision.gameObject.CompareTag("Player")) return;

        // Does it have a hat?
        if (GameManager.Instance.GetPlayer(collision.gameObject)?.id != GameManager.Instance.playerWithHat) return;

        // Can it give the hat?
        if (GameManager.Instance.CanGetHat())
            GameManager.Instance.photonView.RPC(nameof(GameManager.TakeHat), RpcTarget.All, id, false);

    }

    internal void SetHat(bool wearsHat)
    {
        hatObject.SetActive(wearsHat);
    }

    // Use a stream to send back and forth curHatTime - IPunObservable
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting) stream.SendNext(curHatTime);
        else if (stream.IsReading) curHatTime = (float) stream.ReceiveNext();
    }
}
