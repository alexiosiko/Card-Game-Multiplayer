using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static CardState cardState;
    public enum CardState {
        Empty,
        Singles,
        Doubles,
        Triples,
        Quads
    };
}
