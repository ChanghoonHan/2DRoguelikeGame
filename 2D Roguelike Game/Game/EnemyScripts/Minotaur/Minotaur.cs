using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minotaur : MovingEnemy
{
    [Header("Set in Inspector : Minotaur")]
    public Transform spriteRendererTrans;
    public Transform attackerAnchorTrans;
    public float attack2MoveSpeedMult = 0.3f;
    public float attack3DashFroceMult = 1.5f;
    public float attack1DistanceBetweenHero = 1.5f;
    public float attack2DistanceBetweenHero = 3.0f;
    public float attack3DistanceBetweenHero = 2.0f;
    public int attack2CountPerTen = 2;
    public int attack3CountPerTen = 3;
    public float pattern1HealthPersent = 0.75f;
    public float pattern2HealthPersent = 0.50f;

    [Header("Set Dynamically : Minotaur")]
    public int attackNum = 1;
    public List<int> attackTypeList;
    public bool firstUpdate = false;

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
            this.gameObject.layer = LayerMask.NameToLayer("Enemy");
            switch (_curState)
            {
                case EnemyState.idle:
                    animator.CrossFade("MinotaurIdle", 0);
                    break;

                case EnemyState.patroll:
                    animator.CrossFade("MinotaurWalk", 0);
                    break;

                case EnemyState.attack:
                    switch (attackNum)
                    {
                        case 1:
                            animator.CrossFade("MinotaurAttack1", 0);
                            break;
                        case 2:
                            animator.CrossFade("MinotaurAttack2", 0);
                            break;
                        case 3:
                            this.gameObject.layer = LayerMask.NameToLayer("EnemyNotCollWithHero");
                            animator.CrossFade("MinotaurAttack3", 0);
                            enemyRigid.AddForce(new Vector2(moveDir * attack3DashFroceMult, 0), ForceMode2D.Impulse);
                            break;
                        default:
                            break;
                    }
                    break;

                case EnemyState.knockBack:
                    animator.CrossFade("MinotaurKnockBack", 0);
                    break;

                case EnemyState.die:
                    animator.CrossFade("MinotaurDie", 0);
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
                attackerAnchorTrans.transform.localPosition = new Vector3(1f, -0.3f, 0.0f);
            }
            else
            {
                enemySpriteRenderer.flipX = false;
                attackerAnchorTrans.transform.localPosition = new Vector3(-1f, -0.3f, 0.0f);
            }
            attacker.attackDir = _moveDir;
        }
    }

    override public void SetMoveDir(int inputMoveDir)
    {
        moveDir = inputMoveDir;
    }

    private void Awake()
    {
        enemyRigid = GetComponent<Rigidbody2D>();
        curState = _curState;
        moveDir = _moveDir;
        SetMoveDirChangeTime();
        maxHealth = health;
        for (int i = 0; i < 10; i++)
        {
            attackTypeList.Add(1);
        }
    }

    void LateUpdate()
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
                switch (attackNum)
                {
                    case 2:
                        enemyRigid.velocity = new Vector2(moveDir * attack2MoveSpeedMult, 0);
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
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

    override public void SetState(EnemyState enemyState)
    {
        curState = enemyState;
    }

    public override void SetAttackEnd()
    {
        base.SetAttackEnd();
        this.gameObject.layer = LayerMask.NameToLayer("Enemy");
        int attackListRandIdx = Random.Range(0, attackTypeList.Count);
        int attackRandNum = attackTypeList[attackListRandIdx];
        switch (attackRandNum)
        {
            case 1:
                attackDistanceBetweenHero = attack1DistanceBetweenHero;
                break;
            case 2:
                attackDistanceBetweenHero = attack2DistanceBetweenHero;
                break;
            case 3:
                attackDistanceBetweenHero = attack3DistanceBetweenHero;
                break;
            default:
                break;
        }

        attackNum = attackRandNum;
    }

    override protected void DecreaseHealth(int damage)
    {
        base.DecreaseHealth(damage);

        attackTypeList.Clear();
        if (health / maxHealth > pattern1HealthPersent)
        {
            for (int i = 0; i < 10; i++)
            {
                attackTypeList.Add(1);
            }
        }
        else if (health / maxHealth > pattern2HealthPersent)
        {
            for (int i = 0; i < attack3CountPerTen; i++)
            {
                attackTypeList.Add(3);
            }

            for (int i = 0; i < 10 - attack3CountPerTen; i++)
            {
                attackTypeList.Add(1);
            }
        }
        else
        {
            for (int i = 0; i < attack3CountPerTen; i++)
            {
                attackTypeList.Add(3);
            }

            for (int i = 0; i < attack2CountPerTen; i++)
            {
                attackTypeList.Add(2);
            }

            for (int i = 0; i < 10 - attack3CountPerTen - attack2CountPerTen; i++)
            {
                attackTypeList.Add(1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("HeroAttacker"))
        {
            DecreaseHealth(other.transform.parent.GetComponent<Attacker>().GetDamage());
            attacker.OffAttackerColl();
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
