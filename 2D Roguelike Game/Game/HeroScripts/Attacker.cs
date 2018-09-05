using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : Item
{
    public enum AttackerType
    {
        weak,
        strong
    }

    [System.Serializable]
    public struct AttackerInfo
    {
        public int damage;
        public Vector3 originPos;
        public GameObject attacker;
    }

    public int curDamage = 0;
    public int attackDir = 1;

    [Header("Set in Inspector")]
    public List<AttackerInfo> weakAttackerTransList;
    public List<AttackerInfo> strongAttackerTransList;

    [Header("Set Dynamically")]
    public AttackerType curAttackerType = AttackerType.weak;

    public void StrongAttack(int idx)
    {
        curAttackerType = AttackerType.strong;
        curDamage = strongAttackerTransList[idx].damage;
        strongAttackerTransList[idx].attacker.SetActive(true);

        Vector3 tempOriginPos = strongAttackerTransList[idx].originPos;
        tempOriginPos.x *= attackDir;
        strongAttackerTransList[idx].attacker.transform.localPosition = tempOriginPos;
    }

    public void WeakAttack(int idx)
    {
        curAttackerType = AttackerType.weak;
        curDamage = weakAttackerTransList[idx].damage;
        weakAttackerTransList[idx].attacker.SetActive(true);

        Vector3 tempOriginPos = weakAttackerTransList[idx].originPos;
        tempOriginPos.x *= attackDir;
        weakAttackerTransList[idx].attacker.transform.localPosition = tempOriginPos;
    }

    public void OffAttackerColl()
    {
        foreach (var attackerInfo in weakAttackerTransList)
        {
            attackerInfo.attacker.SetActive(false);
        }

        foreach (var attackerInfo in strongAttackerTransList)
        {
            attackerInfo.attacker.SetActive(false);
        }
    }

    public int GetDamage()
    {
        return curDamage;
    }
}
