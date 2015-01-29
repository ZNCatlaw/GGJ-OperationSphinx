using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour
{

    public string messageToGame = "";

    private bool mouseIn = false;
    private bool interactive = true;

    private static Color highlightColor = new Color(1.0f, 0.75f, 0.75f);

    // Use this for initialization
    void Start()
    {
    }
    
    // Update is called once per frame
    void Update()
    {
        // Color based on state
        var shaderColor = Color.white;
        if (mouseIn && interactive) {
            shaderColor = shaderColor * highlightColor;
        }       
        renderer.material.color = shaderColor;
    }

    void OnMouseDown()
    {
        if (interactive) {
            Camera.main.SendMessage(messageToGame);
        }
    }

    void OnMouseEnter()
    {
        if (!Input.touchSupported) {
            mouseIn = true;
        }
    }

    void OnMouseExit()
    {
        mouseIn = false;
    }

    void Enable()
    {
        interactive = true;
    }

    void Disable()
    {
        interactive = false;
    }
}
