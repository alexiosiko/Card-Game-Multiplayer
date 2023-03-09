using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
public class Center : NetworkBehaviour
{
    public List<Transform> recentCards = new List<Transform>();
    public CardState cardState;
    public enum CardState {
        Empty,
        Singles,
        Doubles,
        Triples,
        Quads
    };
    public bool IsValidMove(List<Transform> cards) {
        // Convert transforms to List<string> ids
        List<int> ids = new List<int>();
        foreach (Transform card in cards)
            ids.Add(StrToVal(IgnoreSuit(card.GetComponent<Card>().cardId)));

        // Convert recentCards to List<string> recentIds
        List<int> recentIds = new List<int>();
        foreach (Transform card in recentCards)
            recentIds.Add(StrToVal(IgnoreSuit(card.GetComponent<Card>().cardId)));


        // Joker trump
        if (ids.Count == 1 && ids[0] == 14)
            return true;

        
        switch (cardState)
        {
            case CardState.Empty: return EmptyAndUpdateCardState(ids);
            case CardState.Singles: return Singles(ids, recentIds);
            case CardState.Doubles: return Doubles(ids, recentIds);
            case CardState.Triples: return Triples(ids, recentIds);
            case CardState.Quads: break;
            default: break;
        }
        return true;
    }
    bool Singles(List<int> ids, List<int> recentIds)
    {
        // Double 2'ing a 2
        if (ids.Count == 2 && ids[0] == 13 && ids[1] == 13)
            return true;

        // 2'ing a single
        if (ids.Count == 1 && ids[0] >= 13)
            return true;

        if (ids.Count == 1 && ids[0] >= recentIds[0])
            return true;
        return false;
    }
    bool Doubles(List<int> ids, List<int> recentIds)
    {
        // 2'ing a double
        if (ids.Count == 1 && ids[0] >= 13)
            return true;

        if (ids.Count == 2 && IsSameVal(ids) && ids[0] >= recentIds[0])
            return true;
        return false;
    }
    bool Triples(List<int> ids, List<int> recentIds)
    {
        // Double 2'ing a triple
        if (ids.Count == 2 && ids[0] == 13 && ids[1] == 13)
            return true;

        if (ids.Count == 3 && IsSameVal(ids) && ids[0] >= recentIds[0])
            return true;
            
        return false;
    }
    bool Quads(List<int> ids, List<int> recentIds)
    {
        if (ids.Count == 4 &&  IsSameVal(ids) && ids[0] >= recentIds[0])
            return true;

        return false;
    }
    bool EmptyAndUpdateCardState(List<int> ids)
    {
        switch (ids.Count)
        {
            case 1:
                SetCardStateServerRpc(1);
                return true;
            case 2: if (IsSameVal(ids))
                    {
                        SetCardStateServerRpc(2);
                        return true;
                    } break;
            case 3: if (IsSameVal(ids))
                    {
                        print(CardState.Triples);
                        SetCardStateServerRpc(3);
                        return true;
                    } break;
            case 4: if (IsSameVal(ids))
                    {
                        print(CardState.Quads);
                        SetCardStateServerRpc(4);
                        return true;
                    } break;
            default: return false;
        }
        return false;
    }
    
    bool IsSameVal(List<int> ids)
    {
        switch (ids.Count)
        {
            case 2: return ids[0] == ids[1];
            case 3: return ids[0] == ids[1] && ids[0] == ids[2];
            case 4: return ids[0] == ids[1] && ids[0] == ids[2] && ids[0] == ids[3];
            default: print("ERROR?"); return false;
        }
    }
    int StrToVal(string str)
    {
        // Str:    [3, 4, 5, 6, 7, 8,9 ,10, jj, qq, kk, 01, 02, JOKER]
        // Value:  [1, 2, 3, 4, 5, 6,7 ,8 ,  9, 10, 11, 12, 13, 14   ]
        switch (str)
        {

            case "03": return 1;    // 3
            case "04": return 2;    // 4
            case "05": return 3;    // 5
            case "06": return 4;    // 6
            case "07": return 5;    // 7
            case "08": return 6;    // 8
            case "09": return 7;    // 9
            case "10": return 8;    // 10
            case "jj": return 9;    // J
            case "qq": return 10;   // Q
            case "kk": return 11;   // K
            case "01": return 12;   // A
            case "02": return 13;   // 2
            case "jo": return 14;   // JOKER

            default: print($"StringToValue broke of string: {str}"); return 0;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            ClearTableServerRpc();
    }
    [ServerRpc(RequireOwnership = false)] public void ClearRecentsServerRpc()
    {
        ClearRecentsClientRpc();
    }
    [ClientRpc] void ClearRecentsClientRpc()
    {
        recentCards.Clear();
    }
    [ServerRpc(RequireOwnership = false)] public void ClearTableServerRpc()
    {
        ClearTableClientRpc();
    }
    [ClientRpc] void ClearTableClientRpc()
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

        cardState = CardState.Empty;
        recentCards.Clear();
    }
    [ServerRpc(RequireOwnership = false)] void SetCardStateServerRpc(int cardState)
    {
        SetCardStateClientRpc(cardState);
    } 
    [ClientRpc] void SetCardStateClientRpc(int cardState)
    {
        this.cardState = (CardState)cardState;
    }
    string IgnoreSuit(string value)
    {
        return value.Substring(0, 2);
    }
    public void OrderCards()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = i;
    }
    /* bool Single() {
        if (highlighted[0].GetComponent<Card>().value == 14) // Jack
            return true;
        if (recentCards.Count == 1) 
        {
            if (recentCards[0].GetComponent<Card>().value == 13)
                if (highlighted[0].GetComponent<Card>().value == 13)
                    return false;
            else if (highlighted[0].GetComponent<Card>().value >= recentCards[0].GetComponent<Card>().value)
                return true;
        }
        if (recentCards.Count == 2) 
        {
            if (highlighted[0].GetComponent<Card>().value == 13 && recentCards[0].GetComponent<Card>().value < 13) // and atleast one of recent is les than 2
                return true;
        }
        return false;
    }
    bool TwoPair() {
        if (recentCards.Count == 3) 
        {
            Debug.Log("3");
            if (highlighted[0].GetComponent<Card>().value == 13 && highlighted[1].GetComponent<Card>().value == 13) // Ace
                return true;
        }

        if (recentCards.Count == 1 && recentCards[0].GetComponent<Card>().value == 13)
        {
            if (highlighted[0].GetComponent<Card>().value == 13 && highlighted[1].GetComponent<Card>().value == 13) // Ace
                return true;
        }

        if (recentCards.Count == 2) {
            Debug.Log("2");
            if (highlighted[0].GetComponent<Card>().value == highlighted[1].GetComponent<Card>().value)
            {
                if (highlighted[0].GetComponent<Card>().value >= recentCards[0].GetComponent<Card>().value) {
                    return true;
                }
            }
        }
        return false;
    }
    bool ThreePair() {
        if (highlighted[0].GetComponent<Card>().value == highlighted[1].GetComponent<Card>().value
        && highlighted[0].GetComponent<Card>().value == highlighted[2].GetComponent<Card>().value) {
            if (recentCards.Count == 4) {
                if (highlighted[0].GetComponent<Card>().value == 14) // 3 2's
                    return true;
            }
            if (recentCards.Count == 3) {
                if (highlighted[0].GetComponent<Card>().value >= recentCards[0].GetComponent<Card>().value) {
                    return true;
                }
            }
        }
        return false;
    }
    bool FourPair() {
        if (highlighted[0].GetComponent<Card>().value == highlighted[1].GetComponent<Card>().value
        && highlighted[0].GetComponent<Card>().value == highlighted[2].GetComponent<Card>().value
        && highlighted[0].GetComponent<Card>().value == highlighted[3].GetComponent<Card>().value) {
            if (highlighted[0].GetComponent<Card>().value >= recentCards[0].GetComponent<Card>().value) {
                return true;
            }
        }
        return false;
    }
    bool NewCards() {
        if (highlighted.Count == 1) {
            return true;
        }
        if (highlighted.Count == 2) {
            if (highlighted[0].GetComponent<Card>().value == highlighted[1].GetComponent<Card>().value)
                return true;
        }
        if (highlighted.Count == 3) {
            if (highlighted[0].GetComponent<Card>().value == highlighted[1].GetComponent<Card>().value
            && highlighted[0].GetComponent<Card>().value == highlighted[2].GetComponent<Card>().value)
                return true;
        }
        if (highlighted.Count == 4) {
            if (highlighted[0].GetComponent<Card>().value == highlighted[1].GetComponent<Card>().value
            && highlighted[0].GetComponent<Card>().value == highlighted[2].GetComponent<Card>().value
            && highlighted[0].GetComponent<Card>().value == highlighted[3].GetComponent<Card>().value)
                return true;
        }
        return false;
    }
    */
    void Awake()
    {
        singleton = this;
        sideCardsTransform = GameObject.Find("SideCards").transform;
    }
    Transform sideCardsTransform;
    public static Center singleton;
}
