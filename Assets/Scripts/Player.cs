using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class Player : NetworkBehaviour
{
    [SerializeField] Transform passTransform;
    void Update()
    {
        if (!IsOwner || !hand.isTurn)
                return;
		if (hand.isLocalReady == false)
			return;

        if (Input.GetKeyDown(KeyCode.P))
        {
			SetPassLocal(true); // This is to set locally so show instant feedback for client 
			hand.SetLocalReady(false);
            SetPassServerRpc(true);
            TurnManager.singleton.NextPlayerServerRpc();
        }
    }
    [ServerRpc] public void SetPassServerRpc(bool value)
    {
		TurnManager.singleton.UnReadyPlayers();
        SetPassClientRpc(value);
    }
	void SetPassLocal(bool value)
	{
		passTransform.gameObject.SetActive(value);
	}
    [ClientRpc] public void SetPassClientRpc(bool value)
    {
        passTransform.gameObject.SetActive(value);
        hand.SetPass(value);
    }
    void Awake()
    {
        hand = GetComponentInChildren<Hand>();
    }
    Hand hand;
}
