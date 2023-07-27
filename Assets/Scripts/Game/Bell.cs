using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Bell : MonoBehaviour
{
    float distance = 1f;
    public void AdjustBell(int playerIndex)
    {
		Debug.Log(playerIndex);
		
		Transform handTransform = Players.Singleton.transform.GetChild(playerIndex).GetComponentInChildren<Hand>().transform;
        // Get angle depending on Hand rotation
        float x = handTransform.eulerAngles.z;
        x = Mathf.Sin(x * Mathf.PI/180);

        float y = handTransform.eulerAngles.z;
        y = Mathf.Cos(y * Mathf.PI/180);

        transform.DOMove(handTransform.position + new Vector3(-distance * x, +distance * y, 0), 0.5f);

        CancelInvoke("RingBell");
        InvokeRepeating("RingBell", 7, 5);
    }
    public void RotateBellToHand()
    {
        Hand[] hands = FindObjectsOfType<Hand>();
        foreach (Hand hand in hands)
        {
            if (!hand.player.IsOwner)
                continue;
            
            transform.DORotate(hand.transform.eulerAngles, 1f);
        }
    }
    void RingBell()
	{
        animator.Play("Bell Ring");
    }
    void Awake()
    {
        singleton = this;
        animator = GetComponent<Animator>();
	}
	void Start()
	{
		TurnManager.Singleton.currentPlayerIndex.OnValueChanged += OnCurrentPlayerIndexChanged;
	}
	void OnDestroy()
	{
		TurnManager.Singleton.currentPlayerIndex.OnValueChanged -= OnCurrentPlayerIndexChanged;
		
	}
	public void OnCurrentPlayerIndexChanged(int previousValue, int newValue)
	{
		AdjustBell(newValue);
	}

	Animator animator;
    public static Bell singleton;
}