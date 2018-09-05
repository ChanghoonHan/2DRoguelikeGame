using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class MovingEnemy : Enemy 
{
    [Header("Set in Inspector : MovingEnemy")]
    public float walkSpeed = 3;
    public float knockBackMult = 10;
    public float changeDirChangeSecMin = 2f;
    public float changeDirChangeSecMax = 4f;
    public float attackDistanceBetweenHero = 1f;
    public float attackDurationTime = 2f;
    [Range(3.0f, 90.0f)]
    public float heroSearchDegHalf = 30;
    public bool bothSearch = false;
    public float heroSearchRadius = 3;
    public Attacker attacker;
    public Vector3 enemyEyesPosOffset = new Vector3(0, 1, 0);
    public Vector3 checkCliffStartOffset = new Vector3(0, 0, 0);
    public Vector3 checkHillStartOffset = new Vector3(0, -0.5f, 0);

    [Header("Set Dynamically : MovingEnemy")]
    [SerializeField]
    protected int _moveDir = 1;
    public float prevMoveDirChangeTime = 0;
    public float prevAttackTime = 0;
    public float moveDirChangeTime = 0;
    public Transform heroTrans;
    public Rigidbody2D enemyRigid;
    public float heroSearchRayCount = 6;
    public List<int> collRayIdx = new List<int>();
    public float maxHealth;
    public Vector2 roomIdx;

    abstract public void SetMoveDir(int inputMoveDir);

    protected bool IsFindHero()
    {
        Vector2 rayStartPos = this.transform.position + enemyEyesPosOffset;

        heroSearchRayCount = heroSearchDegHalf * 2 / 5;
        float rayDegDuration = heroSearchDegHalf * 2 / (int)heroSearchRayCount;

        Vector2 rayDir;
        RaycastHit2D rayHit;
        float deg;

        collRayIdx.Clear();
        for (int count = 0; count < (int)heroSearchRayCount + 1; count++)
        {
            deg = (heroSearchDegHalf * -1) + (rayDegDuration * count);
            rayDir = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * deg), Mathf.Sin(Mathf.Deg2Rad * deg));
            rayHit = Physics2D.Raycast(rayStartPos, rayDir, heroSearchRadius, 1 << LayerMask.NameToLayer("Hero"));
            if (rayHit.collider != null)
            {
                collRayIdx.Add(count);
                heroTrans = rayHit.collider.transform;
            }
        }

        if (bothSearch)
        {
            for (int count = (int)heroSearchRayCount + 1; count < (int)(heroSearchRayCount + 1) * 2; count++)
            {
                deg = (heroSearchDegHalf * -1) + (rayDegDuration * (count - (heroSearchRayCount + 1)));
                rayDir = new Vector3(-_moveDir * Mathf.Cos(Mathf.Deg2Rad * deg), Mathf.Sin(Mathf.Deg2Rad * deg));
                rayHit = Physics2D.Raycast(rayStartPos, rayDir, heroSearchRadius, 1 << LayerMask.NameToLayer("Hero"));
                if (rayHit.collider != null)
                {
                    collRayIdx.Add(count);
                    heroTrans = rayHit.collider.transform;
                }
            }
        }

        if (collRayIdx.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected void DrawFindField(Vector2 eyeForce)
    {
        Vector2 lineStart;
        Vector2 lineEnd;

        Vector2 rayStartPos = eyeForce;

        heroSearchRayCount = heroSearchDegHalf * 2 / 5;
        float rayDurationDeg = heroSearchDegHalf * 2 / (int)heroSearchRayCount;
        for (int count = 0; count < (int)heroSearchRayCount + 1; count++)
        {
            float deg = ((heroSearchDegHalf * -1) + (rayDurationDeg * count));
            lineEnd = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * deg), Mathf.Sin(Mathf.Deg2Rad * deg)) * heroSearchRadius;
            if (collRayIdx.Contains(count))
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.blue;
            }
            Gizmos.DrawLine(rayStartPos, rayStartPos + lineEnd);
        }

        Gizmos.color = Color.blue;
        for (int deg = (int)heroSearchDegHalf * -1; deg < (int)heroSearchDegHalf; deg++)
        {
            lineStart = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * deg), Mathf.Sin(Mathf.Deg2Rad * deg)) * heroSearchRadius;
            lineEnd = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * (deg + 1)), Mathf.Sin(Mathf.Deg2Rad * (deg + 1))) * heroSearchRadius;
            Gizmos.DrawLine(rayStartPos + lineStart, rayStartPos + lineEnd);
        }

        if (bothSearch)
        {
            for (int count = (int)heroSearchRayCount + 1; count < (int)(heroSearchRayCount + 1) * 2; count++)
            {
                float deg = ((heroSearchDegHalf * -1) + (rayDurationDeg * (count - (heroSearchRayCount + 1))));
                lineEnd = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * deg), Mathf.Sin(Mathf.Deg2Rad * deg)) * heroSearchRadius;
                if (collRayIdx.Contains(count))
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }
                Gizmos.DrawLine(rayStartPos, rayStartPos + lineEnd * new Vector3(-1, 1, 1));
            }

            Gizmos.color = Color.blue;
            for (int deg = (int)heroSearchDegHalf * -1; deg < (int)heroSearchDegHalf; deg++)
            {
                lineStart = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * deg), Mathf.Sin(Mathf.Deg2Rad * deg)) * heroSearchRadius;
                lineEnd = new Vector3(_moveDir * Mathf.Cos(Mathf.Deg2Rad * (deg + 1)), Mathf.Sin(Mathf.Deg2Rad * (deg + 1))) * heroSearchRadius;
                Gizmos.DrawLine(rayStartPos + lineStart * new Vector3(-1, 1, 1), rayStartPos + lineEnd * new Vector3(-1, 1, 1));
            }
        }
    }

    virtual public bool IsCollHill()
    {
        Vector2 rayStartPos = transform.position;
        rayStartPos.y += checkHillStartOffset.y;
        Vector2 rayDir = new Vector3(_moveDir * 0.5f, 0);
        float distance = rayDir.magnitude;

        Debug.DrawRay(rayStartPos, rayDir, Color.red);//Hill Check Ray
        RaycastHit2D rayHit = Physics2D.Raycast(rayStartPos, rayDir, distance,
            1 << LayerMask.NameToLayer("Ground"));

        if (rayHit.collider != null)
        {
            if (rayHit.collider.tag == "Hill")
            {
                return true;
            }
            return false;
        }

        return false;
    }

    virtual public void SetAttackEnd()
    {
        attacker.OffAttackerColl();
        SetState(EnemyState.patroll);
        prevAttackTime = Time.time;
    }

    virtual protected void DecreaseHealth(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    virtual protected void Die()
    {
        SetState(EnemyState.die);
        enemyRigid.simulated = false;
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void DestroyEnemy()
    {
        SessionDrawController.S.DecreaseMonsterCountAtRoom((int)roomIdx.x, (int)roomIdx.y);
        Destroy(gameObject);
    }

    virtual protected void Patroll()
    {
        Vector2 rayStartPos = this.transform.position;
        rayStartPos.x += _moveDir * 0.5f;
        rayStartPos.y += checkCliffStartOffset.y;
        Vector2 rayDir = new Vector3(0, -0.5f) * 3;
        float distance = rayDir.magnitude;

        Debug.DrawRay(rayStartPos, rayDir, Color.red);//Cliff Check Ray
        RaycastHit2D rayHit = Physics2D.Raycast(rayStartPos, rayDir, distance,
            1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("JumpGround"));

        if (IsFindHero() && rayHit.collider != null)
        {
            FollowHero();
        }
        else
        {
            if (Time.time - prevMoveDirChangeTime > moveDirChangeTime ||
            rayHit.collider == null)
            {
                SetMoveDir(_moveDir * -1);
                prevMoveDirChangeTime = Time.time;
                SetMoveDirChangeTime();
            }

            if (IsCollHill())
            {
                enemyRigid.velocity = new Vector2(_moveDir * walkSpeed * 1.5f, enemyRigid.velocity.y);
            }
            else
            {
                enemyRigid.velocity = new Vector2(_moveDir * walkSpeed, enemyRigid.velocity.y);
            }
        }
    }

    virtual protected void FollowHero()
    {
        Vector2 heroPosTemp = heroTrans.position;
        Vector2 enemyPosTemp = transform.position;
        heroPosTemp.y = 0;
        enemyPosTemp.y = 0;

        Vector2 dir = heroPosTemp - enemyPosTemp;
        dir.y = 0;
        SetMoveDir((int)dir.normalized.x);

        if (dir.magnitude < attackDistanceBetweenHero && Time.time - prevAttackTime > attackDurationTime)
        {
            Attack();
        }
        else
        {
            if (IsCollHill())
            {
                enemyRigid.velocity = new Vector2(_moveDir * walkSpeed * 1.5f, enemyRigid.velocity.y);
            }
            else
            {
                enemyRigid.velocity = new Vector2(_moveDir * walkSpeed, enemyRigid.velocity.y);
            }
        }
    }

    virtual protected void Attack()
    {
        SetState(EnemyState.attack);
    }

    protected void SetMoveDirChangeTime()
    {
        moveDirChangeTime = Random.Range(changeDirChangeSecMin, changeDirChangeSecMax);
    }

    virtual protected void KnockBack(GameObject target)
    {
        attacker.OffAttackerColl();
        Vector2 knockBackVec2 = transform.position - target.transform.position;
        knockBackVec2.y = 0;
        knockBackVec2.Normalize();
        enemyRigid.AddForce(knockBackVec2 * knockBackMult, ForceMode2D.Impulse);
        SetState(EnemyState.knockBack);
    }

    public void SetRoomIndex(int col, int row)
    {
        roomIdx.x = col;
        roomIdx.y = row;
    }
}
