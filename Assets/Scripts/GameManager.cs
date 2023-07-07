using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] Sprite[] faces;
    [SerializeField] GameObject player;
    public byte[] cardIndexs = new byte[] {
         0,  1,  2,  3,  4,  5,  6,  7,  8,  9,
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
        40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
        50, 51, 52, 53
    };
    void Start()
    {
        if (!IsHost)
            return;

        StartGame();
    }
    void StartGame()
    {
        CreatePlayersAndAssignClients();
        AdjustHandsClientRpc();
        AdjustCameraToPlayerClientRpc();
        
        Shuffle();
        StartCoroutine(Deal());
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
        p.GetComponentInChildren<NetworkObject>().TrySetParent(players);
    }
    [ClientRpc]
    void AdjustHandsClientRpc() {
        float angle = 0;
        for (int i = 0; i < players.childCount; i++) {
            players.GetChild(i).position = new Vector3(3 * Mathf.Cos(angle), 3 * Mathf.Sin(angle), 0);
            players.GetChild(i).eulerAngles = new Vector3(0, 0, angle * 180/Mathf.PI + 90);
            angle += 2 * Mathf.PI / players.childCount;
        }
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
    float dealDelay = 0.15f;
    IEnumerator Deal()
    {
        int handIndex = 0;
        for (int cardIndex = 0; cardIndex < cardIndexs.Length; cardIndex++)
        {
            // print("index from server: " + cardIndex);
            DealCardsClientRpc(cardIndexs[cardIndex], handIndex);
            yield return new WaitForSeconds(dealDelay);

            // Remove cardIndex from list

            // Move to next player
            handIndex++;
            if (handIndex >= players.childCount)
                handIndex = 0;
        }
    }
    [ServerRpc(RequireOwnership = false)] public void MoveCardsToCenterServerRpc(string cardId)
    {
        MoveCardsToCenterClientRpc(cardId);  
    }
	public void MoveCardsToCenterLocal(string cardId)
	{
		Transform card = FindCard(cardId);

        if (card == null)
        {
            print($"Could not find cardId: {cardId} on clientId: {NetworkManager.Singleton.LocalClientId}");
            return;
        }

        SpriteRenderer spriteRenderer = card.GetComponent<SpriteRenderer>();
        
        // Remove highlight from card and then hand.highlights
        card.GetComponent<Card>().RemoveHighlight();
        
        // Disable collider
        card.GetComponent<BoxCollider2D>().enabled = false;

        // Change card
        spriteRenderer.sprite = card.GetComponent<Card>().face;

        // Set parent to center
        card.parent = Center.singleton.transform;

        // Get angle depending on Hand rotation
        // float x = transform.parent.eulerAngles.z;
        // x = Mathf.Cos(x * Mathf.PI/180); 
        // float y = transform.parent.eulerAngles.z;
        // y = Mathf.Sin(y * Mathf.PI/180);
        
        // Animations
        card.DOKill();
        card.DOMove(Vector3.zero, 0.2f);
        // card.DOLocalMove(new Vector3(x * spacing * (cardCount - highlightedCount / 2), y * spacing * (cardCount - highlightedCount / 2), 0), 0.2f);

        // Get random rotation
        // card.transform.localRotation = Quaternion.Euler(0, 0,  Random.Range(-20, 20));
        
        // Adjust sort layer
        if (Center.singleton.recentCards.Count == 0)
            spriteRenderer.sortingOrder = 0;
        else
            spriteRenderer.sortingOrder = Center.singleton.recentCards[Center.singleton.recentCards.Count - 1].GetComponent<SpriteRenderer>().sortingOrder + 1;

        // Adjust ALL cards
        // GameManager.singleton.ResetAllCards();

        // Center.singleton.recentCards.Add(card);
        // Center.singleton.OrderCards();
	}

    [ClientRpc] void MoveCardsToCenterClientRpc(string cardId)
    {
        Transform card = FindCard(cardId);

        if (card == null)
        {
            print($"Could not find cardId: {cardId} on clientId: {NetworkManager.Singleton.LocalClientId}");
            return;
        }

        SpriteRenderer spriteRenderer = card.GetComponent<SpriteRenderer>();
        
        // Remove highlight from card and then hand.highlights
        card.GetComponent<Card>().RemoveHighlight();
        
        // Disable collider
        card.GetComponent<BoxCollider2D>().enabled = false;

        // Change card
        spriteRenderer.sprite = card.GetComponent<Card>().face;

        // Set parent to center
        card.parent = Center.singleton.transform;
        card.GetComponent<Card>().RemoveHand();

        // Get angle depending on Hand rotation
        // float x = transform.parent.eulerAngles.z;
        // x = Mathf.Cos(x * Mathf.PI/180); 
        // float y = transform.parent.eulerAngles.z;
        // y = Mathf.Sin(y * Mathf.PI/180);
        
        // Animations
        card.DOKill();
        card.DOMove(Vector3.zero, 0.2f);
        // card.DOLocalMove(new Vector3(x * spacing * (cardCount - highlightedCount / 2), y * spacing * (cardCount - highlightedCount / 2), 0), 0.2f);

        // Get random rotation
        card.transform.localRotation = Quaternion.Euler(0, 0,  Random.Range(-20, 20));
        
        // Adjust sort layer
        if (Center.singleton.recentCards.Count == 0)
            spriteRenderer.sortingOrder = 0;
        else
            spriteRenderer.sortingOrder = Center.singleton.recentCards[Center.singleton.recentCards.Count - 1].GetComponent<SpriteRenderer>().sortingOrder + 1;

        // Adjust ALL cards
        GameManager.singleton.ResetAllCards();

        Center.singleton.recentCards.Add(card);
        Center.singleton.OrderCards();
    }

    Transform FindCard(string cardId)
    {
        // Find card
        Card[] cards = FindObjectsOfType<Card>();

        foreach (Card c in cards)
            if (c.cardId == cardId)
                return c.transform;

        return null;
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
    [ClientRpc]
    public void DealCardsClientRpc(int cardIndex, int handIndex)
    {
        // Create card
        GameObject c = Instantiate<GameObject>(card, Vector3.zero, Quaternion.identity);

        // Set parent
        c.transform.parent = players.GetChild(handIndex).GetChild(0); 
        
        // Match card rotation to player 
        c.transform.rotation = c.transform.parent.parent.rotation;

        // Assign face                // For some reason, cardIndexs ranges from [1, 52] and NOT [0, 51] ... ? So cardIndexs[i] - 1
        c.GetComponent<Card>().face = faces[cardIndex];

        // Initialize card ... (parent, cardvalue)
        c.GetComponent<Card>().InitializeCard();

        // If client is owner of hand, then show face card
        if (players.GetChild(handIndex).GetComponent<NetworkObject>().IsOwner)
            c.GetComponent<SpriteRenderer>().sprite = c.GetComponent<Card>().face;
    
        // Tell hand to update their card's positions
        players.GetChild(handIndex).GetComponentInChildren<Hand>().ResetCards();
    }
    void SetHandOwnerId(int i, ulong clientId)
    {
        // print($"{hands.GetChild(i).name} assignes to clientId: {clientId}");
        NetworkObject netObj = players.GetChild(i).GetComponent<NetworkObject>();
        if (netObj.OwnerClientId != clientId)
            netObj.ChangeOwnership(clientId);
    }
    void Awake()
    {
        singleton = this;
        players = GameObject.Find("Players").transform;
    }
    Transform players;
    public static GameManager singleton;
}
