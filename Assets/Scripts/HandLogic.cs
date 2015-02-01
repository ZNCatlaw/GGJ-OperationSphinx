using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandLibrary;

public class HandLogic : MonoBehaviour
{

    public int maxHandSize = 4;
    public float cardXOffset = 1.0f;
    public float cardZOffset = 1.0f;

    [HideInInspector]
    public List<GameObject> cardsInHand;

    // Use this for initialization
    void Start()
    {
    }
    
    // Update is called once per frame
    void Update()
    {

    }

    public void Reset()
    {       
        cardsInHand = new List<GameObject>();

        var cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (var card in cards) {
            GameObject.Destroy(card);
        }
    }

    // For hand-spreading on instantiation
    public Vector3 GetXOffset()
    {
        float offset = (cardsInHand.Count * cardXOffset) - (cardXOffset * maxHandSize / 2) - (cardXOffset / 2);
        return new Vector3(offset, 0, 0);
    }

    // For Z-sorting on instantiation.
    public Vector3 GetZOffset()
    {
        float offset = cardsInHand.Count * cardZOffset;
        return new Vector3(0, 0, -offset);
    }

    public void FlipNext()
    {
        for (var i = cardsInHand.Count - 1; i >= 0; i--) {
            var cardLogic = cardsInHand[i].GetComponent<CardLogic>();
            if (cardLogic.faceUp == false) {
                cardLogic.faceUp = true;
                break;
            }
        }
    }
}
