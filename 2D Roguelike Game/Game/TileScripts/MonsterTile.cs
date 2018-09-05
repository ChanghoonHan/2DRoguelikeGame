using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTile : MonoBehaviour {
    
    [Header ("Set in Inspector")]
    public List<GameObject> monsterPrefabList;
    public List<GameObject> bossMonsterPrefabList;

    [Header ("Set Dynamically")]
    public bool Boss = false;

    public GameObject SetMonster()
    {
        GameObject tempGO = null;

        if (Boss)
        {
            int randIdx = Random.Range(0, bossMonsterPrefabList.Count);
            tempGO = Instantiate<GameObject>(bossMonsterPrefabList[randIdx]);
        }
        else
        {
            int randIdx = Random.Range(0, monsterPrefabList.Count);
            tempGO = Instantiate<GameObject>(monsterPrefabList[randIdx]);
        }

        return tempGO;
    }
}
