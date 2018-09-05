using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour {

    static public MainCameraController S;

    [Header("Set in Inspector")]
    public float camFollowXU = 0.1f;
    public float camFollowYU = 0.5f;
    public float camOffsetX = 4;
    public float camOffsetY = 2;
    public Transform heroTrans;
    public Transform sessionAnchorTrans;

    [Header ("Set Dynamically")]
    public Transform roomAnchorTrans;
    public Camera mainCamera;
    public float camHeight;
    public float camWidth;
    public float roomHeight;
    public float roomWidth;
    public float camPosZ = -10;

    private void Awake()
    {
        S = this;
        mainCamera = Camera.main;
        camHeight = mainCamera.orthographicSize;
        camWidth = mainCamera.aspect * camHeight;
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 tempCamPos = transform.position;
        Vector3 heroPosTemp = heroTrans.position;

        heroPosTemp.x = camOffsetX * HeroController.S.moveDir + heroPosTemp.x;
        heroPosTemp.y = camOffsetY + heroPosTemp.y;
        tempCamPos.x = Mathf.Lerp(tempCamPos.x, heroPosTemp.x, camFollowXU);
        tempCamPos.y = Mathf.Lerp(tempCamPos.y, heroPosTemp.y, camFollowYU);
        tempCamPos.z = camPosZ;
        transform.position = tempCamPos;

        Vector3 tempCamLocalPos = transform.localPosition;


        if (tempCamLocalPos.y < camHeight - 0.5f)
        {
            tempCamLocalPos.y = camHeight - 0.5f;
        }

        if (tempCamLocalPos.y > roomHeight - camHeight - 0.5f)
        {
            tempCamLocalPos.y = roomHeight - camHeight - 0.5f;
        }

        if (tempCamLocalPos.x < camWidth - 0.5f)
        {
            tempCamLocalPos.x = camWidth - 0.5f;
        }

        if (tempCamLocalPos.x > roomWidth - camWidth - 0.5f)
        {
            tempCamLocalPos.x = roomWidth - camWidth - 0.5f;
        }

        tempCamLocalPos.z = camPosZ;
        transform.localPosition = tempCamLocalPos;
    }

    public void SetRoom(int row, int col)
    {
        roomAnchorTrans = sessionAnchorTrans.Find("RoomAnchor" + row + col).transform;
        Vector2 tempRoomSize = SessionDrawController.S.GetSessionArray()[row, col].size;
        roomHeight = tempRoomSize.y;
        roomWidth = tempRoomSize.x;
        transform.SetParent(roomAnchorTrans);

        Vector3 heroPosTemp = heroTrans.position;
        heroPosTemp.x = camOffsetX * HeroController.S.moveDir + heroPosTemp.x;
        heroPosTemp.y = camOffsetY + heroPosTemp.y;
        HeroController.S.SetRoomChange();
        transform.position = heroPosTemp;
    }
}
