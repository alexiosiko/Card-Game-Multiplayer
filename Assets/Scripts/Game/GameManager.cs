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
		AdjustHandsAndCameraToPlayerClientRpc();
		ResetWinPositions();
		ResetAllHandsClientRpc();
		Deck.Singleton.ShuffleAndDeal();
		Center.singleton.ClearTableClientRpc();
		TurnManager.Singleton.StartPlayerServerRpc();
	}
	[ClientRpc]	void ResetAllHandsClientRpc()
	{
		Hand[] hands = FindObjectsOfType<Hand>();
		foreach (Hand hand in hands)
			hand.highlighted.Clear();
	}
	void AssignSeats()
	{
		Player[] players = FindObjectsOfType<Player>();
		foreach (Player player in players)
			player.GetComponent<NetworkObject>().TryRemoveParent();

		for (int i = 0; i < players.Length; i++)
			foreach (Player player in players)
				if (player.winPosition.Value == i)
					player.GetComponent<NetworkObject>().TrySetParent(Players.Singleton.transform);
	
		// If i missed anyone, just add them...
		foreach (Player player in players)
			if (player.transform.parent == null)
				player.GetComponent<NetworkObject>().TrySetParent(Players.Singleton.transform);
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
        AdjustHandsAndCameraToPlayerClientRpc();
        
		Deck.Singleton.ShuffleAndDeal();
        TurnManager.Singleton.StartPlayerServerRpc();
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
        p.GetComponentInChildren<NetworkObject>().TrySetParent(Players.Singleton.transform);
    }
    [ClientRpc] void AdjustHandsAndCameraToPlayerClientRpc() {
        float angle = 0;
        for (int i = 0; i < Players.Singleton.transform.childCount; i++) {
            Players.Singleton.transform.GetChild(i).DOMove(new Vector3(3 * Mathf.Cos(angle), 3 * Mathf.Sin(angle), 0), 1f);
            Players.Singleton.transform.GetChild(i).DORotate(new Vector3(0, 0, angle * 180/Mathf.PI + 90), 1f);
            angle += 2 * Mathf.PI / Players.Singleton.transform.childCount;
        }
		Invoke("AdjustCameraToPlayer", 2f); // Wait for AdjustHandsClientRpc() to finish animation
    }
    void AdjustCameraToPlayer()
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
        foreach (Transform player in Players.Singleton.transform)
            player.GetChild(0).GetComponent<Hand>().ResetCards();
    }
    void SetHandOwnerId(int i, ulong clientId)
    {
        // print($"{hands.GetChild(i).name} assignes to clientId: {clientId}");
        NetworkObject netObj = Players.Singleton.transform.GetChild(i).GetComponent<NetworkObject>();
        if (netObj.OwnerClientId != clientId)
            netObj.ChangeOwnership(clientId);
    }
    void Awake()
    {
        singleton = this;
    }
    public static GameManager singleton;
}
