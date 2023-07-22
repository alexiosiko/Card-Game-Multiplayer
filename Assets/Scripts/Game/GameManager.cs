using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject player;
    void Start()
    {
        if (!IsHost)
            return;

        StartGame();
    }
	void Update()
	{
		if (!IsHost)
			return;
		if (Input.GetKeyDown(KeyCode.A))
			AssignSeats();
	}
	public void RestartGame()
	{
		StopAllTweensClientRpc();
		RemoveAllCardsClientRpc();
		AssignSeats();
		ResetWinPositions();
		Deck.Singleton.ShuffleAndDeal();
		Center.singleton.ClearTableClientRpc();
		TurnManager.singleton.StartPlayerServerRpc();
	}
	void AssignSeats()
	{
		Player[] players = FindObjectsOfType<Player>();
		foreach (Player player in players)
			player.GetComponent<NetworkObject>().TryRemoveParent();

		for (int i = 0; i < players.Length; i++)
			foreach (Player player in players)
				if (player.winPosition.Value == i)
					player.GetComponent<NetworkObject>().TrySetParent(playersTransform);
	
		// If i missed anyone, just add them...
		foreach (Player player in players)
			if (player.transform.parent == null)
				player.GetComponent<NetworkObject>().TrySetParent(playersTransform);
	}
	void ResetWinPositions()
	{
		Player[] players = FindObjectsOfType<Player>();
		foreach (Player player in players)
			player.winPosition.Value = -1;
	}
	[ClientRpc] void StopAllTweensClientRpc()
	{
		DOTween.KillAll();
	}
	[ClientRpc] void RemoveAllCardsClientRpc()
	{
		Card[] cards = FindObjectsOfType<Card>();
		foreach (Card card in cards)
			Destroy(card.gameObject);
	}
    void StartGame()
    {
        CreatePlayersAndAssignClients();
        AdjustHandsClientRpc();
        AdjustCameraToPlayerClientRpc();
        
		Deck.Singleton.ShuffleAndDeal();
        TurnManager.singleton.StartPlayerServerRpc();
    }
    void CreatePlayersAndAssignClients()
    {
        var clientList = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < clientList.Count; i++)
        {
            CreateHands();
            SetHandOwnerId(i, clientList[i]);
        }
    }
    void CreateHands()
    {
        GameObject p = Instantiate<GameObject>(player, Vector3.zero, Quaternion.identity);
        p.GetComponentInChildren<NetworkObject>().Spawn();
        p.GetComponentInChildren<NetworkObject>().TrySetParent(playersTransform);
    }
    [ClientRpc] void AdjustHandsClientRpc() {
        float angle = 0;
        for (int i = 0; i < playersTransform.childCount; i++) {
            playersTransform.GetChild(i).position = new Vector3(3 * Mathf.Cos(angle), 3 * Mathf.Sin(angle), 0);
            playersTransform.GetChild(i).eulerAngles = new Vector3(0, 0, angle * 180/Mathf.PI + 90);
            angle += 2 * Mathf.PI / playersTransform.childCount;
        }
    }
    [ClientRpc] void AdjustCameraToPlayerClientRpc()
    {
        // Rotate camera to their hand
        Hand[] hs = FindObjectsOfType<Hand>();
        foreach (Hand h in hs)
            h.RotateCameraToHand();

        // Rotate the bell to their hand
        Bell.singleton.RotateBellToHand();
    }
    public void ResetAllCards()
    {
        foreach (Transform player in GameObject.Find("Players").transform)
            player.GetChild(0).GetComponent<Hand>().ResetCards();
    }
    void SetHandOwnerId(int i, ulong clientId)
    {
        // print($"{hands.GetChild(i).name} assignes to clientId: {clientId}");
        NetworkObject netObj = playersTransform.GetChild(i).GetComponent<NetworkObject>();
        if (netObj.OwnerClientId != clientId)
            netObj.ChangeOwnership(clientId);
    }
    void Awake()
    {
        singleton = this;
        playersTransform = GameObject.Find("Players").transform;
    }
    Transform playersTransform;
    public static GameManager singleton;
}
