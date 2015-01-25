using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandLogic : MonoBehaviour {

	public int maxHandSize = 4;
	public float cardOffset = 0.75f;

	public List<GameObject> cardsInHand;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Vector3 getXOffset() {
		float offset = (transform.childCount * cardOffset) - (cardOffset * maxHandSize / 2) - (cardOffset / 2);
		return new Vector3(offset, 0, 0);
	}
}
