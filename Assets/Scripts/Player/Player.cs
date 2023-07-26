using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class Player : NetworkBehaviour
{
	[SerializeField] TMP_Text nameText;
	public NetworkVariable<int> winPosition = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] Transform passTransform;
    void Update()
    {
        if (!IsOwner || !hand.isTurn.Value)
                return;
		// if (hand.isLocalReady == false)
		// 	return;

        if (Input.GetKeyDown(KeyCode.P))
			SetPassedServerRpc(true);
    }
	[ServerRpc(RequireOwnership = false)] void SetPassedServerRpc(bool value)
	{
		
		hand.isPassed.Value = value;
        TurnManager.Singleton.NextPlayerServerRpc();
	}
	public void CheckWin()
	{	
		if (hand.transform.childCount == 0) // Win
			SetWinPosition();
	}
	void SetWinPosition()
	{
		Player[] players = FindObjectsOfType<Player>();
		int nextWinPosition = 0;
		for (int i = 0; i < players.Length; i++)
		{
			Player player = players[i];
			if (player.winPosition.Value >= nextWinPosition)
				nextWinPosition = player.winPosition.Value + 1;
		}
		winPosition.Value = nextWinPosition;
	}
	public bool IsPlaying()
	{
		if (winPosition.Value == -1)
			return true;
		return false;
	}

	void SetPassLocal(bool value)
	{
		passTransform.gameObject.SetActive(value);
	}
    void Awake()
    {
        hand = GetComponentInChildren<Hand>();
    }
	public void OnIsPassedValueChanged(bool before, bool after)
	{
		if (after == true)
			SetPassLocal(true);
		else
			SetPassLocal(false);
	}
	void Start()
	{
		nameText.name = Steamworks.SteamClient.Name;
	}
    Hand hand;
}
