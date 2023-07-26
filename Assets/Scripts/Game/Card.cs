using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public Sprite face;
    public Sprite back;
    public string cardId = ""; // [1,2,3,4,5,6,7,8 ,9,10,11,12,13, 14]
    [SerializeField] Color highlightColor = new Color(1,0.8f,1,1);
    [SerializeField] Color defaultColor = new Color(1,1,1,1);
    bool highlight = false;
    public void InitializeCard()
    {
        GetCardId();
        GetHand();
    }
    void GetCardId()
    {
        cardId = face.name;
        // Face:   [3,4,5,6,7,8,9,10,J, Q, K, A, 2, JOKER]
        // Value:  [1,2,3,4,5,6,7,8 ,9,10,11,12,13, 14   ]
    }
    void OnMouseOver()
    {
        if (!hand || !hand.player.IsOwner)
            return;

        transform.DOKill();
        transform.DOLocalMoveY(0.3f, 0.2f);
    }
    public void ToggleHighlight()
    {
        if (highlight == false) {
            highlight = true;
            spriteRenderer.color = highlightColor;
            // Add to list
            GetComponentInParent<Hand>().highlighted.Add(transform);
        }
        else {
            highlight = false;
            spriteRenderer.color = defaultColor;
            // Remove from list
            GetComponentInParent<Hand>().highlighted.Remove(transform);
        }
    }
    public void RemoveHighlight()
    {
        highlight = false;
        if (hand.highlighted.Contains(transform))
            hand.highlighted.Remove(transform);
        spriteRenderer.color = defaultColor;
    }
    void OnMouseExit()
    {    
        if (!hand || !hand.player.IsOwner)
            return;

        hand.ResetCards();
    }
    void OnMouseDrag()
    {
        if (!hand || !hand.player.IsOwner)
            return;
        
        // This is so stop any current tweens
        transform.DOKill();

        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10)
        + mouseDragOffset;
    }
    Vector3 mousePos;
    Vector3 mouseDragOffset;
    void OnMouseUp()
    {
        // If hand doesn't exist (When re-parented to center)
        // If hand is not client owner
        // If hand is not current turn
        if (!hand || !hand.player.IsOwner)
            return;

		if (hand.isLocalReady == false)
		{
			print("Is not local ready. This is to prevent double playing");
			return;
		}

        // Check if a click and not a highlight by calculating distance from mounseDown mouseUp
        if (Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), mousePos) < 0.1f) {
            ToggleHighlight();
            return;
        }

		// If not everyone is ready
		if (TurnManager.Singleton.everyoneIsReady.Value == false)
		{
			Debug.Log("Not everyone is ready!");
			return;
		}

        // Check if current turn
        if (!hand.isTurn.Value)
            return;
        
		if (HitCenter())
		{
            // If current card is not in list, then add to list
            if (hand.highlighted.Contains(transform) == false)
                ToggleHighlight();
            
            if (GameLogic.singleton.IsValidMove(hand.highlighted))
                hand.HandleMove();
            else
            {
                hand.ResetCards();
                RemoveHighlight();
            }
        }
    }
	bool HitCenter()
	{
		RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (var hit in hits)
            if (hit.collider.name == "Center")
                return true;

		return false;
	}
    public void RemoveHand()
    {
        hand = null;
    }
    public void GetHand()
    {
        hand = GetComponentInParent<Hand>();
    }
    void OnMouseDown()
    {
        if (!hand || !hand.player.IsOwner)
            return;

        // Get card click offset -> for OnMouseDrag()
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        mouseDragOffset = transform.position - mousePos;
    }
    [SerializeField] Hand hand;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    SpriteRenderer spriteRenderer;
}
