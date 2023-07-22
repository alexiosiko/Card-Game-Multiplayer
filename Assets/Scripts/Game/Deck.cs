using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Deck : NetworkBehaviour
{
    public float dealDelay = 0.15f;
    [SerializeField] GameObject card;
    [SerializeField] Sprite[] faces;
    public byte[] cardIndexs = new byte[] {
         0,  1,  2,  3,  4,  5,  6,  7,  8,  9,
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
        40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
        50, 51, 52, 53
    };
	public void ShuffleAndDeal()
	{
        Shuffle();
        StartCoroutine(Deal());

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
	IEnumerator Deal()
    {
        int handIndex = 0;
        for (int cardIndex = 0; cardIndex < cardIndexs.Length; cardIndex++)
        {
            // print("index from server: " + cardIndex);
            DealCardsClientRpc(cardIndexs[cardIndex], handIndex);
            yield return new WaitForSeconds(dealDelay);
			
            // Move to next player
            handIndex++;
            if (handIndex >= playersTransform.childCount)
                handIndex = 0;
        }
    }
	[ClientRpc]
    public void DealCardsClientRpc(int cardIndex, int handIndex)
    {
        // Create card
        GameObject c = Instantiate<GameObject>(card, Vector3.zero, Quaternion.identity);

        // Set parent
        c.transform.parent = playersTransform.GetChild(handIndex).GetChild(0); 
        
        // Match card rotation to player 
        c.transform.rotation = c.transform.parent.parent.rotation;

        // Assign face                // For some reason, cardIndexs ranges from [1, 52] and NOT [0, 51] ... ? So cardIndexs[i] - 1
        c.GetComponent<Card>().face = faces[cardIndex];

        // Initialize card ... (parent, cardvalue)
        c.GetComponent<Card>().InitializeCard();

        // If client is owner of hand, then show face card
        if (playersTransform.GetChild(handIndex).GetComponent<NetworkObject>().IsOwner)
            c.GetComponent<SpriteRenderer>().sprite = c.GetComponent<Card>().face;
    
        // Tell hand to update their card's positions
        playersTransform.GetChild(handIndex).GetComponentInChildren<Hand>().ResetCards();
    }
	void Awake()
	{
		Singleton = this;
        playersTransform = GameObject.Find("Players").transform;
	}
	Transform playersTransform;
	public static Deck Singleton;
}
