using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    town,
    camp,
    eventRoom,
    monster,
    boss,
    bridge,
    none
}

public enum DoorDirInRoom
{
    none = 0,
    right = 1,
    down = 2,
    downRight = 3,
    left = 4,
    leftRight = 5,
    downLeft = 6,
    downLeftRight = 7,
    up = 8,
    upRight = 9,
    upDown = 10,
    upDownRight = 11,
    upLeft = 12,
    upLeftRight = 13,
    upDownLeft = 14,
    upDownLeftRight = 15,
    end
}

public struct RoomInfo
{
    public RoomType roomType;
    public DoorDirInRoom doorDir;
    public int pathCost;
    public Vector2 pos;
    public Vector2 size;
    public Dictionary<DoorDirInRoom, GameObject> doorsInRoomDic;
    public List<GameObject> monsterList;
    public int movingEnemyCount;
    public GameObject miniMapIcon;
}

public class SessionMaker : MonoBehaviour
{
    [System.Serializable]
    public struct BasicRoomInfo
    {
        public RoomType roomType;
        public Vector2 pos;
    }

    public static SessionMaker S;

    [Header ("Set in Inspector")]
    public bool debugCheck = true;
    public int minRoomCount = 20;
    public int maxRoomCount = 30;
    public int minEventRoomCount = 2;
    public int maxEventRoomCount = 3;
    public int minCampRoomCount = 2;
    public int maxCampRoomCount = 3;
    public int campRoomMakePahtCost = 6;
    public float makeCycleBirdgeProb = 0.1f;
    public int sessionArrayRowSize = 9;
    public int sessionArrayColSize = 11;
    public Sprite questionSprite;
    public List<BasicRoomInfo> basicRoomList = new List<BasicRoomInfo>();
    public List<Vector2> basicBridgePosList = new List<Vector2>();
    public List<GameObject> roomPrefab = new List<GameObject>();
    private Transform SessionMiniMapAnchorTrans = null;

    [Header("Set Dynamically")]
    public int roomCount = 0;
    public int campRoomCount = 0;
    public int eventRoomCount = 0;
    public int maxPathCost = 0;

    private int roomCountTemp;
    private List<Vector2> dirCrossList= new List<Vector2>();
    private List<Vector2> dirXList = new List<Vector2>();
    private RoomInfo[,] sessionArray = new RoomInfo[0, 0];
    private int initCurPosQuotient = 5;

    private void Awake()
    {
        S = this;
        sessionArray = new RoomInfo[sessionArrayRowSize, sessionArrayColSize];
        dirCrossList.Add(new Vector2(1, 0));
        dirCrossList.Add(new Vector2(0, 1));
        dirCrossList.Add(new Vector2(-1, 0));
        dirCrossList.Add(new Vector2(0, -1));
        dirXList.Add(new Vector2(1, 1));
        dirXList.Add(new Vector2(-1, 1));
        dirXList.Add(new Vector2(-1, -1));
        dirXList.Add(new Vector2(1, -1));
        MakeSession();
    }

    // Use this for initialization
    void Start () {
        SessionMiniMapAnchorTrans = GameObject.Find("SessionMiniMapAnchor").transform;
        DrawSession();
        SessionDrawController.S.SetSessionArray(sessionArray);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void InitSession()
    {
        roomCount = Random.Range(minRoomCount, maxRoomCount);
        roomCountTemp = roomCount;

        for (int row = 0; row < sessionArrayRowSize; row ++)
        {
            for (int col = 0; col < sessionArrayColSize; col ++)
            {
                sessionArray[row, col].roomType = RoomType.none;
                sessionArray[row, col].pathCost = -1;
                sessionArray[row, col].pos = new Vector2(col, row);
                sessionArray[row, col].doorDir = DoorDirInRoom.none;
                sessionArray[row, col].doorsInRoomDic = new Dictionary<DoorDirInRoom, GameObject>();
                sessionArray[row, col].size = Vector2.zero;
                sessionArray[row, col].monsterList = new List<GameObject>();
            }
        }

        foreach (var basicRoom in basicRoomList)
        {
            sessionArray[(int)basicRoom.pos.y, (int)basicRoom.pos.x].roomType = basicRoom.roomType;
            roomCountTemp--;
        }

        foreach (var pos in basicBridgePosList)
        {
            sessionArray[(int)pos.y, (int)pos.x].roomType = RoomType.bridge;
        }
    }

    void MakeSession()
    {
        InitSession();
        int randTemp = 0;
        Vector2 dirTemp;
        int curRow = 0;
        int curCol = 0;

        while (roomCountTemp != 0)
        {
            randTemp = Random.Range(0, dirCrossList.Count);
            dirTemp = dirCrossList[randTemp];
            if (curRow + dirTemp.y * 2 >= 0 && curRow + dirTemp.y * 2 < sessionArrayRowSize &&
                curCol + dirTemp.x * 2 >= 0 && curCol + dirTemp.x * 2 < sessionArrayColSize)
            {
                curRow += (int)dirTemp.y * 2;
                curCol += (int)dirTemp.x * 2;
                if (sessionArray[curRow, curCol].roomType == RoomType.none)
                {
                    sessionArray[curRow, curCol].roomType = RoomType.monster;
                    sessionArray[curRow - (int)dirTemp.y, curCol - (int)dirTemp.x].roomType = RoomType.bridge;
                    roomCountTemp--;
                    if (roomCountTemp % initCurPosQuotient == 0)
                    {
                        curRow = 0;
                        curCol = 0;
                    }
                }
            }
        }

        MakeCycleBridge();
        SetDoorDir();
        CalulatePathCost();
        MakeBossRoom();
        MakeCampRoom();
        MakeEventRoom();
    }

    void MakeCycleBridge()
    {
        for (int row = 0; row < sessionArrayRowSize; row++)
        {
            if (row % 2 == 0)
            {
                for (int col = 1; col < sessionArrayColSize; col+=2)
                {
                    if (Random.value < makeCycleBirdgeProb &&
                        sessionArray[row, col - 1].roomType != RoomType.none &&
                        sessionArray[row, col + 1].roomType != RoomType.none)
                    {
                        sessionArray[row, col].roomType = RoomType.bridge;
                    }
                }
            }
            else
            {
                for (int col = 0; col < sessionArrayColSize; col += 2)
                {
                    if (Random.value < makeCycleBirdgeProb &&
                        sessionArray[row - 1, col].roomType != RoomType.none &&
                        sessionArray[row + 1, col].roomType != RoomType.none)
                    {
                        sessionArray[row, col].roomType = RoomType.bridge;
                    }
                }
            }
        }
    }

    void SetDoorDir()
    {
        int tempRow = 0;
        int tempCol = 0;

        for (int row = 0; row < sessionArrayRowSize; row += 2)
        {
            for (int col = 0; col < sessionArrayColSize; col += 2)
            {
                for (int i = 0; i < dirCrossList.Count; i++)
                {
                    tempRow = row + (int)dirCrossList[i].y;
                    tempCol = col + (int)dirCrossList[i].x;

                    if (tempCol >= 0 &&
                        tempCol < sessionArrayColSize &&
                        tempRow >= 0 &&
                        tempRow < sessionArrayRowSize && 
                        sessionArray[tempRow, tempCol].roomType == RoomType.bridge)
                    {
                        int roomDirTemp = (int)sessionArray[row, col].doorDir;
                        switch (i)
                        {
                            case 0:
                                sessionArray[row, col].doorDir = (DoorDirInRoom)(roomDirTemp + (int)DoorDirInRoom.right);
                                break;
                            case 1:
                                sessionArray[row, col].doorDir = (DoorDirInRoom)(roomDirTemp + (int)DoorDirInRoom.down);
                                break;
                            case 2:
                                sessionArray[row, col].doorDir = (DoorDirInRoom)(roomDirTemp + (int)DoorDirInRoom.left);
                                break;
                            case 3:
                                sessionArray[row, col].doorDir = (DoorDirInRoom)(roomDirTemp + (int)DoorDirInRoom.up);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }

    void CalulatePathCost()
    {
        List<Vector2> rootRoomInfoPosList = new List<Vector2>();
        List<Vector2> nextRootRoomInfoPosList = new List<Vector2>();
        rootRoomInfoPosList.Add(new Vector2(0, 0));
        sessionArray[0, 0].pathCost = 0;
        maxPathCost = 0;

        while (rootRoomInfoPosList.Count != 0)
        {
            nextRootRoomInfoPosList.Clear();
            foreach (var roomInfoPos in rootRoomInfoPosList)
            {
                foreach (var dir in dirCrossList)
                {
                    if (roomInfoPos.x + dir.x >= 0 &&
                        roomInfoPos.x + dir.x < sessionArrayColSize &&
                        roomInfoPos.y + dir.y >= 0 &&
                        roomInfoPos.y + dir.y < sessionArrayRowSize)
                    {
                        if (sessionArray[(int)(roomInfoPos.y + dir.y), (int)(roomInfoPos.x + dir.x)].roomType == RoomType.bridge)
                        {
                            int curPathCost = sessionArray[(int)(roomInfoPos.y + dir.y * 2), (int)(roomInfoPos.x + dir.x * 2)].pathCost;
                            int rootPathCost = sessionArray[(int)roomInfoPos.y, (int)roomInfoPos.x].pathCost;
                            if (curPathCost == -1 || curPathCost > rootPathCost + 1)
                            {
                                sessionArray[(int)(roomInfoPos.y + dir.y * 2), (int)(roomInfoPos.x + dir.x * 2)].pathCost = sessionArray[(int)roomInfoPos.y, (int)roomInfoPos.x].pathCost + 1;
                                nextRootRoomInfoPosList.Add(new Vector2((int)(roomInfoPos.x + dir.x * 2), (int)(roomInfoPos.y + dir.y * 2)));
                                maxPathCost = Mathf.Max(sessionArray[(int)(roomInfoPos.y + dir.y * 2), (int)(roomInfoPos.x + dir.x * 2)].pathCost, maxPathCost);
                            }
                        }
                    }
                }
            }
            rootRoomInfoPosList.Clear();
            foreach (var newRoot in nextRootRoomInfoPosList)
            {
                rootRoomInfoPosList.Add(newRoot);
            }
        }
    }

    void MakeBossRoom()
    {
        int randIdx = 0;
        Vector2 tempPos = new Vector2();
        List<Vector2> bossCandidatePosList = new List<Vector2>();

        for (int row = 0; row < sessionArrayRowSize; row += 2)
        {
            for (int col = 0; col < sessionArrayColSize; col += 2)
            {
                if (sessionArray[row, col].pathCost == maxPathCost)
                {
                    bossCandidatePosList.Add(new Vector2(col, row));
                }
            }
        }


        randIdx = Random.Range(0, bossCandidatePosList.Count);
        tempPos = bossCandidatePosList[randIdx];
        sessionArray[(int)tempPos.y, (int)tempPos.x].roomType = RoomType.boss;
    }

    void MakeCampRoom()
    {
        campRoomCount = Random.Range(minCampRoomCount, maxCampRoomCount + 1);
        int randIdx = 0;
        bool haveNearCamp = false;
        Vector2 tempPos = new Vector2();
        List<Vector2> campCandidatePosList = new List<Vector2>();

        for (int row = 0; row < sessionArrayRowSize; row += 2)
        {
            for (int col = 0; col < sessionArrayColSize; col += 2)
            {
                if (sessionArray[row, col].pathCost >= campRoomMakePahtCost)
                {
                    campCandidatePosList.Add(new Vector2(col, row));
                }
            }
        }

        for (int i = 0; i < campRoomCount; i++)
        {
            randIdx = Random.Range(0, campCandidatePosList.Count);
            tempPos = campCandidatePosList[randIdx];

            haveNearCamp = false;
            foreach (var dirCross in dirCrossList)
            {
                if (tempPos.x + dirCross.x * 2 >= 0 &&
                    tempPos.x + dirCross.x * 2 < sessionArrayColSize &&
                    tempPos.y + dirCross.y * 2 >= 0 &&
                    tempPos.y + dirCross.y * 2 < sessionArrayRowSize)
                {
                    if (sessionArray[(int)(tempPos.y + dirCross.y * 2), (int)(tempPos.x + dirCross.x * 2)].roomType == RoomType.camp)
                    {
                        haveNearCamp = true;
                    }
                }
            }
            foreach (var dirX in dirXList)
            {
                if (tempPos.x + dirX.x * 2 >= 0 &&
                    tempPos.x + dirX.x * 2 < sessionArrayColSize &&
                    tempPos.y + dirX.y * 2 >= 0 &&
                    tempPos.y + dirX.y * 2 < sessionArrayRowSize)
                {
                    if (sessionArray[(int)(tempPos.y + dirX.y * 2), (int)(tempPos.x + dirX.x * 2)].roomType == RoomType.camp)
                    {

                        haveNearCamp = true;
                    }
                }
            }

            if (haveNearCamp || sessionArray[(int)tempPos.y, (int)tempPos.x].roomType != RoomType.monster)
            {
                i--;
            }
            else
            {
                sessionArray[(int)tempPos.y, (int)tempPos.x].roomType = RoomType.camp;
            }

            campCandidatePosList.RemoveAt(randIdx);
            if (campCandidatePosList.Count == 0)
            {
                return;
            }
        }
    }

    void MakeEventRoom()
    {
        eventRoomCount = Random.Range(minEventRoomCount, maxEventRoomCount + 1);
        int tempEventRoomCount = eventRoomCount;
        int randRow = 0;
        int randCol = 0;

        while (tempEventRoomCount != 0)
        {
            randRow = Random.Range(0, sessionArrayRowSize);
            randCol = Random.Range(0, sessionArrayColSize);

            if (sessionArray[randRow, randCol].roomType == RoomType.monster)
            {
                sessionArray[randRow, randCol].roomType = RoomType.eventRoom;
                tempEventRoomCount--;
            }
        }
    }

    void DrawSession()
    {
        GameObject tempGO = null;
        for (int row = 0; row < sessionArrayRowSize; row++)
        {
            for (int col = 0; col < sessionArrayColSize; col++)
            {
                switch (sessionArray[row,col].roomType)
                {
                    case RoomType.town:
                        tempGO = Instantiate<GameObject>(roomPrefab[(int)sessionArray[row, col].roomType]);
                        break;
                    case RoomType.camp:
                    case RoomType.eventRoom:
                    case RoomType.monster:
                    case RoomType.boss:
                        tempGO = Instantiate<GameObject>(roomPrefab[(int)sessionArray[row, col].roomType]);
                        if (!debugCheck)
                        {
                            tempGO.GetComponent<SpriteRenderer>().sprite = questionSprite;
                        }
                        break;
                    case RoomType.bridge:
                        tempGO = Instantiate<GameObject>(roomPrefab[(int)sessionArray[row, col].roomType]);
                        if (col%2 == 0)
                        {
                            tempGO.gameObject.transform.localScale = new Vector3(0.1f, 1.0f, 1.0f);
                        }
                        else
                        {
                            tempGO.gameObject.transform.localScale = new Vector3(1.0f, 0.1f, 1.0f);
                        }
                        break;
                    case RoomType.none:
                        tempGO = null;
                        break;
                    default:
                        break;
                }

                if (tempGO != null)
                {
                    tempGO.transform.SetParent(SessionMiniMapAnchorTrans);
                    tempGO.transform.localPosition = new Vector3(col, -row, 0);
                    sessionArray[row, col].miniMapIcon = tempGO;
                }
            }
        }

        string costs = "";
        for (int row = 0; row < sessionArrayRowSize; row += 2)
        {
            for (int col = 0; col < sessionArrayColSize; col += 2)
            {
                costs += sessionArray[row, col].pathCost + "(" + sessionArray[row, col].doorDir + ")" + "\t";
            }
            costs += "\n";
        }
        print(costs);
    }
}
