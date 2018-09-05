using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieKnight : MovingEnemy
{

    [Header ("Set in Inspector : ZombieKnight")]
    public Transform spriteRendererTrans;
    public Transform attackerAnchorTrans;

    public EnemyState curState
    {
        get
        {
            return _curState;
        }

        set
        {
            _prevState = _curState;
            _curState = value;

            switch (_curState)
            {
                case EnemyState.idle:
                    animator.CrossFade("ZombieKnightIdle", 0);
                    break;

                case EnemyState.patroll:
                    animator.CrossFade("ZombieKnightWalk", 0);
                    break;

                case EnemyState.attack:
                    animator.CrossFade("ZombieKnightAttack", 0);
                    break;

                case EnemyState.knockBack:
                    animator.CrossFade("ZombieKnightKnockBack", 0);
                    break;

                case EnemyState.die:
                    animator.CrossFade("ZombieKnightDie", 0);
                    break;

                default:
                    break;
            }
        }
    }

    public int moveDir
    {
        get
        {
            return _moveDir;
        }

        set
        {
            _moveDir = value;
            if (_moveDir == 1)
            {
                enemySpriteRenderer.flipX = true;
                spriteRendererTrans.transform.localPosition = new Vector3(0.4f, 0.5f, 0.0f);
                attackerAnchorTrans.transform.localPosition = new Vector3(0.72f, 0.5f, 0.0f);
            }
            else
            {
                enemySpriteRenderer.flipX = false;
                spriteRendererTrans.transform.localPosition = new Vector3(-0.4f, 0.5f, 0.0f);
                attackerAnchorTrans.transform.localPosition = new Vector3(-0.72f, 0.5f, 0.0f);
            }

            attacker.attackDir = _moveDir;
        }
    }

    private void Awake()
    {
        enemyRigid = GetComponent<Rigidbody2D>();
        curState = _curState;
        moveDir = _moveDir;
        SetMoveDirChangeTime();
        maxHealth = health;
    }

    void LateUpdate ()
    {
        switch (curState)
        {
            case EnemyState.idle:
                IsFindHero();
                break;
            case EnemyState.patroll:
                Patroll();
                break;
            case EnemyState.attack:
                break;
            case EnemyState.knockBack:
                attacker.OffAttackerColl();
                if (enemyRigid.velocity == Vector2.zero)
                {
                    curState = EnemyState.patroll;
                }
                break;
            case EnemyState.die:
                attacker.OffAttackerColl();
                break;
            default:
                break;
        }
    }

    override public void SetMoveDir(int inputMoveDir)
    {
        moveDir = inputMoveDir;
    }

    override public void SetState(EnemyState enemyState)
    {
        curState = enemyState;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("HeroAttacker"))
        {
            DecreaseHealth(other.transform.parent.GetComponent<Attacker>().GetDamage());
            if (health != 0)
            {
                KnockBack(other.gameObject.transform.root.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawFindField(this.transform.position + enemyEyesPosOffset);
    }
}
