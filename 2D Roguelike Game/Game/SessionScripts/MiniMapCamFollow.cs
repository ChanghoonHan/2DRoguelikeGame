using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamFollow : MonoBehaviour {

    static public MiniMapCamFollow S;

    private float offsetZ = -10;

    private void Awake()
    {
        S = this;
        this.transform.localPosition = new Vector3(0, 0, offsetZ);
    }

    public void SetCamPos(Vector2 roomIndex)
    {
        roomIndex.x = (int)roomIndex.x - (int)roomIndex.x % 2;
        roomIndex.y = (int)roomIndex.y - (int)roomIndex.y % 2;
        this.transform.localPosition = new Vector3(roomIndex.x, -roomIndex.y, offsetZ);
    }
}
