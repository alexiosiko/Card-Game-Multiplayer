using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class Hand : NetworkBehaviour
{
    public List<Transform> highlighted;
    [SerializeField] Color defaultColor = new Color(1,1,1,1);
    public float spacing = 1.2f;
    public void MoveCardsToCenter()
    {
        // Clear center recents
        Center.singleton.ClearRecentsServerRpc();

        // Deep copy this highlight
        List<Transform> storedHighlighted = new List<Transform>();
        foreach (Transform t in highlighted)
            storedHighlighted.Add(t);


        foreach (Transform card in storedHighlighted)
            MoveCardsToCenterServerRpc(card.GetComponent<Card>().cardId);
    }
    [ServerRpc(RequireOwnership = false)] public void MoveCardsToCenterServerRpc(string cardId)
    {
        MoveCardsToCenterClientRpc(cardId);  
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
        ResetAllCards();

        Center.singleton.recentCards.Add(card);
        Center.singleton.OrderCards();
    }
    public void ResetAllCards()
    {
        foreach (Transform hand in GameObject.Find("Hands").transform)
            hand.GetComponent<Hand>().ResetCards();
    }
    public void ResetCards()
    {
        float offset = (spacing * (float)transform.childCount) / 2f;
        for (int i = 0; i < transform.childCount; i++)
        {
            // Get card transform
            Transform cardTransform = transform.GetChild(i);

            // Calculate spot
            Vector3 cardSpot = new Vector3(-i * spacing + offset, 0, 0);
            
            // Animate move
            cardTransform.DOKill();
            cardTransform.DOLocalMove(cardSpot, 0.5f);
            
            // Set order
            cardTransform.GetComponent<SpriteRenderer>().sortingOrder = -i;

            // Set box colliders
            BoxCollider2D box = cardTransform.GetComponent<BoxCollider2D>();
            
            // Make each box collider smaller EXECPT for first
            if (i == 0)
            {
                // Default size
                box.offset = new Vector2(0, 0);
                box.size = new Vector2(0.96f, 1.5f);
            }
            else
            {
                // Skinner size inbetween cards
                box.offset = new Vector2(-0.361f, 0);
                box.size = new Vector2(0.238f, 1.5f);
            }
        }
    }
}