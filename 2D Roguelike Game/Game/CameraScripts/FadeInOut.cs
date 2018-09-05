using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOut : MonoBehaviour {

    static public FadeInOut S;

    [Header ("Set in Inspector")]
    public GameObject winSprite;
    public GameObject loseSprite;
    public float fadeSpeed = 1000;

    [Header("Set in Dynamically")]
    public SpriteRenderer spriteRenderer;
    public bool fadeOut = true;
    public bool fadeInFinish = false;
    public bool fadeOutFinish = true;
    

    private void Awake()
    {
        S = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        fadeOut = false;
        Color temp = spriteRenderer.color;
        temp.a = 1;
        spriteRenderer.color = temp;
    }
    
	void Update ()
    {
        Color temp = spriteRenderer.color;

        if (fadeOut)
        {
            fadeInFinish = false;
            temp.a = (temp.a * 255 + fadeSpeed * Time.deltaTime) / 255;
            if (temp.a > 1)
            {
                temp.a = 1;
                fadeOutFinish = true;
            }
        }
        else
        {
            temp.a = (temp.a * 255 - fadeSpeed * Time.deltaTime) / 255;
            if (temp.a < 0)
            {
                temp.a = 0;
                fadeInFinish = true;
                fadeOutFinish = false;
            }
        }

        spriteRenderer.color = temp;
    }

    public void SetEndScene(bool win)
    {
        if (win)
        {
            winSprite.SetActive(true);
        }
        else
        {
            loseSprite.SetActive(true);
        }
    }
}
