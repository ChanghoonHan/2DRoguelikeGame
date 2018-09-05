using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public enum EnemyState
    {
        idle,
        patroll,
        attack,
        knockBack,
        die,
    }

    [Header ("Set in Inspector : Enemy")]
    public int colDamage = 1;
    public int health = 0;
    public Animator animator;
    public SpriteRenderer enemySpriteRenderer;

    [Header("Set Dynamically : Enemy")]
    [SerializeField]
    protected EnemyState _curState = EnemyState.idle;
    [SerializeField]
    protected EnemyState _prevState = EnemyState.idle;

    public int GetDamage()
    {
        return colDamage;
    }

    virtual public void SetState(EnemyState enemyStates)
    {

    }
}
