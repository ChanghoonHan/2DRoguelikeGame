using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroFootJumpTrigger : MonoBehaviour {

    public Collider2D footCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        footCollider.isTrigger = false;
        gameObject.SetActive(false);
    }
}
