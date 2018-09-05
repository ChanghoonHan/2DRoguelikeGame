using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionDrawController : MonoBehaviour {

    enum BlockType
    {
        ground,
        jumpGround,
        tile,
        wall,
        hill,
        door,
        tileWall,
        itme,
        npc,
        monster
    }

    [System.Serializable]
    public struct tilePrefabs
    {
        public List<GameObject> tilePrfabs;
    }

    static public SessionDrawController S;

    [Header("Set in Inspector")]
    public List<Sprite> miniMapIconSpites;
    public Transform SessionAnchorTrans;
    public GameObject RoomAnchorPrefab;
    public int mapOffset = 100;
    public List<tilePrefabs> tilePrefabList;
    public List<TextAsset> mapTextFileList = new List<TextAsset>();
    public List<Dictionary<DoorDirInRoom, List<string[,]>>> roomList = new List<Dictionary<DoorDirInRoom, List<string[,]>>>();

    [Header("Set Dynamically")]
    public Vector2 bossRoomIdx;
    private RoomInfo[,] sessionArray;


    private void Awake()
    {
        S = this;
        roomList = new List<Dictionary<DoorDirInRoom, List<string[,]>>>();
        ParsingMapTextFile();
    }

    // Use this for initialization
    void Start () {
        DrawSession();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetSessionArray(RoomInfo[,] sessionArrayInput)
    {
        sessionArray = sessionArrayInput;
    }

    public RoomInfo[,] GetSessionArray()
    {
        return sessionArray;
    }

    public void ChangeMonsterState(Vector2 roomIdx, Enemy.EnemyState enemyState)
    {
        List<GameObject> monterList = sessionArray[(int)roomIdx.y, (int)roomIdx.x].monsterList;

        Enemy enemy = null;
        foreach (var monster in monterList)
        {
            enemy = monster.GetComponent<Enemy>();
            enemy.SetState(enemyState);
        }
    }

    void ParsingMapTextFile()
    {
        Dictionary < DoorDirInRoom, List<string[,]>> roomDicTemp = new Dictionary<DoorDirInRoom, List<string[,]>>();
        string[] temp;
        string[] mapTextTempArray;
        string[,] roomTextTempArray;
        List<string[]> roomStringTemp = new List<string[]>();
        DoorDirInRoom roomType = DoorDirInRoom.none;

        for (int i = 0; i < (int)RoomType.bridge; i++)//텍스트 파일 5개 돌기
        {
            mapTextTempArray = mapTextFileList[i].text.Split('\n');//각각의 텍스트파일 줄별로 파싱

            roomDicTemp = new Dictionary<DoorDirInRoom, List<string[,]>>();
            roomStringTemp = new List<string[]>();

            for (int j = 0; j < (int)DoorDirInRoom.end; j++)//문에 따라서 맵데이터를 저장 할 변수 생성
            {
                roomDicTemp.Add((DoorDirInRoom)j, new List<string[,]>());
            }

            for (int index = 0; index < mapTextTempArray.Length-1; index++)
            {
                if (mapTextTempArray[index].Trim() == "")
                {
                    continue;
                }

                if (mapTextTempArray[index][0] == '#')
                {
                    temp = mapTextTempArray[index].Split('_');
                    SetDoorDirInRoomType(temp[1], out roomType);
                    continue;
                }

                if (mapTextTempArray[index][0] == '@')
                {
                    roomTextTempArray = new string[roomStringTemp.Count, roomStringTemp[0].Length];
                    int row = 0;
                    int col = 0;
                    foreach (var strArray in roomStringTemp)
                    {
                        foreach (var block in strArray)
                        {
                            roomTextTempArray[row, col] = block;
                            col++;
                        }
                        col = 0;
                        row++;
                    }
                    roomDicTemp[roomType].Add(roomTextTempArray);
                    roomType = DoorDirInRoom.none;
                    roomStringTemp.Clear();
                    continue;
                }

                roomStringTemp.Add(mapTextTempArray[index].Trim().Split(' '));
            }

            roomList.Add(roomDicTemp);
        }
    }

    void SetDoorDirInRoomType(string roomDirStr, out DoorDirInRoom roomType)
    {
        switch (roomDirStr)
        {
            case "R":
                roomType = DoorDirInRoom.right;
                break;
            case "D":
                roomType = DoorDirInRoom.down;
                break;
            case "DR":
                roomType = DoorDirInRoom.downRight;
                break;
            case "L":
                roomType = DoorDirInRoom.left;
                break;
            case "LR":
                roomType = DoorDirInRoom.leftRight;
                break;
            case "DL":
                roomType = DoorDirInRoom.downLeft;
                break;
            case "DLR":
                roomType = DoorDirInRoom.downLeftRight;
                break;
            case "U":
                roomType = DoorDirInRoom.up;
                break;
            case "UR":
                roomType = DoorDirInRoom.upRight;
                break;
            case "UD":
                roomType = DoorDirInRoom.upDown;
                break;
            case "UDR":
                roomType = DoorDirInRoom.upDownRight;
                break;
            case "UL":
                roomType = DoorDirInRoom.upLeft;
                break;
            case "ULR":
                roomType = DoorDirInRoom.upLeftRight;
                break;
            case "UDL":
                roomType = DoorDirInRoom.upDownLeft;
                break;
            case "UDLR":
                roomType = DoorDirInRoom.upDownLeftRight;
                break;
            default:
                roomType = DoorDirInRoom.none;
                break;
        }
    }

    void DrawSession()
    {
        RoomType roomTypeTemp;
        DoorDirInRoom doorDirTemp;
        List<string[,]> tempRoomList;
        int randIdx = 0;
        string[,] roomStrTemp;
        GameObject blockTemp = null;
        GameObject roomAnchorTemp = null;

        List<GameObject> blockListTemp = null;
        int randTileIdx;


        for (int sessionRow = 0; sessionRow < sessionArray.GetLength(0); sessionRow += 2)
        {
            for (int sessionCol = 0; sessionCol < sessionArray.GetLength(1); sessionCol += 2)
            {
                roomTypeTemp = sessionArray[sessionRow, sessionCol].roomType;
                if (roomTypeTemp == RoomType.none)
                {
                    continue;
                }

                if (roomTypeTemp == RoomType.boss)//DEBUG!!!!!!!!!
                {
                    bossRoomIdx = new Vector2(sessionCol, sessionRow);
                }

                doorDirTemp = sessionArray[sessionRow, sessionCol].doorDir;
                tempRoomList = roomList[(int)roomTypeTemp][doorDirTemp];
                randIdx = Random.Range(0, tempRoomList.Count);
                roomStrTemp = tempRoomList[randIdx];
                blockTemp = null;
                blockListTemp = null;
                roomAnchorTemp = Instantiate<GameObject>(RoomAnchorPrefab);
                roomAnchorTemp.transform.SetParent(SessionAnchorTrans);
                roomAnchorTemp.transform.localPosition = new Vector3(sessionCol / 2 * mapOffset, -(sessionRow / 2 * mapOffset), 0);

                sessionArray[sessionRow, sessionCol].size.y = roomStrTemp.GetLength(0);
                sessionArray[sessionRow, sessionCol].size.x = roomStrTemp.GetLength(1);
                roomAnchorTemp.name = "RoomAnchor" + sessionRow + sessionCol;
                if (sessionRow == 0 && sessionCol == 0)
                {
                    MainCameraController.S.SetRoom(0, 0);
                }

                for (int row = 0; row < roomStrTemp.GetLength(0); row++)
                {
                    for (int col = 0; col < roomStrTemp.GetLength(1); col++)
                    {
                        switch (roomStrTemp[row, col])
                        {
                            case "W":
                                blockListTemp = tilePrefabList[(int)BlockType.wall].tilePrfabs;
                                break;
                            case "G":
                                blockListTemp = tilePrefabList[(int)BlockType.ground].tilePrfabs;
                                break;
                            case "JG":
                                blockListTemp = tilePrefabList[(int)BlockType.jumpGround].tilePrfabs;
                                break;
                            case "T":
                                blockListTemp = tilePrefabList[(int)BlockType.tile].tilePrfabs;
                                break;
                            case "TW":
                                blockListTemp = tilePrefabList[(int)BlockType.tileWall].tilePrfabs;
                                break;
                            case "LH":
                                blockListTemp = tilePrefabList[(int)BlockType.hill].tilePrfabs;
                                blockTemp = Instantiate<GameObject>(blockListTemp[0]);
                                break;
                            case "RH":
                                blockListTemp = tilePrefabList[(int)BlockType.hill].tilePrfabs;
                                blockTemp = Instantiate<GameObject>(blockListTemp[1]);
                                break;
                            case "I":
                                blockListTemp = tilePrefabList[(int)BlockType.itme].tilePrfabs;
                                break;
                            case "M":
                                blockListTemp = tilePrefabList[(int)BlockType.monster].tilePrfabs;
                                break;
                            case "NP":
                                blockListTemp = tilePrefabList[(int)BlockType.npc].tilePrfabs;
                                break;
                            case "DU":
                                InitDoor(ref blockListTemp, ref blockTemp, sessionRow, sessionCol, DoorDirInRoom.up, roomTypeTemp);
                                break;
                            case "DD":
                                InitDoor(ref blockListTemp, ref blockTemp, sessionRow, sessionCol, DoorDirInRoom.down, roomTypeTemp);
                                break;
                            case "DL":
                                InitDoor(ref blockListTemp, ref blockTemp, sessionRow, sessionCol, DoorDirInRoom.left, roomTypeTemp);
                                break;
                            case "DR":
                                InitDoor(ref blockListTemp, ref blockTemp, sessionRow, sessionCol, DoorDirInRoom.right, roomTypeTemp);
                                break;
                            default:
                                break;
                        }

                        if (blockListTemp != null)
                        {
                            if (roomStrTemp[row, col] != "LH" && roomStrTemp[row, col] != "RH" && roomStrTemp[row, col] != "DU" &&
                                roomStrTemp[row, col] != "DD" && roomStrTemp[row, col] != "DL" && roomStrTemp[row, col] != "DR")
                            {
                                if (roomStrTemp[row, col] == "W")
                                {
                                    if (row == 0 && col == 0)
                                    {
                                        blockTemp = Instantiate<GameObject>(blockListTemp[blockListTemp.Count - 1]);
                                        blockTemp.transform.Rotate(new Vector3(0, 0, 90));
                                    }
                                    else if (row == 0 && col == roomStrTemp.GetLength(1) - 1)
                                    {
                                        blockTemp = Instantiate<GameObject>(blockListTemp[blockListTemp.Count - 1]);
                                        blockTemp.transform.Rotate(new Vector3(0, 0, 180));
                                    }
                                    else if (row == roomStrTemp.GetLength(0) - 1 && col == roomStrTemp.GetLength(1) - 1)
                                    {
                                        blockTemp = Instantiate<GameObject>(blockListTemp[blockListTemp.Count - 1]);
                                        blockTemp.transform.Rotate(new Vector3(0, 0, 270));
                                    }
                                    else if (row == roomStrTemp.GetLength(0) - 1 && col == 0)
                                    {
                                        blockTemp = Instantiate<GameObject>(blockListTemp[blockListTemp.Count - 1]);
                                    }
                                    else if (row == 0)
                                    {
                                        randTileIdx = Random.Range(0, blockListTemp.Count - 1);
                                        blockTemp = Instantiate<GameObject>(blockListTemp[randTileIdx]);
                                        blockTemp.transform.Rotate(new Vector3(0, 0, 180));
                                    }
                                    else if (row == roomStrTemp.GetLength(0) - 1)
                                    {
                                        randTileIdx = Random.Range(0, blockListTemp.Count - 1);
                                        blockTemp = Instantiate<GameObject>(blockListTemp[randTileIdx]);
                                    }
                                    else if (col == 0)
                                    {
                                        randTileIdx = Random.Range(0, blockListTemp.Count - 1);
                                        blockTemp = Instantiate<GameObject>(blockListTemp[randTileIdx]);
                                        blockTemp.transform.Rotate(new Vector3(0, 0, 90));
                                    }
                                    else if (col == roomStrTemp.GetLength(1) - 1)
                                    {
                                        randTileIdx = Random.Range(0, blockListTemp.Count - 1);
                                        blockTemp = Instantiate<GameObject>(blockListTemp[randTileIdx]);
                                        blockTemp.transform.Rotate(new Vector3(0, 0, 270));
                                    }
                                    else
                                    {
                                        blockTemp = Instantiate<GameObject>(blockListTemp[blockListTemp.Count - 1]);
                                    }
                                }
                                else
                                {
                                    randTileIdx = Random.Range(0, blockListTemp.Count);
                                    blockTemp = Instantiate<GameObject>(blockListTemp[randTileIdx]);
                                }
                            }

                            blockTemp.transform.SetParent(roomAnchorTemp.transform);
                            blockTemp.transform.localPosition = new Vector3(col, row, 0);
                            GameObject tempGO = null;
                            switch (roomStrTemp[row, col])
                            {
                                case "I":
                                    tempGO = blockTemp.GetComponent<ItemTile>().SetItem();
                                    tempGO.transform.position = blockTemp.transform.position;
                                    Destroy(blockTemp);
                                    break;
                                case "M":
                                    if (roomTypeTemp == RoomType.boss)
                                    {
                                        blockTemp.GetComponent<MonsterTile>().Boss = true;
                                    }
                                    tempGO = blockTemp.GetComponent<MonsterTile>().SetMonster();

                                    if (roomTypeTemp == RoomType.boss)
                                    {
                                        Vector3 tempBlockPos = blockTemp.transform.position;
                                        tempBlockPos.y += 1;
                                        tempGO.transform.position = tempBlockPos;
                                    }
                                    else
                                    {
                                        tempGO.transform.position = blockTemp.transform.position;
                                    }

                                    MovingEnemy movingEnemy = tempGO.GetComponent<MovingEnemy>();
                                    if (movingEnemy != null)
                                    {
                                        movingEnemy.SetRoomIndex(sessionCol, sessionRow);
                                        sessionArray[sessionRow, sessionCol].monsterList.Add(tempGO);
                                        sessionArray[sessionRow, sessionCol].movingEnemyCount =
                                            sessionArray[sessionRow, sessionCol].monsterList.Count;
                                    }

                                    Destroy(blockTemp);
                                    break;
                                case "NP":
                                    break;
                                default:
                                    break;
                            }
                        }

                        blockListTemp = null;
                        blockTemp = null;
                    }
                }

                if (sessionArray[sessionRow, sessionCol].movingEnemyCount == 0)
                {
                    DoorController tempDoorController = null;
                    foreach (var door in sessionArray[sessionRow, sessionCol].doorsInRoomDic)
                    {
                        tempDoorController = door.Value.transform.Find("Door").GetComponent<DoorController>();
                        if (tempDoorController != null)
                        {
                            tempDoorController.DoorOpen();
                        }
                    }
                }
            }
        }
    }

    void InitDoor(ref List<GameObject> blockListTemp, ref GameObject blockTemp, int sessionRow, int sessionCol, DoorDirInRoom doorDirInRoom, RoomType roomType)
    {
        blockListTemp = tilePrefabList[(int)BlockType.door].tilePrfabs;
        blockTemp = Instantiate<GameObject>(blockListTemp[0]);
        DoorController doorController = blockTemp.transform.Find("Door").GetComponent<DoorController>();
        doorController.doorDir = doorDirInRoom;
        sessionArray[sessionRow, sessionCol].doorsInRoomDic.Add(doorDirInRoom, blockTemp);
        switch (roomType)
        {
            case RoomType.town:
            case RoomType.camp:
            case RoomType.eventRoom:
                doorController.DoorOpen();
                break;
            case RoomType.monster:
            case RoomType.boss:
                break;
            default:
                break;
        }
    }

    public void DecreaseMonsterCountAtRoom(int col, int row)
    {
        sessionArray[row, col].movingEnemyCount--;
        if (sessionArray[row, col].movingEnemyCount <= 0)
        {
            if (sessionArray[row, col].roomType == RoomType.boss)
            {
                HeroController.S.isClearSession = true;
            }
            else
            {
                sessionArray[row, col].movingEnemyCount = 0;
                sessionArray[row, col].monsterList.Clear();
                DoorController tempDoorController = null;
                foreach (var door in sessionArray[row, col].doorsInRoomDic)
                {
                    tempDoorController = door.Value.transform.Find("Door").GetComponent<DoorController>();
                    if (tempDoorController != null)
                    {
                        tempDoorController.DoorOpen();
                    }
                }
            }
        }
    }

    public void OpenMiniMapIcon(int col, int row)
    {
        sessionArray[row, col].miniMapIcon.GetComponent<SpriteRenderer>().sprite =
                    miniMapIconSpites[(int)sessionArray[row, col].roomType];
    }
}
