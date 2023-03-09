using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] Sprite back;
    [SerializeField] Sprite[] faces;
    public byte[] cardIndexs = new byte[] {
         0,  1,  2,  3,  4,  5,  6,  7,  8,  9,
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
        40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
        50, 51, 52, 53
    };
    public void InitializeGame()
    {
        AssignClientsToHands();
        Shuffle();
        StartCoroutine(Deal());
    }
    void AssignClientsToHands()
    {
        var clientList = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < hands.childCount; i++)
            SetHandOwnerId(i, clientList[i]);
    }
    void Shuffle()
    {
        int n = cardIndexs.Length;
        while (n > 1) 
        {
            int k = Random.Range(0, n--);
            byte temp = cardIndexs[n];
            cardIndexs[n] = cardIndexs[k];
            cardIndexs[k] = temp;
        }
    }
    public float dealDelay = 0.2f;
    IEnumerator Deal()
    {
        int handIndex = 0;
        for (int cardIndex = 0; cardIndex < cardIndexs.Length; cardIndex++)
        {
            print("index from server: " + cardIndex);
            DealCardsClientRpc(cardIndexs[cardIndex], handIndex);
            yield return new WaitForSeconds(0);

            // Remove cardIndex from list

            // Move to next player
            handIndex++;
            if (handIndex >= hands.childCount)
                handIndex = 0;
        }
    }
    [ClientRpc]
    public void DealCardsClientRpc(int cardIndex, int handIndex)
    {
        // Create card
        GameObject c = Instantiate<GameObject>(card, Vector3.zero, Quaternion.identity);

        // Set parent
        c.transform.parent = hands.GetChild(handIndex); 
        
        print("index from client: " + cardIndex);

        // Match card rotation to parent 
        c.transform.rotation = c.transform.parent.rotation;

        // Assign face                // For some reason, cardIndexs ranges from [1, 52] and NOT [0, 51] ... ? So cardIndexs[i] - 1
        c.GetComponent<Card>().face = faces[cardIndex];

        // Initialize card ... (parent, cardvalue)
        c.GetComponent<Card>().InitializeCard();

        // If client is owner of hand, then show face card
        if (hands.GetChild(handIndex).GetComponent<NetworkObject>().IsOwner)
            c.GetComponent<SpriteRenderer>().sprite = c.GetComponent<Card>().face;
        else
            c.GetComponent<SpriteRenderer>().sprite = back;
    
        // Tell hand to update their card's positions
        hands.GetChild(handIndex).GetComponent<Hand>().ResetCards();

        // c.transform.position = hands.GetChild(handIndex).transform.position;
    }
    void SetHandOwnerId(int i, ulong clientId)
    {
        print($"{hands.GetChild(i).name} assignes to clientId: {clientId}");
        NetworkObject netObj = hands.GetChild(i).GetComponent<NetworkObject>();
        if (netObj.OwnerClientId != clientId)
            netObj.ChangeOwnership(clientId);
    }
    void Awake()
    {
        singleton = this;
        hands = GameObject.Find("Hands").transform;
    }
    Transform hands;
    public static GameManager singleton;
}
