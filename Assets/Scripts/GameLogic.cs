using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour {

	public GameObject cardPrefab;

	private GameObject c, n, e, s, w;

	private static string[] cardDeck = {
		"AH","2H","3H","4H","5H","6H","7H","8H","9H","TH","JH","QH","KH",
		"AD","2D","3D","4D","5D","6D","7D","8D","9D","TD","JD","QD","KD",
		"AC","2C","3C","4C","5C","6C","7C","8C","9C","TC","JC","QC","KC",
		"AS","2S","3S","4S","5S","6S","7S","8S","9S","TS","JS","QS","KS"
	};

	static void Shuffle<T>(T[] array) {
		int n = array.Length;
		for (int i = 0; i < n; i++) {
			int r = i + (int)(Random.value * (n - i));
			T t = array[r];
			array[r] = array[i];
			array[i] = t;
		}
	}

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

		GameObject[][] revealOrder = {
			new GameObject[] {e, s, w, n, c},
			new GameObject[] {e, s, w, n},
			new GameObject[] {e, s, w, n},
			new GameObject[] {e, s, w, n},
			new GameObject[] {c}
		};

		var gameDeck = cardDeck.Clone() as string[];
		Shuffle(gameDeck);

		// Deal Cards
		for (int i = 0; i < dealOrder.Length; i++) {
			InstantiateCard(gameDeck[i], dealOrder[i]);
		}

		// Flip over
		// TODO: Obviously don't flip them all over.
		for (var i = 0; i < revealOrder.Length - 1; i++) {
			var hands = revealOrder[i];
			for (var j = 0; j < hands.Length; j++) {
				var hand = hands[j];
				hand.SendMessage("FlipNext");
			}
		}
	}

	GameObject InstantiateCard(string cardName, GameObject hand) {
		// Get information about hand
		var handLogic = hand.GetComponent<HandLogic>();

		// Instantiate card object
		var card = Instantiate(cardPrefab) as GameObject;
		card.transform.parent = hand.transform;
		card.transform.localPosition = handLogic.GetXOffset() + handLogic.GetZOffset();
		card.transform.localRotation = Quaternion.identity;

		handLogic.cardsInHand.Add(card);

		// Setup card parameters
		var cardLogic = card.GetComponent<CardLogic>();
		card.name = cardName;
		cardLogic.SetUp();
		return card;
	}
}
