using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
public class Center : NetworkBehaviour
{
    public List<Transform> recentCards = new List<Transform>();
    [ClientRpc] public void ClearTableClientRpc()
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
    }
	[ServerRpc(RequireOwnership = false)] public void MoveCardsToCenterServerRpc(string cardId)
    {
        MoveCardsToCenterClientRpc(cardId);  
    }
	Transform MoveCard(string cardId)
	{
		Transform card = FindCard(cardId);

        if (card == null)
        {
            print($"Could not find cardId: {cardId} on clientId: {NetworkManager.Singleton.LocalClientId}");
            return null;
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

		return card;
	}
	[ClientRpc] void MoveCardsToCenterClientRpc(string cardId)
    {
		Transform card = MoveCard(cardId);
		if (card == null) return;
        
        // Adjust ALL cards
        GameManager.Singleton.ResetAllCards();

		// Add card to center
        Center.singleton.recentCards.Add(card);
        Center.singleton.OrderCards();

		// Tell server that this client is ready
		TurnManager.Singleton.TellServerThatThisClientIsReadyServerRPC();
    }
	public void MoveCardsToCenter(List<Transform> highlighted)
    {
        // Deep copy this highlight
        List<Transform> storedHighlighted = new List<Transform>();
        foreach (Transform t in highlighted)
            storedHighlighted.Add(t);
		
		// ONLY move card to center on this client
		// foreach (Transform card in storedHighlighted)
        //     GameManager.singleton.MoveCardsToCenterLocal(card.GetComponent<Card>().cardId);
		// GameManager.singleton.testClientRpc("ah");

        foreach (Transform card in storedHighlighted) 
            Center.singleton.MoveCardsToCenterServerRpc(card.GetComponent<Card>().cardId);


		// Next player turn
        TurnManager.Singleton.NextPlayerServerRpc();
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
	Transform FindCard(string cardId)
    {
        // Find card
        Card[] cards = FindObjectsOfType<Card>();

        foreach (Card c in cards)
            if (c.cardId == cardId)
                return c.transform;

        return null;
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
    }
    Transform sideCardsTransform;
    public static Center singleton;
}
