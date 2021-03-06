﻿using UnityEngine;
using System.Collections;
using CommandLibrary;

public class CardLogic : MonoBehaviour
{

    public string cardName = "";
    public bool faceUp = false;
    public bool locked = false;
    public bool selected = false;
    [HideInInspector]
    public bool busy = false;
    public bool won = false;
    public bool lost = false;

    private bool mouseIn = false;
    private bool visLocked = false;
    private bool visSelected = false;
    private bool visFaceUp = false;

    private static Quaternion faceUpRotation = Quaternion.Euler(0, 180, 0);
    private static Quaternion lockedRotation = Quaternion.Euler(0, 0, 10);
    private static Vector3 selectedPosition = new Vector3(0.0f, 0.25f, 0.0f);
    private static Color lockedColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
    private static Color highlightColor = new Color(1.0f, 0.75f, 0.75f, 1.0f);
    private static Color selectedColor = new Color(0.75f, 0.75f, 1.0f, 1.0f);
    private static Color lostColor = new Color(1.0f, 0.85f, 0.85f, 1.0f);
    private static Color wonColor = new Color(0.85f, 0.85f, 1.0f, 1.0f);

    private static AudioClip[] drawSounds = Resources.LoadAll<AudioClip>("Sounds/Draw");
    private static AudioClip[] flipSounds = Resources.LoadAll<AudioClip>("Sounds/Flip");
    private static AudioClip[] placeSounds = Resources.LoadAll<AudioClip>("Sounds/Place");
    private static AudioClip[] takeSounds = Resources.LoadAll<AudioClip>("Sounds/Take");

    // Called by GameLogic.CardInstantiate
    public void SetUp()
    {
        var cardMaterial = Resources.Load("Materials/Cards/" + name, typeof(Material)) as Material;
        var frontQuad = this.transform.FindChild("Front").gameObject;
        frontQuad.renderer.material = cardMaterial;
        PlayRandomSound(drawSounds);
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        // Transition into states
        if (faceUp != visFaceUp) {
            visFaceUp = faceUp;
            if (faceUp) {
                transform.rotation = transform.rotation * faceUpRotation;
            } else {
                transform.rotation = transform.rotation * Quaternion.Inverse(faceUpRotation);
            }
            PlayRandomSound(flipSounds);
        }

        if (selected != visSelected) {
            visSelected = selected;
            if (selected) {
                transform.localPosition = transform.localPosition + selectedPosition;
            } else {
                transform.localPosition = transform.localPosition - selectedPosition;
            }
        }

        if (locked != visLocked) {
            visLocked = locked;
            if (locked) {
                transform.rotation = transform.rotation * lockedRotation;
            } else {
                transform.rotation = transform.rotation * Quaternion.Inverse(lockedRotation);
            }
        }

        // Color based on state
        var shaderColor = Color.white;

        if (locked) {
            shaderColor = shaderColor * lockedColor;
        }

        if (selected) {
            shaderColor = shaderColor * selectedColor;
        }

        if (won) {
            shaderColor = shaderColor * wonColor;
        } else if (lost) {
            shaderColor = shaderColor * lostColor;
        }

        if (mouseIn && !locked && !busy && faceUp) {
            shaderColor = shaderColor * highlightColor;
        }

        var frontQuad = this.transform.FindChild("Front").gameObject;
        frontQuad.renderer.material.color = shaderColor;
    }

    void PlayRandomSound(AudioClip[] sounds)
    {
        var myAudio = gameObject.AddComponent<AudioSource>();
        myAudio.clip = sounds[Random.Range(0, sounds.Length)];
        myAudio.Play();
    }

    void PlayPickup()
    {
        PlayRandomSound(takeSounds);
    }

    void PlayPlace()
    {
        PlayRandomSound(placeSounds);
    }
    
    void OnMouseDown()
    {
        //Debug.Log(string.Format("MouseDown `{0}`, hand `{1}`", name, this.transform.parent.name));
        if (!locked && !busy && faceUp) {
            //Check if any other card in the same hand is locked
            var children = transform.parent.GetComponentsInChildren<CardLogic>();
            foreach (var child in children) {
                if (child.selected && child != this) {
                    child.selected = false;
                }
            }

            //Swap selected state
            selected = !selected;
            Update();

            //Possibly trigger a check.
            if (selected) {
                GameObject.Find("Main Camera").SendMessage("CheckCards");
            }
        }
    }

    void OnMouseEnter()
    {
        //Debug.Log(string.Format("MouseEnter `{0}`, hand `{1}`", name, this.transform.parent.name));
        if (!Input.touchSupported) {
            mouseIn = true;
        }
    }

    void OnMouseExit()
    {
        //Debug.Log(string.Format("MouseExit `{0}`, hand `{1}`", name, this.transform.parent.name));
        mouseIn = false;
    }
}
