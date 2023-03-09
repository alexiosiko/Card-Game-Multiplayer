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
    public int randomNumber;
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
        if (!hand || !hand.IsOwner)
            return;

        transform.DOKill();
        transform.DOLocalMoveY(1, 0.2f);
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
        if (!hand || !hand.IsOwner)
            return;

        hand.ResetCards();
    }
    void OnMouseDrag()
    {
        if (!hand || !hand.IsOwner)
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
        if (!hand || !hand.IsOwner)
            return;

        // Check if a click and not a highlight by calculating distance from mounseDown mouseUp
        if (Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), mousePos) < 0.1f) {
            ToggleHighlight();
            return;
        }

        // Check if CURRENT turn
        // todo
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider.name != "Center")
                continue;

            // If current card is not in list, then add to list
            if (hand.highlighted.Contains(transform) == false) {
                ToggleHighlight();
            }
            
            if (Center.singleton.IsValidMove(hand.highlighted))
                hand.MoveCardsToCenter();
            else
            {
                hand.ResetCards();
                RemoveHighlight();
            }
        }
        
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
        if (!hand || !hand.IsOwner)
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
