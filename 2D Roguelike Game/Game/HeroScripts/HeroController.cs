using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour {

    public static HeroController S;

    public enum HeroState
    {
        idle,
        walk,
        run,
        jump,
        underJump,
        roll,
        guard,
        strongAttack,
        weakAttack,
        knockBack,
        die
    }

    [Header ("Set in Inspector")]
    public float health = 100;
    public float maxWalkSpeed = 4f;
    public float maxRunSpeed = 8f;
    public float walkAddedSpeed = 4f;
    public float breakForce = 0.8f;
    public float jumpForce = 10f;
    public float knockBackMult = 2f;
    public float knockBackWithGuardMult = 1.3f;
    public float strongAttackDashForce = 10f;
    [Range (0.1f, 5.0f)]
    public float strongAttackDuration = 0.1f;
    public float weakAttackDashForce = 10f;
    public float roolDashForce = 10f;
    public float doubleClickDurationTime = 0.3f;
    public float weakAttackComboDurationTime = 0.3f;
    public float rollDurationTime = 0.1f;
    public float defensivePower = 0.3f;
    public bool notKnockBackCollWithEnemy = true;
    public SpriteRenderer heroSpriteRenderer;
    public Animator heroAnimator;
    public Collider2D footColl;
    public GameObject footTrigger;
    public Transform attackerAnchor;
    public Attacker attakerScript;
    public BarUI hpBarUI;
    public HeroEffector heroEffector;

    [Header("Set Dynamically")]
    [SerializeField]
    protected int _moveDir = 1;
    public float maxHealth = 0;
    public float curAddSpeed = 0f;
    public float curMaxSpeed = 0f;
    public float lastClickTime = 0f;
    public float lastRollEndTime = 0f;
    public float lastClickWeakAttackKeyTime = 0f;
    public float strongAttackEndTime = 0f;
    public float jumpXForce = 0f;
    public bool canUnderJump = false;
    public bool dieAnimationEnd = false;
    public bool knockBackFirstUpdate = false;
    public bool weakAttackCombo = false;
    public bool isClearSession = false;
    public Vector2 knockBackFroce;
    public KeyCode lastKey = KeyCode.None;
    public Vector2 checkCurVel;
    public Collider2D heroCapColl;
    [SerializeField]
    private HeroState _curState = HeroState.idle;
    [SerializeField]
    private HeroState _prevState = HeroState.idle;
    [SerializeField]
    private GameObject prevCollGo = null;

    private Rigidbody2D heroRigidBody;

    public int moveDir
    {
        get
        {
            return _moveDir;
        }

        set
        {
            if (value > 0)
            {
                _moveDir = 1;
                Vector3 tempPos = attackerAnchor.localPosition;
                tempPos.x = 0.6f;
                attackerAnchor.localPosition = tempPos;
                heroSpriteRenderer.flipX = false;
            }
            else if (value < 0)
            {
                _moveDir = -1;
                Vector3 tempPos = attackerAnchor.localPosition;
                tempPos.x = -0.6f;
                attackerAnchor.localPosition = tempPos;
                heroSpriteRenderer.flipX = true;
            }
            attakerScript.attackDir = _moveDir;
        }
    }

    public HeroState curState
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
                case HeroState.idle:
                    heroAnimator.CrossFade("Idle", 0);
                    break;
                case HeroState.walk:
                    heroAnimator.CrossFade("Walk", 0);
                    break;
                case HeroState.run:
                    heroAnimator.CrossFade("Run", 0);
                    break;
                case HeroState.jump:
                    heroAnimator.CrossFade("Jump", 0);
                    break;
                case HeroState.underJump:
                    heroAnimator.CrossFade("UnderJump", 0);
                    break;
                case HeroState.roll:
                    heroAnimator.CrossFade("Roll", 0);
                    break;
                case HeroState.guard:
                    heroAnimator.CrossFade("Guard", 0);
                    break;
                case HeroState.strongAttack:
                    heroAnimator.CrossFade("Attack1", 0);
                    break;
                case HeroState.weakAttack:
                    if (weakAttackCombo)
                    {
                        heroAnimator.CrossFade("Attack3", 0);
                    }
                    else
                    {
                        heroAnimator.CrossFade("Attack2", 0);
                    }
                    break;
                case HeroState.knockBack:
                    if (_prevState == HeroState.guard)
                    {
                        heroAnimator.CrossFade("KnockBackWithGuard", 0);
                    }
                    else
                    {
                        heroAnimator.CrossFade("KnockBack", 0);
                    }
                    break;
                case HeroState.die:
                    heroAnimator.CrossFade("Die", 0);
                    break;
                default:
                    break;
            }
        }
    }

    private void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        maxHealth = health;
        curAddSpeed = 0f;
        curMaxSpeed = maxWalkSpeed;
        curState = HeroState.idle;
        moveDir = 1;
        canUnderJump = false;
        heroRigidBody = GetComponent<Rigidbody2D>();
        heroCapColl = GetComponent<CapsuleCollider2D>();
        attakerScript = attackerAnchor.GetComponent<Attacker>();
        footTrigger.SetActive(false);
        hpBarUI.SetInit(health);
    }

    // Update is called once per frame
    void LateUpdate() {
        if (isClearSession)
        {
            FadeInOut.S.fadeSpeed = 100;
            FadeInOut.S.fadeOut = true;
            if (FadeInOut.S.fadeOutFinish)
            {
                FadeInOut.S.SetEndScene(true);
                FaustSceneManager.S.SceneChange("MainGameScene", 3);
            }

            return;
        }

        if (curState == HeroState.die)
        {
            if (dieAnimationEnd || FadeInOut.S.fadeOutFinish)
            {
                FadeInOut.S.fadeSpeed = 100;
                FadeInOut.S.fadeOut = true;
                if (FadeInOut.S.fadeOutFinish)
                {
                    FadeInOut.S.SetEndScene(false);
                    FaustSceneManager.S.SceneChange("MainGameScene", 3);
                }
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            IncreaseHealth(10);
        }

        if (Input.GetKeyDown(KeyCode.W))//DEBUG!!!!!!!!!!!
        {
            Vector2 bossRoomIdx = SessionDrawController.S.bossRoomIdx;
            GameObject nextRoomDoorGO = null;
            foreach (var item in SessionDrawController.S.GetSessionArray()[(int)bossRoomIdx.y, (int)bossRoomIdx.x].doorsInRoomDic)
            {
                if (item.Value != null)
                {
                    nextRoomDoorGO = item.Value;
                    break;
                }
            }
                
            Vector3 doorPosTemp = nextRoomDoorGO.transform.position;
            doorPosTemp.x += 0.5f;
            transform.position = doorPosTemp;
            MiniMapCamFollow.S.SetCamPos(bossRoomIdx);
            MainCameraController.S.SetRoom((int)bossRoomIdx.y, (int)bossRoomIdx.x);
            SessionDrawController.S.ChangeMonsterState(bossRoomIdx, Enemy.EnemyState.patroll);
        }

        checkCurVel = heroRigidBody.velocity;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CheckDoublcClick(KeyCode.RightArrow, Time.time);
            curAddSpeed += walkAddedSpeed;
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            curAddSpeed -= walkAddedSpeed;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CheckDoublcClick(KeyCode.LeftArrow, Time.time);
            curAddSpeed -= walkAddedSpeed;
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            curAddSpeed += walkAddedSpeed;
        }

        if (curState != HeroState.jump && curState != HeroState.roll)
        {
            if (curAddSpeed > 0)
            {
                moveDir = 1;
            }
            else if (curAddSpeed < 0)
            {
                moveDir = -1;
            }
        }

        if (!FadeInOut.S.fadeInFinish)
        {
            return;
        }

        if (curState == HeroState.idle)
        {
            if (curAddSpeed != 0)
            {
                curState = HeroState.walk;
            }
        }

        if (curState == HeroState.idle || curState == HeroState.guard)
        {
            heroRigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            heroRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (Input.GetKeyDown(KeyCode.C) &&
            (curState == HeroState.idle ||
            curState == HeroState.walk ||
            curState == HeroState.run ||
            curState == HeroState.guard))
        {
            if (curState == HeroState.guard && canUnderJump)
            {
                curState = HeroState.underJump;
                footColl.isTrigger = true;
            }
            else
            {
                curState = HeroState.jump;
                jumpXForce = heroRigidBody.velocity.x;
                heroRigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                footColl.isTrigger = true;
            }
        }

        if (Input.GetKey(KeyCode.DownArrow) &&
            (curState == HeroState.idle ||
            curState == HeroState.walk ||
            curState == HeroState.run))
        {
            curState = HeroState.guard;
        }

        if (Input.GetKeyDown(KeyCode.X) &&
            Time.time - strongAttackEndTime > strongAttackDuration &&
            (curState == HeroState.idle ||
            curState == HeroState.walk ||
            curState == HeroState.run ||
            curState == HeroState.jump))
        {
            curState = HeroState.strongAttack;
        }

        if (Input.GetKeyDown(KeyCode.Z) &&
            (curState == HeroState.idle ||
            curState == HeroState.walk ||
            curState == HeroState.run ||
            curState == HeroState.jump))
        {
            if (!weakAttackCombo && (Time.time - lastClickWeakAttackKeyTime) < weakAttackComboDurationTime)
            {
                weakAttackCombo = true;
            }
            curState = HeroState.weakAttack;
        }

        if (Time.time - lastRollEndTime > rollDurationTime &&
            Input.GetKeyDown(KeyCode.V) &&
            (curState == HeroState.idle ||
            curState == HeroState.walk ||
            curState == HeroState.run))
        {
            heroCapColl.isTrigger = true;
            curState = HeroState.roll;
        }

        Vector3 curVelocity = heroRigidBody.velocity;
        switch (curState)
        {
            case HeroState.idle:
                footColl.isTrigger = false;
                break;

            case HeroState.walk:
            case HeroState.run:
                footColl.isTrigger = false;
                if (curAddSpeed == 0 &&
                    (Mathf.Abs(heroRigidBody.velocity.x) <= breakForce * curMaxSpeed ||
                    heroRigidBody.velocity.y < 0))
                {
                    heroRigidBody.velocity = new Vector2(0, curVelocity.y);
                    curState = HeroState.idle;
                    curMaxSpeed = maxWalkSpeed;
                }
                else
                {
                    Vector2 rayStartPos = transform.position;
                    rayStartPos.y -= 0.5f;
                    Vector2 rayDir = new Vector3(moveDir * 0.5f, 0);
                    float distance = rayDir.magnitude;

                    Debug.DrawRay(rayStartPos, rayDir, Color.red);//Hill Check Ray
                    RaycastHit2D rayHit = Physics2D.Raycast(rayStartPos, rayDir, distance,
                        1 << LayerMask.NameToLayer("Ground"));

                    curVelocity.x += curAddSpeed * Time.deltaTime;

                    if (curState == HeroState.walk && rayHit.collider != null && rayHit.collider.tag == "Hill")
                    {
                        heroRigidBody.velocity = new Vector2(moveDir * maxWalkSpeed * 1.5f, curVelocity.y);
                    }
                    else
                    {
                        heroRigidBody.velocity = new Vector2(curVelocity.x, curVelocity.y);
                        ClampSpeed(false);
                    }
                }
                
                break;

            case HeroState.jump:
                if (curVelocity.y < 1)
                {
                    footTrigger.SetActive(true);
                }

                if (footTrigger.activeSelf && curVelocity.y == 0)
                {
                    SetJumpEnd();
                }

                heroRigidBody.AddForce(new Vector2(jumpXForce, 0));
                ClampSpeed(true);
                break;

            case HeroState.roll:
                heroRigidBody.velocity = new Vector2(moveDir * roolDashForce, curVelocity.y);
                break;

            case HeroState.guard:
                footColl.isTrigger = false;
                if (!Input.GetKey(KeyCode.DownArrow))
                {
                    curState = HeroState.idle;
                }
                break;

            case HeroState.strongAttack:
                if (_prevState == HeroState.jump && curVelocity.y < 1)
                {
                    footTrigger.SetActive(true);
                }

                if (_prevState == HeroState.jump)
                {
                    heroRigidBody.AddForce(new Vector2(jumpXForce, 0));
                }
                else
                {
                    heroRigidBody.velocity = new Vector2(moveDir * strongAttackDashForce, curVelocity.y);
                }

                ClampSpeed(true);
                break;

            case HeroState.weakAttack:
                if (_prevState == HeroState.jump && curVelocity.y < 1)
                {
                    footTrigger.SetActive(true);
                }

                if (_prevState == HeroState.jump)
                {
                    heroRigidBody.AddForce(new Vector2(jumpXForce, 0));
                }
                else
                {
                    heroRigidBody.velocity = new Vector2(moveDir * strongAttackDashForce, curVelocity.y);
                }

                ClampSpeed(true);
                break;

            case HeroState.knockBack:
                attakerScript.OffAttackerColl();
                if (knockBackFirstUpdate)
                {
                    heroRigidBody.velocity = Vector2.zero;
                    heroRigidBody.AddForce(knockBackFroce, ForceMode2D.Impulse);
                    knockBackFirstUpdate = false;
                }
                else
                {
                    if (heroRigidBody.velocity == Vector2.zero && prevCollGo != null)
                    {
                        SetEndState();
                        prevCollGo = null;
                    }
                }
                break;

            default:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (curState == HeroState.knockBack || health == 0)
        {
            return;
        }

        GameObject tempGo = collision.gameObject;
        if (notKnockBackCollWithEnemy && tempGo.layer != LayerMask.NameToLayer("Obstacles"))
        {
            print(tempGo.name);
            return;
        }

        if (tempGo.layer == LayerMask.NameToLayer("Enemy") || tempGo.layer == LayerMask.NameToLayer("Obstacles"))
        {
            if (tempGo == prevCollGo)
            {
                return;
            }

            prevCollGo = tempGo;

            Enemy enemy = tempGo.GetComponent<Enemy>();
            DecreaseHealth(enemy.GetDamage());
            if (health != 0)
            {
                KnockBack(tempGo);
                curState = HeroState.knockBack;
            }
            else
            {
                KnockBack(tempGo);
                Die();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    { 
        GameObject tempGo = collision.gameObject;

        if (tempGo.layer == LayerMask.NameToLayer("Enemy"))
        {
            Vector3 knockBackDir = transform.position - collision.transform.position;
            knockBackDir.Normalize();
            knockBackDir.y = 0;
            if (knockBackDir.x > 0)
            {
                knockBackDir.x += 2;
            }
            else
            {
                knockBackDir.x -= 2;
            }
            heroRigidBody.AddForce(knockBackDir * 1, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject tempGo = collision.gameObject;

        if (tempGo.layer == LayerMask.NameToLayer("Item"))
        {
            switch (tempGo.tag)
            {
                case "HealItem":
                    HealItem healItem = tempGo.GetComponent<HealItem>();
                    IncreaseHealth(healItem.healSize);
                    Destroy(healItem.gameObject);
                    break;
                default:
                    break;
            }
        }

        if (curState == HeroState.knockBack)
        {
            return;
        }

        if (tempGo.layer == LayerMask.NameToLayer("EnemyAttacker"))
        {
            if (tempGo == prevCollGo)
            {
                return;
            }

            prevCollGo = tempGo.transform.root.gameObject;

            Attacker enemyAttacker = tempGo.transform.parent.GetComponent<Attacker>();
            DecreaseHealth(enemyAttacker.GetDamage());
            if (health != 0)
            {
                tempGo = tempGo.transform.root.gameObject;
                KnockBack(tempGo);
                curState = HeroState.knockBack;
            }
            else
            {
                Die();
            }
        }
    }

    void KnockBack(GameObject other)
    {
        attakerScript.OffAttackerColl();
        Vector2 knockBackDir = transform.position - other.transform.position;
        knockBackDir.y = 0;
        knockBackDir.Normalize();

        knockBackFirstUpdate = true;
        if (curState == HeroState.guard)
        {
            knockBackFroce = knockBackDir * knockBackWithGuardMult;
        }
        else
        {
            knockBackFroce = knockBackDir * knockBackMult;
        }
    }

    void CheckDoublcClick(KeyCode keyCode, float curTime)
    {
        if (curState == HeroState.run && lastKey != keyCode)
        {
            curMaxSpeed = maxRunSpeed;
            curState = HeroState.run;
        }
        else
        {
            if (curState == HeroState.walk || curState == HeroState.idle)
            {
                if (lastKey == keyCode && curTime - lastClickTime < doubleClickDurationTime)
                {
                    curMaxSpeed = maxRunSpeed;
                    curState = HeroState.run;
                }
                else
                {
                    curMaxSpeed = maxWalkSpeed;
                    curState = HeroState.walk;
                }
            }
        }

        lastKey = keyCode;
        lastClickTime = Time.time;
    }

    void ClampSpeed(bool xOnly)
    {
        Vector3 tempVel = heroRigidBody.velocity;

        if(xOnly)
        {
            if (Mathf.Abs(tempVel.x) > curMaxSpeed)
            {
                heroRigidBody.velocity = new Vector2(moveDir * curMaxSpeed, tempVel.y);
            }
        }
        else
        {
            float x = tempVel.x;
            float y = tempVel.y;

            if (Mathf.Abs(tempVel.x) > curMaxSpeed)
            {
                x = moveDir * curMaxSpeed;
            }

            if (tempVel.y > 3)
            {
                y = 3;
            }

            heroRigidBody.velocity = new Vector2(x, y);
        }
    }

    public void SetAttackEnd()
    {
        if (curState == HeroState.weakAttack && !weakAttackCombo)
        {
            lastClickWeakAttackKeyTime = Time.time;
        }
        else
        {
            weakAttackCombo = false;
        }

        if (curState == HeroState.strongAttack)
        {
            strongAttackEndTime = Time.time;
        }
        attakerScript.OffAttackerColl();
        SetEndState();
    }

    public void SetJumpEnd()
    {
        if (curState != HeroState.jump && curState != HeroState.underJump)
        {
            return;
        }

        SetEndState();
    }

    public void SetRollEnd()
    {
        lastRollEndTime = Time.time;
        heroCapColl.isTrigger = false;
        SetEndState();
    }

    public void SetEndState()
    {
        if (curAddSpeed == 0)
        {
            curState = HeroState.idle;
        }
        else if (curMaxSpeed == maxWalkSpeed)
        {
            curState = HeroState.walk;
        }
        else
        {
            curState = HeroState.run;
        }
    }

    public void SetCanUnderJump (bool canUJ)
    {
        canUnderJump = canUJ;
    }

    public HeroState GetCurState()
    {
        return curState;
    }
    
    public void SetRoomChange()
    {
        attakerScript.OffAttackerColl();
        curState = HeroState.idle;
        heroCapColl.isTrigger = false;
    }

    public void Die()
    {
        curState = HeroState.die;
        dieAnimationEnd = false;
        heroCapColl.isTrigger = true;
    }

    public void DieEnd()
    {
        dieAnimationEnd = true;
    }

    public void DecreaseHealth(int demage)
    {
        if (curState == HeroState.guard)
        {
            health -= demage * (1 - defensivePower);
        }
        else
        {
            health -= demage;
        }


        if (health <= 0)
        {
            health = 0;
        }

        hpBarUI.SetBarCurValue(health);
    }

    public void IncreaseHealth(float healSize)
    {
        heroEffector.PlayEffect(HeroEffector.EffectType.Heal);
        health += healSize;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        hpBarUI.SetBarCurValue(health);
    }
}
