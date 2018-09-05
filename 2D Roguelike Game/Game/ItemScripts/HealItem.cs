using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItem : MonoBehaviour {

    [Header ("Set in Inspector")]
    public float healSize = 5;
    public float moveSpeed = 0.1f;
    public float moveDuration = 0.5f;
    public int moveDir = 1; // 1 == up
    public GameObject anchor;

	void Update ()
    {
        anchor.transform.Translate(new Vector3(0, moveSpeed * moveDir * Time.deltaTime, 0));
        if (Mathf.Abs(anchor.transform.localPosition.y) > moveDuration)
        {
            Vector3 pos = anchor.transform.localPosition;
            pos.y = moveDir * moveDuration;
            anchor.transform.localPosition = pos;
            moveDir *= -1;
        }
    }
}
