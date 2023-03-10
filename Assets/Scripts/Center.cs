using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
public class Center : NetworkBehaviour
{
    public List<Transform> recentCards = new List<Transform>();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            ClearTableServerRpc();
    }
    [ServerRpc(RequireOwnership = false)] public void ClearRecentsServerRpc()
    {
        ClearRecentsClientRpc();
    }
    [ClientRpc] void ClearRecentsClientRpc()
    {
        print("clearing table");
        recentCards.Clear();
    }
    [ServerRpc(RequireOwnership = false)] void ClearTableServerRpc()
    {
        ClearTableClientRpc();
    }
    [ClientRpc] void ClearTableClientRpc()
    {
        ClearTable();
    }
    public void ClearTable()
    {

        // Deep copy
        List<Transform> cardTransforms = new List<Transform>();
        foreach (Transform child in transform)
            cardTransforms.Add(child);
        
        foreach (Transform t in cardTransforms)
        {
            t.parent = sideCardsTransform;
            t.DOLocalMove(Vector3.zero, 0.2f);
        }

        GameState.cardState = GameState.CardState.Empty;
        recentCards.Clear();
        ResetPasses();
    }
    void ResetPasses()
    {
        foreach (Transform player in players)
            player.GetComponent<Player>().SetPassClientRpc(false);
    }



    public void OrderCards()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = i - 60;
    }
    void Awake()
    {
        singleton = this;
        sideCardsTransform = GameObject.Find("SideCards").transform;
        players = GameObject.Find("Players").transform;
    }
    Transform players;
    Transform sideCardsTransform;
    public static Center singleton;
}
