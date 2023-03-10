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

        if (Input.GetKeyDown(KeyCode.P))
        {
            SetPassServerRpc(true);
            TurnManager.singleton.NextPlayerServerRpc();
        }
    }
    [ServerRpc] public void SetPassServerRpc(bool value)
    {
        SetPassClientRpc(value);
    }

    [ClientRpc] public void SetPassClientRpc(bool value)
    {
        print(value);
        passTransform.gameObject.SetActive(value);
        hand.SetPass(value);
    }
    void Awake()
    {
        hand = GetComponentInChildren<Hand>();
    }
    Hand hand;
}
