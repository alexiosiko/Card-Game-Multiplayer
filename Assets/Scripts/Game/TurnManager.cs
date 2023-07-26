using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class TurnManager : NetworkBehaviour
{
	public NetworkVariable<int> nonReadyPlayersCount = new NetworkVariable<int>();
	public NetworkVariable<bool> everyoneIsReady = new NetworkVariable<bool>(true);
    public List<Hand> hands;
    public NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(-1);
    [ServerRpc(RequireOwnership = false)] public void StartPlayerServerRpc()
    {
        // Wait 3 seconds before we start turn
        // This is so if someone hasn't received cards yet
        // it doesnt give them WIN
        Invoke("StartPlayer", 3);
    }
	void Start()
	{
        hands = GetHands();

		if (!IsHost)
			return;

		nonReadyPlayersCount.OnValueChanged += OnNonReadyPlayersCount;
		currentPlayerIndex.OnValueChanged += OnCurrentPlayerIndexChanged;
		currentPlayerIndex.OnValueChanged += Bell.singleton.OnCurrentPlayerIndexChanged;

	}
	void OnCurrentPlayerIndexChanged(int prev, int current)
	{
		foreach (Hand hand in hands)
			hand.isTurn.Value = false;
		
		Players.Singleton.transform.GetChild(current).GetComponentInChildren<Hand>().isTurn.Value = true;
	}
	void OnNonReadyPlayersCount(int before, int after)
	{
		if (after <= 0)
		{
			everyoneIsReady.Value = true;
			foreach (Hand hand in hands)
				hand.SetLocalReadyClientRpc(true);
		}
		else
			everyoneIsReady.Value = false;
	}
    void StartPlayer()
    {
        currentPlayerIndex.Value = 1;
        currentPlayerIndex.Value = 0;
    }
    [ServerRpc(RequireOwnership = false)] public void NextPlayerServerRpc()
    {
		// Check if gameover
		if (IsGameOver())
		{
			GameOverUI.Singleton.GameOverScreen();
			return;
		}

		// Count the number of active players (players with cards in hand and not passed)
		int activePlayers = GetActivePlayers();

		// Find the next player with cards in hand and not passed
		int nextPlayerIndex = (currentPlayerIndex.Value + 1) % hands.Count;

		while (activePlayers > 1)
		{
			Hand nextPlayerHand = hands[nextPlayerIndex];

			// Check if the player has cards in hand and has not passed
			if (IsPlayerActive(nextPlayerHand))
			{
				// Update the current player index
				currentPlayerIndex.Value = nextPlayerIndex;
				return;
			}

			// Move on to the next player
			nextPlayerIndex = (nextPlayerIndex + 1) % hands.Count;
		}

		ClearPasses();
		ClearTable();
		
		// Update variable
		currentPlayerIndex.Value = nextPlayerIndex;
	}
	void ClearPasses()
	{
		// Reset passes
		foreach (Hand hand in hands)
			hand.GetComponent<Hand>().isPassed.Value = false;
	}
	bool IsPlayerActive(Hand hand)
	{
		if (hand.transform.childCount == 0 || hand.GetIsPassed() == true)
			return false;
		return true;
	}
	int GetActivePlayers()
	{
		int count = 0;
		foreach (Hand hand in hands)
			if (hand.transform.childCount != 0 && hand.isPassed.Value == false)
				count++;
		return count;
	}
	int GetPlayersPlayingCount()
	{
		return Players.Singleton.transform.childCount;
	}
	int GetPassedPlayersCount()
	{
		int count = 0;
		foreach (Hand hand in hands)
			if (hand.isPassed.Value == true)
				count++;
		return count;
	}
	void ClearTable()
	{
		// Clear recents
		Center.singleton.ClearTableClientRpc();
	}
    int GetPassedCount(List<Transform> hands)
    {
        int passedCount = 0;

        for (int i = 0; i < hands.Count; i++)
        {
            // Hand has no cards left
            if (hands[i].childCount == 0)
                passedCount++;

            // If hand is passed
            Hand h = hands[i].GetComponent<Hand>();
            if (h.GetIsPassed())
                passedCount++;
        }
        return passedCount;
    }
	bool IsGameOver()
	{
		int activePlayers = 0;
		foreach (Hand hand in hands)
			if (hand.transform.childCount != 0)
				activePlayers++;

		if (activePlayers <= 1)
			return true;

		return false;
	}
	[ServerRpc(RequireOwnership = false)] public void TellServerThatThisClientIsReadyServerRPC() 
	{
		nonReadyPlayersCount.Value--;

		Player[] players = FindObjectsOfType<Player>();
		foreach (Player p in players)
			p.CheckWin();
		
	}
    public List<Hand> GetHands()
    {
        List<Hand> handsList = new List<Hand>();
		foreach (Transform handTransform in Players.Singleton.transform)
            handsList.Add(handTransform.GetComponentInChildren<Hand>());
		
		return handsList;
    }
	[ServerRpc(RequireOwnership = false)] public void UnReadyPlayersServerRpc()
	{
		nonReadyPlayersCount.Value = hands.Count;
	}
    void Awake()
    {
        Singleton = this;
    }
    public static TurnManager Singleton;
	public override void OnDestroy()
	{
		nonReadyPlayersCount.OnValueChanged -= OnNonReadyPlayersCount;
		currentPlayerIndex.OnValueChanged -= OnCurrentPlayerIndexChanged;
		currentPlayerIndex.OnValueChanged -= Bell.singleton.OnCurrentPlayerIndexChanged;
	}
}
