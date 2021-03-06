using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CommandLibrary;
using HoldemHand;

public class GameLogic : MonoBehaviour
{

    public GameObject cardPrefab;
    public GameObject c, n, e, s, w;

    private GameObject[] hands, playerHands, uiEls;
    private int currentGameReveal = 0;
    private GameObject[][] currentGameRevealOrder;
    private List<GameObject> currentGameCards;
    private CommandQueue _queue = new CommandQueue();

    private static string[] cardDeck = {
        "AH","2H","3H","4H","5H","6H","7H","8H","9H","TH","JH","QH","KH",
        "AD","2D","3D","4D","5D","6D","7D","8D","9D","TD","JD","QD","KD",
        "AC","2C","3C","4C","5C","6C","7C","8C","9C","TC","JC","QC","KC",
        "AS","2S","3S","4S","5S","6S","7S","8S","9S","TS","JS","QS","KS"
    };

    static void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++) {
            int r = i + (int)(Random.value * (n - i));
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }

    // Use this for initialization
    void Start()
    {
        playerHands = new GameObject[] {n, e, s, w};

        hands = new GameObject[] { c, n, e, s, w };
        uiEls = GameObject.FindGameObjectsWithTag("UI");

        ResetState();
    }

    // Update is called once per frame
    void Update()
    {
        _queue.Update(Time.deltaTime);
    }

    void ResetState()
    {
        _queue = new CommandQueue();
        currentGameReveal = 0;
        currentGameCards = new List<GameObject>();

        foreach (var hand in hands) {
            hand.SendMessage("Reset");
        }

        foreach (var el in uiEls) {
            el.SendMessage("Disable");
        }

        Camera.main.audio.Stop();
    }

    void NewGame()
    {
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
            _queue.Enqueue(
                Commands.Do(delegate {
                    currentGameCards.Add(InstantiateCard(deck, next));
                }),
                Commands.WaitForSeconds(0.25f)
            );
        }

        // Run the next reveal
        RevealNext();

        // Enable the UI
        _queue.Enqueue(
            Commands.Do(delegate {
                foreach (var el in uiEls) {
                    el.SendMessage("Enable");
                }
            })
        );
    }

    void ResetGame()
    {
        ResetState();
        NewGame();
    }

    void EndGame()
    {
        //Debug.Log("GAME OVER");
        foreach (var card in currentGameCards) {
            var logic = card.GetComponent<CardLogic>();
            logic.selected = false;
            logic.locked = false;
            logic.busy = true;
        }

        var handsMasks = new Dictionary<GameObject, ulong>();
        var centerCards = c.GetComponent<HandLogic>().cardsInHand;

        // Parse masks for each hand.
        foreach (var hand in playerHands) {
            var cardsInHand = hand.GetComponent<HandLogic>().cardsInHand;
            var cards = centerCards.Concat(cardsInHand).Select(e => e.name).ToArray<string>();
            var cardMask = Hand.ParseHand(string.Join(" ", cards).ToLower());
            handsMasks.Add(hand, cardMask);
            //Debug.Log(string.Format("Hand {0}: {1}", hand.name, Hand.DescriptionFromMask(cardMask)));
        }

        // If every hand is at least a pair, somebody won!
        if (handsMasks.All(e => Hand.EvaluateType(e.Value) >= Hand.HandTypes.Pair)) {
            var winner = handsMasks.OrderByDescending(e => Hand.Evaluate(e.Value)).First();
            //Debug.Log(string.Format("WINNER: {0} with {1}", winner.Key, Hand.DescriptionFromMask(winner.Value)));

            // Was that somebody you (south)?
            EndGameAnnounce(winner.Key);
        } else {
            EndGameAnnounce(null);
        }
        GameObject.Find("SkipTurn").SendMessage("Disable");
    }

    void EndGameAnnounce(GameObject winner)
    {
        AudioSource endMusic;
        if (winner == s) {
            endMusic = GetComponents<AudioSource>()[1];
            //Debug.Log("WINNER IS YOU!");
        } else {
            endMusic = GetComponents<AudioSource>()[2];
            //Debug.Log("YOU LOSE!");
        }

        foreach (var hand in hands) {
            var cards = hand.GetComponent<HandLogic>().cardsInHand;
            foreach (var card in cards) {
                var logic = card.GetComponent<CardLogic>();
                if (hand == winner || (winner != null && hand == c)) {
                    logic.won = true;
                } else {
                    logic.lost = true;
                }
            }
        }

        Camera.main.audio.Stop();
        endMusic.Play();
    }

    GameObject InstantiateCard(string cardName, GameObject hand)
    {
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

        return card;
    }

    void RevealNext()
    {
        if (currentGameReveal >= currentGameRevealOrder.Length) {
            _queue.Enqueue(
                Commands.Do(delegate {
                    EndGame();
                })
            );
            return;
        }

        _queue.Enqueue(
            Commands.Do(delegate {
                foreach (var el in uiEls) {
                    el.SendMessage("Disable");
                }
                foreach (var card in currentGameCards) {
                    card.GetComponent<CardLogic>().busy = true;
                }
            })
        );

        var hands = currentGameRevealOrder[currentGameReveal];
        for (var j = 0; j < hands.Length; j++) {
            var hand = hands[j];
            _queue.Enqueue(
                Commands.Do(delegate {
                    hand.SendMessage("FlipNext");
                }),
                Commands.WaitForSeconds(0.4f));
        }

        _queue.Enqueue(
            Commands.Do(delegate {
                foreach (var el in uiEls) {
                    el.SendMessage("Enable");
                }
                foreach (var card in currentGameCards) {
                    card.GetComponent<CardLogic>().busy = false;
                }
            })
        );

        currentGameReveal++;
    }

    // Check to see if two cards are selected.
    void CheckCards()
    {
        //Debug.Log ("Check Cards!");

        var selectedCards = new List<GameObject>();
        foreach (var card in currentGameCards) {
            if (card.GetComponent<CardLogic>().selected) {
                selectedCards.Add(card);
            }
        }
        if (selectedCards.Count > 2) {
            Debug.LogError("More than 2 cards selected. WTF?!");
        } else if (selectedCards.Count == 2) {
            SwapCards(selectedCards[0], selectedCards[1]);
            RevealNext();
        }
    }

    void SwapCards(GameObject a, GameObject b)
    {
        //Debug.Log("Swap Cards!");

        var parA = a.transform.parent;
        var posA = a.transform.localPosition;
        var rotA = a.transform.localRotation;
        var logA = a.GetComponent<CardLogic>();
        var prlA = parA.GetComponent<HandLogic>();
        prlA.cardsInHand.Remove(a);
        prlA.cardsInHand.Add(b);

        var parB = b.transform.parent;
        var posB = b.transform.localPosition;
        var rotB = b.transform.localRotation;
        var logB = b.GetComponent<CardLogic>();
        var prlB = parB.GetComponent<HandLogic>();
        prlB.cardsInHand.Remove(b);
        prlB.cardsInHand.Add(a);
        
        _queue.Enqueue(
            Commands.WaitForSeconds(0.2f),
            Commands.Do(delegate {
                b.transform.position = new Vector3(0, 0, 1000);
                b.SendMessage("PlayPickup");
            }),
            Commands.WaitForSeconds(0.2f),
            Commands.Do(delegate {
                a.transform.position = new Vector3(0, 0, 1000);
                a.SendMessage("PlayPickup");
            }),
            Commands.WaitForSeconds(0.3f),
            Commands.Do(delegate {
                //Rudimentary Swap
                logA.selected = false;
                a.transform.parent = parB;
                a.transform.localPosition = posB;
                a.transform.localRotation = rotB;
                logA.locked = true;
                a.SendMessage("PlayPlace");
            }),
            Commands.WaitForSeconds(0.3f),
            Commands.Do(delegate {
                logB.selected = false;
                b.transform.parent = parA;
                b.transform.localPosition = posA;
                b.transform.localRotation = rotA;
                logB.locked = true;
                b.SendMessage("PlayPlace");
            })
        );
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
