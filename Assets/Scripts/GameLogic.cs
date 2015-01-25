﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandLibrary;

public class GameLogic : MonoBehaviour {

	public GameObject cardPrefab;

	private GameObject c, n, e, s, w;
	private int currentGameReveal = 0;
	private GameObject[][] currentGameRevealOrder;

	private CommandQueue _queue;

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
		_queue = new CommandQueue();

		c = GameObject.Find("Hands/Center");
		n = GameObject.Find("Hands/North");
		e = GameObject.Find("Hands/East");
		s = GameObject.Find("Hands/South");
		w = GameObject.Find("Hands/West");

		ResetGame();
	}

	// Update is called once per frame
	void Update () {
		_queue.Update(Time.deltaTime);
	}

	void ResetState() {
		currentGameReveal = 0;

		var hands = GameObject.FindGameObjectsWithTag("Hand");
		foreach (GameObject hand in hands) {
			hand.SendMessage("Reset");
		}

		var uiEls = GameObject.FindGameObjectsWithTag("UI");
		foreach (GameObject el in uiEls) {
			el.SendMessage("Disable");
		}
	}

	void NewGame() {
		Camera.main.audio.Play();

		currentGameRevealOrder = new GameObject[][] {
			new GameObject[] {s, s, s, s, w, n, e, c},
			new GameObject[] {e, w, n},
			new GameObject[] {e, w, n},
			new GameObject[] {e, w, n},
			new GameObject[] {c}
		};

		var gameDeck = cardDeck.Clone() as string[];
		Shuffle(gameDeck);

		// Deal Cards
		GameObject[] dealOrder = {
			w, n, e, s, c,
			w, n, e, s,
			w, n, e, s,
			w, n, e, s, c
		};
		
		for (int i = 0; i < dealOrder.Length; i++) {
			var deck = gameDeck[i];
			var next = dealOrder[i];
			_queue.Enqueue(Commands.Do(() => { InstantiateCard(deck, next); }), Commands.WaitForSeconds(0.25f));
		}

		_queue.Enqueue(Commands.Do(() => { RevealNext(); }), Commands.WaitForSeconds(0.25f));

		// Enable the UI
		_queue.Enqueue(Commands.Do(() => { 
			var uiEls = GameObject.FindGameObjectsWithTag("UI");
			foreach (GameObject el in uiEls) {
				el.SendMessage("Enable");
			}
		}));
	}

	void ResetGame() {
		ResetState();
		NewGame();
	}

	void EndGame() {
		Debug.Log("GAME OVER");
		var cards = GameObject.FindGameObjectsWithTag("Card");
		foreach (var card in cards) {
			var logic = card.GetComponent<CardLogic>();
			logic.locked = true;
			logic.busy = true;
		}
	}

	void InstantiateCard(string cardName, GameObject hand) {
		// Get information about hand
		var handLogic = hand.GetComponent<HandLogic>();

		// Instantiate card object
		var card = Instantiate(cardPrefab) as GameObject;
		handLogic.cardsInHand.Add(card);

		card.transform.parent = hand.transform;
		card.transform.localPosition = handLogic.GetXOffset() + handLogic.GetZOffset();
		card.transform.localRotation = Quaternion.identity;

		// Setup card parameters
		var cardLogic = card.GetComponent<CardLogic>();
		card.name = cardName;
		cardLogic.SetUp();
	}

	void RevealNext() {
		if(currentGameReveal >= currentGameRevealOrder.Length) {
			EndGame();
			return;
		}

		var hands = currentGameRevealOrder[currentGameReveal];
		for (var j = 0; j < hands.Length; j++) {
			var hand = hands[j];
			_queue.Enqueue(Commands.Do(() => { hand.SendMessage("FlipNext"); }), Commands.WaitForSeconds(0.4f));
		}

		currentGameReveal++;
	}

	// Check to see if two cards are selected.
	void CheckCards () {
		//Debug.Log ("Check Cards!");

		var cards = GameObject.FindGameObjectsWithTag("Card");
		var selectedCards = new List<GameObject>();
		foreach (var card in cards) {
			if (card.GetComponent<CardLogic>().selected) {
				selectedCards.Add(card);
			}
		}
		if (selectedCards.Count > 2) {
			Debug.LogError("More than 2 cards selected. WTF?!");
		}else if (selectedCards.Count == 2) {
			SwapCards(selectedCards[0], selectedCards[1]);
			RevealNext();
		}
	}

	void SwapCards (GameObject a, GameObject b) {
		//Debug.Log("Swap Cards!");

		var parA = a.transform.parent;
		var posA = a.transform.localPosition;
		var rotA = a.transform.localRotation;
		var logA = a.GetComponent<CardLogic>();

		var parB = b.transform.parent;
		var posB = b.transform.localPosition;
		var rotB = b.transform.localRotation;
		var logB = b.GetComponent<CardLogic>();

		_queue.Enqueue(
			Commands.WaitForSeconds(0.2f),
			Commands.Do(() => {
				b.transform.position = new Vector3(0, 0, 1000);
				b.SendMessage("PlayPickup");
			}),
			Commands.WaitForSeconds(0.2f),
			Commands.Do(() => {
				a.transform.position = new Vector3(0, 0, 1000);
				a.SendMessage("PlayPickup");
			}),
			Commands.WaitForSeconds(0.3f),
			Commands.Do(() => {
				//Rudimentary Swap
				logA.selected = false;
				a.transform.parent = parB;
				a.transform.localPosition = posB;
				a.transform.localRotation = rotB;
				logA.locked = true;
				a.SendMessage("PlayPlace");
			}),
			Commands.WaitForSeconds(0.3f),
			Commands.Do(() => {
				logB.selected = false;
				b.transform.parent = parA;
				b.transform.localPosition = posA;
				b.transform.localRotation = rotA;
				logB.locked = true;
				b.SendMessage("PlayPlace");
			})
		);
	}
}
