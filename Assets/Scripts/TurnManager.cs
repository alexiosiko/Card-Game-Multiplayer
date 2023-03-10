using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class TurnManager : NetworkBehaviour
{
    List<Transform> hands;
    int currentPlayerIndex = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            NextPlayerServerRpc();
        if (Input.GetKeyDown(KeyCode.S))
            StartPlayerServerRpc();
        
    }
    [ServerRpc(RequireOwnership = false)] public void StartPlayerServerRpc()
    {
        // Wait 3 seconds before we start turn
        // This is so if someone hasn't received cards yet
        // it doesnt give them WIN
        Invoke("StartPlayerClientRpc", 3);
    }
    [ClientRpc]
    void StartPlayerClientRpc()
    {
        GetHands();

        // Make all hands false
        for (int i = 0; i < hands.Count; i++)
            hands[i].GetComponent<Hand>().isTurn = false;

        // Make hand[0] true
        currentPlayerIndex = 0;
        hands[currentPlayerIndex].GetComponent<Hand>().isTurn = true;

        // Bell
        Bell.singleton.AdjustBell(currentPlayerIndex, hands);
    }
    [ServerRpc(RequireOwnership = false)] public void NextPlayerServerRpc()
    {
        NextPlayerClientRpc();
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
            if (h.GetPass())
                passedCount++;
        }
        return passedCount;
    }
    [ClientRpc] void NextPlayerClientRpc()
    {
        int passedCount = GetPassedCount(hands);

        // If there is one not passed or less
        // Ex:
        // 6 players
        // 5 passed -> true
        if (passedCount >= hands.Count - 1)
            Center.singleton.ClearTable();


        hands[currentPlayerIndex].GetComponent<Hand>().isTurn = false;
        
        // -- is clockwise, ++ is counter-clockwise
        currentPlayerIndex--;
        if (currentPlayerIndex < 0)
            currentPlayerIndex = hands.Count - 1;

        // Get currentIndex hand
        Hand hand = hands[currentPlayerIndex].GetComponent<Hand>();
        
        hand.isTurn = true;

        // If hand is empty, run this script again
        if (hand.transform.childCount <= 0)
            NextPlayerServerRpc();
        // Or if hand is passed
        else if (hand.GetPass() == true)
            NextPlayerServerRpc();
        
        // Bell
        Bell.singleton.AdjustBell(currentPlayerIndex, hands);

        // Finished
        if (hands.Count <= 1) {
            Debug.Log("Finished!");
            this.enabled = false;
        }
    }
    public void GetHands()
    {
        hands = new List<Transform>();
        foreach (Transform player in GameObject.Find("Players").transform)
            hands.Add(player.GetChild(0));
    }
    void Awake()
    {
        singleton = this;
    }
    public static TurnManager singleton;
}
