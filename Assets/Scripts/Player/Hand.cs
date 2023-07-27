using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class Hand : NetworkBehaviour
{
    public List<Transform> highlighted;
	public bool isLocalReady = true;
    [SerializeField] Color defaultColor = new Color(1,1,1,1);
    public float spacing = 0.2f;
    public NetworkVariable<bool> isTurn = new NetworkVariable<bool>(false); 
    public NetworkVariable<bool> isPassed = new NetworkVariable<bool>(false);
    public bool GetIsPassed()
    {
        return isPassed.Value;
    }
	[ClientRpc] public void SetLocalReadyClientRpc(bool value)
	{
		isLocalReady = value;
	}
    public void RotateCameraToHand()
    {
        if (!player.IsOwner)
            return;
        
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
        Camera.main.transform.DORotate(transform.eulerAngles, 2f);
    }
	public void HandleMove()
	{
		TurnManager.Singleton.UnReadyPlayersServerRpc();
		Center.singleton.MoveCardsToCenter(highlighted);
		isLocalReady = false;
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
	public override void OnDestroy()
	{
		// isPassed.OnValueChanged -= GetComponentInParent<Player>().OnIsPassedValueChanged;
	}
    void Awake()
    {
		isPassed.OnValueChanged += GetComponentInParent<Player>().OnIsPassedValueChanged;
        player = GetComponentInParent<Player>();
    }
    public Player player;
}