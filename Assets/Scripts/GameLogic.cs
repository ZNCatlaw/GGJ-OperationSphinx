using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour {

	public GameObject cardPrefab;

	private GameObject c, n, e, s, w;

	// Use this for initialization
	void Start () {
		c = GameObject.Find("Hands/Center");
		n = GameObject.Find("Hands/North");
		e = GameObject.Find("Hands/East");
		s = GameObject.Find("Hands/South");
		w = GameObject.Find("Hands/West");

		NewGame();
	}

	// Update is called once per frame
	void Update () {

	}

	void NewGame() {
		GameObject[] dealOrder = {
			e, s, w, n, c,
			e, s, w, n,
			e, s, w, n,
			e, s, w, n, c
		};

		for (int i = 0; i < dealOrder.Length; i++) {
			InstantiateCard(string.Format("#{0}", i), dealOrder[i]);
		}
	}

	GameObject InstantiateCard(string cardName, GameObject hand) {
		// Get information about hand
		var handLogic = hand.GetComponent<HandLogic>();

		// Instantiate card object
		var card = Instantiate(cardPrefab) as GameObject;
		card.transform.parent = hand.transform;
		card.transform.localPosition = handLogic.getXOffset();
		card.transform.localRotation = Quaternion.identity;

		handLogic.cardsInHand.Add(card);

		// Setup card parameters
		var cardLogic = card.GetComponent<CardLogic>();
		cardLogic.name = cardName;
		cardLogic.SetUp();
		return card;
	}
}
