using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTile : MonoBehaviour {
    
    [Header ("Set in Inspector")]
    public List<GameObject> itemPrefabList;

    public GameObject SetItem()
    {
        GameObject tempGO = null;

        int randIdx = Random.Range(0, itemPrefabList.Count);
        tempGO = Instantiate<GameObject>(itemPrefabList[randIdx]);

        return tempGO;
    }
}
