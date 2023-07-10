using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hand : MonoBehaviour
{
    public List<Transform> highlighted;
	public bool isLocalReady = true;
    [SerializeField] Color defaultColor = new Color(1,1,1,1);
    public float spacing = 0.2f;
    public bool isTurn = false; 
    public  bool isPassed = false;
    [SerializeField] GameObject passTransform;
    public bool GetPass()
    {
        return isPassed;
    }
	public void SetLocalReady(bool value)
	{
		print("setting local");
		isLocalReady = value;
	}
    public void SetPass(bool value)
    {
        isPassed = value;
    }
    public void RotateCameraToHand()
    {
        if (!player.IsOwner)
            return;
        
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
        Camera.main.transform.eulerAngles = transform.eulerAngles;
    }
	public void HandleMove()
	{
		MoveCardsToCenter();
		isLocalReady = false;
	}
    public void MoveCardsToCenter()
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
            GameManager.singleton.MoveCardsToCenterServerRpc(card.GetComponent<Card>().cardId);

		// Next player turn
        TurnManager.singleton.NextPlayerServerRpc();
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
                box.size = new Vector2(0.84f, 1.2f);
            }
            else
            {
                // Skinner size inbetween cards
                box.offset = new Vector2(-0.3f, 0);
                box.size = new Vector2(0.24f, 1.3f);
            }
        }
    }
    void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    public Player player;
}