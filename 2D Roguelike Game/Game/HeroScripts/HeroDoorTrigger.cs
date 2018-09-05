using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroDoorTrigger : MonoBehaviour {

    public bool colWithDoor = false;
    public Vector2 roomIdx = Vector3.zero;
    public DoorController doorController = null;
    public GameObject nextRoomDoorGO = null;

    private bool roomChange = false;

    private void Awake()
    {
        colWithDoor = false;
        roomChange = false;
        roomIdx = Vector3.zero;
    }

    private void Start()
    {
        GameObject tempGO = SessionDrawController.S.GetSessionArray()[(int)roomIdx.y, (int)roomIdx.x].doorsInRoomDic[DoorDirInRoom.down];
        transform.position = tempGO.transform.position;
        MiniMapCamFollow.S.SetCamPos(roomIdx);
        MainCameraController.S.SetRoom((int)roomIdx.y, (int)roomIdx.x);
    }

    private void Update()
    {
        if (roomChange)
        {
            if (FadeInOut.S.fadeInFinish)
            {
                colWithDoor = true;
                roomChange = false;
            }

            if (FadeInOut.S.fadeOutFinish)
            {
                Vector3 doorPosTemp = nextRoomDoorGO.transform.position;
                doorPosTemp.x += 0.5f;
                transform.position = doorPosTemp;
                MiniMapCamFollow.S.SetCamPos(roomIdx);
                MainCameraController.S.SetRoom((int)roomIdx.y, (int)roomIdx.x);
                FadeInOut.S.fadeSpeed = 600;
                FadeInOut.S.fadeOut = false;
                SessionDrawController.S.ChangeMonsterState(roomIdx, Enemy.EnemyState.patroll);
            }
        }

        if (colWithDoor && Input.GetKeyDown(KeyCode.UpArrow) && FadeInOut.S.fadeInFinish && doorController.isOpen)
        {
            switch (doorController.doorDir)
            {
                case DoorDirInRoom.up:
                    roomIdx.y -= 2;
                    nextRoomDoorGO = SessionDrawController.S.GetSessionArray()[(int)roomIdx.y, (int)roomIdx.x].doorsInRoomDic[DoorDirInRoom.down];
                    break;

                case DoorDirInRoom.down:
                    roomIdx.y += 2;
                    nextRoomDoorGO = SessionDrawController.S.GetSessionArray()[(int)roomIdx.y, (int)roomIdx.x].doorsInRoomDic[DoorDirInRoom.up];
                    break;

                case DoorDirInRoom.left:
                    roomIdx.x -= 2;
                    nextRoomDoorGO = SessionDrawController.S.GetSessionArray()[(int)roomIdx.y, (int)roomIdx.x].doorsInRoomDic[DoorDirInRoom.right];
                    break;

                case DoorDirInRoom.right:
                    roomIdx.x += 2;
                    nextRoomDoorGO = SessionDrawController.S.GetSessionArray()[(int)roomIdx.y, (int)roomIdx.x].doorsInRoomDic[DoorDirInRoom.left];
                    break;

                default:
                    break;
            }

            SessionDrawController.S.OpenMiniMapIcon((int)roomIdx.x, (int)roomIdx.y);
            doorController = nextRoomDoorGO.transform.Find("Door").GetComponent<DoorController>();
            doorController.DoorOpen();
            FadeInOut.S.fadeSpeed = 600;
            FadeInOut.S.fadeOut = true;
            roomChange = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Door"))
        {
            print("Enter Door");
            colWithDoor = true;
            doorController = collision.gameObject.GetComponent<DoorController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Door"))
        {
            print("Exit Door");
            colWithDoor = false;
        }
    }

}
