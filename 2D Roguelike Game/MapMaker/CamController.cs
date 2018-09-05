using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamController : MonoBehaviour {

    public int orthoSizeMin;
    public int orthoSizeMax;
    public int scrollSpeed;
    public int camSpeed;
    public float camMovePadding;

    private Camera thisCam;

    void Awake ()
    {
        thisCam = this.gameObject.GetComponent<Camera>();
        orthoSizeMin = 5;
        orthoSizeMax = 40;
        scrollSpeed = 1;
        camSpeed = 20;
        camMovePadding = 0.001f;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 mPos = Input.mousePosition;
        Vector3 transVec = Vector3.zero;
        Vector3 mPosInViewport = thisCam.ScreenToViewportPoint(mPos);

        if (mPosInViewport.x < 0 + camMovePadding)
        {
            transVec.x = -camSpeed * Time.deltaTime;
        }

        if (mPosInViewport.x > 1 - camMovePadding)
        {
            transVec.x = camSpeed * Time.deltaTime;
        }

        if (mPosInViewport.y < 0 + camMovePadding)
        {
            transVec.y = -camSpeed * Time.deltaTime;
        }

        if (mPosInViewport.y > 1 - camMovePadding)
        {
            transVec.y = camSpeed * Time.deltaTime;
        }

        this.transform.Translate(transVec);


        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            thisCam.orthographicSize += scrollSpeed;
            if (thisCam.orthographicSize > orthoSizeMax)
            {
                thisCam.orthographicSize = orthoSizeMax;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            thisCam.orthographicSize -= scrollSpeed;
            if (thisCam.orthographicSize < orthoSizeMin)
            {
                thisCam.orthographicSize = orthoSizeMin;
            }
        }
    }
    
    public void MakeOrthoSize10 ()
    {
        thisCam.orthographicSize = 10;
    }

    public void MakeCamPosZero ()
    {
        Vector3 pos = Vector3.zero;
        pos.z = -10;
        this.transform.position = pos;
    }
}
