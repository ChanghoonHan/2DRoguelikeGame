using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CreateMap : MonoBehaviour {

    public enum MapTileType
    {
        wall,
        ground,
        jumpGround,
        doorU,
        box22,
        box33,
        monster,
        tile,
        tileWall,
        lHill,
        rHill,
        item,
        NPC,
        none,
        boxArea,
        doorArea,
        doorD,
        doorL,
        doorR,
    }

    public List<GameObject> blocksList;
    public InputField makeWallInput;
    public InputField filePathInput;
    public InputField templateNameInput;
    public Dropdown mapTileSelector;
    public Dropdown roomTypeSelector;
    public Dropdown roomDoorDirSelector;
    public Dropdown doorDirSelector;
    public int mapWidthCount;
    public int mapHeightCount;
    public MapTileType curSelectMapTile;
    public GameObject followMouseTile;
    public Text errorText;

    private GameObject[,] drawBlocksArray;
    private MapTileType[,] mapTileArray;

    private void Awake ()
    {
        curSelectMapTile = MapTileType.none;
        followMouseTile = null;
        doorDirSelector.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (mapTileArray == null)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Please Make Base Map";
        }
        else
        {
            errorText.gameObject.SetActive(false);
            errorText.text = "";
        }

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                PutTile();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                PutTile();
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                PutTile(true);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                PutTile(true);
            }
        }


        if (followMouseTile != null)
        {
            Vector3 mPos = Input.mousePosition;
            mPos.z = -Camera.main.transform.position.z;
            Vector3 mPosInGame = Camera.main.ScreenToWorldPoint(mPos);
            followMouseTile.transform.position = mPosInGame;
        }
    }

    private void PutTile(bool erase = false)
    {
        if (mapTileArray == null)
        {
            return;
        }

        Vector3 mPos = Input.mousePosition;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 mPosInGame = Camera.main.ScreenToWorldPoint(mPos);

        int width = (int)(mPosInGame.x + 0.5);
        int height = (int)(mPosInGame.y + 0.5);

        if (height <= 0 || height >= mapHeightCount - 1 || width <= 0 || width >= mapWidthCount - 1)
        {
            return;
        }

        if (erase)
        {
            if (mapTileArray[height, width] == MapTileType.boxArea || mapTileArray[height, width] == MapTileType.doorArea)
            {
                return;
            }

            if (mapTileArray[height, width] != MapTileType.none)
            {
                switch (mapTileArray[height, width])
                {
                    case MapTileType.doorU:
                    case MapTileType.doorD:
                    case MapTileType.doorL:
                    case MapTileType.doorR:
                        PutArea(height, width, 3, 2, MapTileType.none);
                        break;
                    case MapTileType.box22:
                        PutArea(height, width, 2, 2, MapTileType.none);
                        break;
                    case MapTileType.box33:
                        PutArea(height, width, 3, 3, MapTileType.none);
                        break;
                    default:
                        break;
                }

                mapTileArray[height, width] = MapTileType.none;
                CreateMapBlock(height, width, mapTileArray[height, width]);
            }
        }
        else
        {
            if (!CheckCanPutBlock(height, width, curSelectMapTile))
            {
                return;
            }

            if (mapTileArray[height, width] != curSelectMapTile)
            {
                switch (curSelectMapTile)
                {
                    case MapTileType.doorU:
                    case MapTileType.doorD:
                    case MapTileType.doorL:
                    case MapTileType.doorR:
                        PutArea(height, width, 3, 2, MapTileType.doorArea);
                        break;
                    case MapTileType.box22:
                        PutArea(height, width, 2, 2, MapTileType.boxArea);
                        break;
                    case MapTileType.box33:
                        PutArea(height, width, 3, 3, MapTileType.boxArea);
                        break;
                    default:
                        break;
                }

                if (curSelectMapTile == MapTileType.doorU)
                {
                    switch (doorDirSelector.value)
                    {
                        case 0:
                            mapTileArray[height, width] = MapTileType.doorU;
                            break;
                        case 1:
                            mapTileArray[height, width] = MapTileType.doorD;
                            break;
                        case 2:
                            mapTileArray[height, width] = MapTileType.doorL;
                            break;
                        case 3:
                            mapTileArray[height, width] = MapTileType.doorR;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    mapTileArray[height, width] = curSelectMapTile;
                }
                CreateMapBlock(height, width, mapTileArray[height, width]);
            }
        }
    }

    public void PutArea(int height, int width, int objHeight, int objWidth, MapTileType tileType)
    {
        for (int iHeight = height; iHeight < height + objHeight; iHeight++)
        {
            for (int iWidth = width; iWidth < width + objWidth; iWidth++)
            {
                if (iHeight == height && iWidth == width)
                {
                    continue;
                }
                mapTileArray[iHeight, iWidth] = tileType;
            }
        }
    }

    public bool CheckCanPutBlock(int height, int width, MapTileType tileType)
    {
        int objHeight = 1;
        int objWidth = 1;

        switch (tileType)
        {
            case MapTileType.doorU:
            case MapTileType.doorD:
            case MapTileType.doorL:
            case MapTileType.doorR:
                objHeight = 3;
                objWidth = 2;
                break;
            case MapTileType.box22:
                objHeight = 2;
                objWidth = 2;
                break;
            case MapTileType.box33:
                objHeight = 3;
                objWidth = 3;
                break;
            default:
                break;
        }

        for (int iHeight = height; iHeight < height + objHeight; iHeight++)
        {
            for (int iWidth = width; iWidth < width + objWidth; iWidth++)
            {
                if (mapTileArray[iHeight, iWidth] != MapTileType.none)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void MakeBaseMap ()
    {
        string mapStr = makeWallInput.text;
        string[] mapStrSplite = mapStr.Split(',');
        if (mapStrSplite.Length < 2)
        {
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            mapStrSplite[i] = mapStrSplite[i].Trim();
        }
        mapWidthCount = int.Parse(mapStrSplite[0]);
        mapHeightCount = int.Parse(mapStrSplite[1]);

        ClearMap();
        mapTileArray = new MapTileType[mapHeightCount, mapWidthCount];
        drawBlocksArray = new GameObject[mapHeightCount, mapWidthCount];
        InitMap();
        for (int height = 0; height < mapHeightCount; height++)
        {
            for (int width = 0; width < mapWidthCount; width++)
            {
                if (height == 0 || height == mapHeightCount - 1 || width == 0 || width == mapWidthCount - 1)
                {
                    mapTileArray[height, width] = MapTileType.wall;
                    CreateMapBlock(height, width, mapTileArray[height, width]);
                }
            }
        }
    }

    public void CreateMapBlock(int height, int width, MapTileType tileType)
    {
        if (mapTileArray == null)
        {
            return;
        }

        if (drawBlocksArray[height, width] != null)
        {
            Destroy(drawBlocksArray[height, width]);
            drawBlocksArray[height, width] = null;
        }

        if (tileType == MapTileType.none)
        {
            return;
        }

        if (tileType == MapTileType.doorU ||
            tileType == MapTileType.doorD ||
            tileType == MapTileType.doorL ||
            tileType == MapTileType.doorR)
        {
            tileType = MapTileType.doorU;
        }
        GameObject tempBlock;
        tempBlock = Instantiate<GameObject>(blocksList[(int)tileType]);
        drawBlocksArray[height, width] = tempBlock;
        Vector3 blockPos = new Vector3(width, height, 0);
        tempBlock.transform.position = blockPos;
    }

    public void InitMap()
    {
        if (mapTileArray == null)
        {
            return;
        }

        for (int height = 0; height < mapHeightCount; height++)
        {
            for (int width = 0; width < mapWidthCount; width++)
            {
                mapTileArray[height, width] = MapTileType.none;
            }
        }
    }

    public void ClearMap()
    {
        if (mapTileArray == null)
        {
            return;
        }

        foreach (var block in drawBlocksArray)
        {
            Destroy(block);
        }
        drawBlocksArray = null;

        for (int height = 0; height < mapHeightCount; height++)
        {
            for (int width = 0; width < mapWidthCount; width++)
            {
                mapTileArray[height, width] = MapTileType.none;
            }
        }
        mapTileArray = null;
    }

    public void ChangeCurMapTile()
    {
        curSelectMapTile = (MapTileType)mapTileSelector.value;
        if ((MapTileType)mapTileSelector.value == MapTileType.doorU ||
            (MapTileType)mapTileSelector.value == MapTileType.doorD ||
            (MapTileType)mapTileSelector.value == MapTileType.doorL ||
            (MapTileType)mapTileSelector.value == MapTileType.doorR)
        {
            doorDirSelector.gameObject.SetActive(true);
        }
        else
        {
            doorDirSelector.gameObject.SetActive(false);
        }

        if (followMouseTile != null)
        {
            Destroy(followMouseTile);
            followMouseTile = null;
        }

        if (curSelectMapTile == MapTileType.none)
        {
            return;
        }

        followMouseTile = Instantiate<GameObject>(blocksList[(int)curSelectMapTile]);
        followMouseTile.transform.position = Vector3.zero;
        followMouseTile.transform.SetParent(followMouseTile.transform);
    }

    public void SaveMapToFile()
    {
        string filePath = "";
        string templateName = "";
        string mapStr = "";
        filePath = filePathInput.text;
        templateName = templateNameInput.text;

        if (filePath == "")
        {
            filePathInput.text = "Please Input File Path";
            return;
        }

        if (templateName == "")
        {
            templateNameInput.text = "Please Input Template Name";
            return;
        }

        List<string> mapStrList = new List<string>();

        for (int height = 0; height < mapHeightCount; height++)
        {
            for (int width = 0; width < mapWidthCount; width++)
            {
                switch (mapTileArray[height, width])
                {
                    case MapTileType.wall:
                        mapStr += "W ";
                        break;
                    case MapTileType.ground:
                        mapStr += "G ";
                        break;
                    case MapTileType.jumpGround:
                        mapStr += "JG ";
                        break;
                    case MapTileType.doorU:
                        mapStr += "DU ";
                        break;
                    case MapTileType.doorD:
                        mapStr += "DD ";
                        break;
                    case MapTileType.doorL:
                        mapStr += "DL ";
                        break;
                    case MapTileType.doorR:
                        mapStr += "DR ";
                        break;
                    case MapTileType.box22:
                        mapStr += "B22 ";
                        break;
                    case MapTileType.box33:
                        mapStr += "B33 ";
                        break;
                    case MapTileType.monster:
                        mapStr += "M ";
                        break;
                    case MapTileType.tile:
                        mapStr += "T ";
                        break;
                    case MapTileType.tileWall:
                        mapStr += "TW ";
                        break;
                    case MapTileType.lHill:
                        mapStr += "LH ";
                        break;
                    case MapTileType.rHill:
                        mapStr += "RH ";
                        break;
                    case MapTileType.item:
                        mapStr += "I ";
                        break;
                    case MapTileType.NPC:
                        mapStr += "NP ";
                        break;
                    case MapTileType.none:
                        mapStr += "N ";
                        break;
                    case MapTileType.boxArea:
                        mapStr += "BA ";
                        break;
                    case MapTileType.doorArea:
                        mapStr += "DA ";
                        break;
                    default:
                        break;
                }
            }

            mapStrList.Add(mapStr);
            mapStr = "";
        }

        using (StreamWriter saveFile = new StreamWriter(filePath, true))
        {
            string roomTypeStr = roomTypeSelector.transform.Find("Label").GetComponent<Text>().text;
            string doorDirStr = roomDoorDirSelector.transform.Find("Label").GetComponent<Text>().text;

            saveFile.WriteLine("#" + roomTypeStr + "_" + doorDirStr + "_" + templateName);
            foreach (var line in mapStrList)
            {
                saveFile.WriteLine(line);
            }
            saveFile.WriteLine("@");
        }
    }

    public void LoadMapFromFile()
    {
        string filePath = "";
        string templateName = "";
        filePath = filePathInput.text;
        templateName = templateNameInput.text;

        if (filePath == "")
        {
            filePathInput.text = "Please Input File Path";
            return;
        }

        if (templateName == "")
        {
            templateNameInput.text = "Please Input Template Name";
            return;
        }

        string[] fileStream = File.ReadAllLines(filePath);
        int mapStartIndex = 0;
        List<string[]> mapTileList = new List<string[]>();

        for (int i = 0; i < fileStream.Length-1; i++)
        {
            if (fileStream[i][0] == '#')
            {
                string roomTypeStr = roomTypeSelector.transform.Find("Label").GetComponent<Text>().text;
                string doorDirStr = roomDoorDirSelector.transform.Find("Label").GetComponent<Text>().text;

                string templateNameFromFile = fileStream[i].TrimStart('#');
                if (templateNameFromFile == roomTypeStr + "_" + doorDirStr + "_" + templateName)
                {
                    mapStartIndex = i + 1;
                }
            }
            else
            {
                continue;
            }
        }

        while (fileStream[mapStartIndex][0] != '@')
        {
            mapTileList.Add(fileStream[mapStartIndex].Split(' '));
            mapStartIndex++;
        }

        ClearMap();
        mapHeightCount = mapTileList.Count;
        mapWidthCount = mapTileList[0].Length-1;
        mapTileArray = new MapTileType[mapHeightCount, mapWidthCount];
        drawBlocksArray = new GameObject[mapHeightCount, mapWidthCount];

        for (int height = 0; height < mapHeightCount; height++)
        {
            for (int width = 0; width < mapWidthCount; width++)
            {
                switch (mapTileList[height][width])
                {
                    case "W":
                        mapTileArray[height, width] = MapTileType.wall;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "G":
                    case "B"://저번 버전에는 B로 썼음
                        mapTileArray[height, width] = MapTileType.ground;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "JG":
                        mapTileArray[height, width] = MapTileType.jumpGround;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "D"://저번 버전에는 D로 썼음
                    case "DU":
                        mapTileArray[height, width] = MapTileType.doorU;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "DD":
                        mapTileArray[height, width] = MapTileType.doorD;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "DL":
                        mapTileArray[height, width] = MapTileType.doorL;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "DR":
                        mapTileArray[height, width] = MapTileType.doorR;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "B22":
                        mapTileArray[height, width] = MapTileType.box22;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "B33":
                        mapTileArray[height, width] = MapTileType.box33;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "M":
                        mapTileArray[height, width] = MapTileType.monster;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "T":
                        mapTileArray[height, width] = MapTileType.tile;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "TW":
                        mapTileArray[height, width] = MapTileType.tileWall;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "LH":
                        mapTileArray[height, width] = MapTileType.lHill;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "RH":
                        mapTileArray[height, width] = MapTileType.rHill;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "I":
                        mapTileArray[height, width] = MapTileType.item;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "NP":
                        mapTileArray[height, width] = MapTileType.NPC;
                        CreateMapBlock(height, width, mapTileArray[height, width]);
                        break;
                    case "N":
                        mapTileArray[height, width] = MapTileType.none;
                        break;
                    case "BA":
                        mapTileArray[height, width] = MapTileType.boxArea;
                        break;
                    case "DA":
                        mapTileArray[height, width] = MapTileType.doorArea;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
