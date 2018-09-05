using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Set in Inspector")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header ("Set Dynamically")]
    public bool isOpen;
    public DoorDirInRoom doorDir;
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
    }

    public void DoorOpen()
    {
        spriteRenderer.sprite = openSprite;
        isOpen = true;
    }
}
