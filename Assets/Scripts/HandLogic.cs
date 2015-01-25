using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandLogic : MonoBehaviour {

	public int maxHandSize = 4;
	public float cardXOffset = 1.0f;
	public float cardZOffset = 1.0f;

	[HideInInspector]
	public List<GameObject> cardsInHand;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Reset () {		
		foreach (Transform child in transform) {
			Destroy(child);
		}
		cardsInHand = new List<GameObject>();
	}

	// For hand-spreading on instantiation
	public Vector3 GetXOffset() {
		float offset = (transform.childCount * cardXOffset) - (cardXOffset * maxHandSize / 2) - (cardXOffset / 2);
		return new Vector3(offset, 0, 0);
	}

	// For Z-sorting on instantiation.
	public Vector3 GetZOffset() {
		float offset = transform.childCount * cardZOffset;
		return new Vector3(0, 0, -offset);
	}

	public void FlipNext() {
		for (int i = cardsInHand.Count - 1; i >= 0; i--) {
			var cardLogic = cardsInHand[i].GetComponent<CardLogic>();
			if(cardLogic.faceUp == false) {
				cardLogic.faceUp = true;
				break;
			}
		}
	}
}
