using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bell : MonoBehaviour
{
    float distance = 1f;
    public void AdjustBell(int playerIndex, List<Transform> hands)
    {
        // Get angle depending on Hand rotation
        float x = hands[playerIndex].transform.eulerAngles.z;
        x = Mathf.Sin(x * Mathf.PI/180);

        float y = hands[playerIndex].transform.eulerAngles.z;
        y = Mathf.Cos(y * Mathf.PI/180);

        transform.DOMove(hands[playerIndex].position + new Vector3(-distance * x, +distance * y, 0), 0.5f);

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
            
            transform.eulerAngles = hand.transform.eulerAngles;
        }
    }
    void RingBell() {
        animator.Play("Bell Ring");
    }
    void Awake()
    {
        singleton = this;
        animator = GetComponent<Animator>();
    }
    Animator animator;
    public static Bell singleton;
}
