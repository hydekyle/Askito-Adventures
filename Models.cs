using System;
using System.Collections;
using UnityEngine;
using Assets.FantasyHeroes.Scripts;

[Serializable]
public struct Item
{
    public Sprite sprite;
    public string name;
    public string description;
}

[Serializable]
public struct Stats
{
    public int velocity, strength, life;
}

[Serializable]
public struct Weapon
{
    public string name;
    public Sprite sprite;
    public WeaponType type;
    public WeaponSpecial special;
    public int damage, longitude, radius;

}

public enum BodyLimb { Head, LegLeft, LegRight, ArmLeft, ArmRight }
public enum Status { Alive, Dead }
public enum BehaviorIA { Aggressive, Normal, Defensive }
public enum TypeIA { Normal, Dodger, Caster }

[Serializable]
public abstract class Entity
{
    public int ID;
    public Status status = Status.Dead;

    public Character character;
    public Weapon weapon;
    public Transform transform;
    public Stats stats;
    public Animator AttackAnimator;
    public Rigidbody2D rigidbody;
    public LayerMask enemyMask;
    public Vector2 attackDirection = Vector2.right;
    public string name;
    public bool isActive = false;
    //public float padVibration = 0f;

    public float lastTimeDash = 0f;
    float lastTimeAttack = 0f;
    float lastTimeAttackCombo = 0f;
    float lastTimeAttackHit = 0f;
    float dashCD = 0.5f;
    float attackCD = 0.6f;

    public bool isPlayer = false;

    public Transform headT, armLeftT, armRightT, legLeftT, legRightT;

    public abstract void Die();

    public void EquipWeapon(Weapon newWeapon)
    {
        weapon = newWeapon;
        this.character.WeaponType = this.weapon.type;
        character.SetWeaponSprite(newWeapon.sprite);
    }

    public void Spawn(Vector2 position, Stats newStats)
    {
        this.stats = newStats;
        this.transform.position = position;
        this.status = Status.Alive;
        this.character.isActive = true;
        this.ClampMyself(false, true);
        this.transform.gameObject.SetActive(true);
    }

    public abstract void GetStrike(int strikeForce, Vector2 hitDir);

    public void SaveTransformReferences()
    {
        Transform pelvisT = transform.Find("Animation").Find("Pelvis");
        Transform torsoT = pelvisT.Find("Torso");
        headT = torsoT.Find("Head");
        armLeftT = torsoT.Find("ArmL");
        armRightT = torsoT.Find("ArmR");
        legLeftT = pelvisT.Find("LegL");
        legRightT = pelvisT.Find("LegR");
    }

    public abstract void Update();

    public void CastAttack(Vector2 attackDir)
    {
        SoundManager.Instance.PlayAttack();
        if (this.status == Status.Alive)
        {
            var raycastHit = Physics2D.CircleCastAll(
            transform.position + transform.right,
            0.1f + 0.1f * weapon.radius,
            attackDir,
            0.1f + 0.1f * weapon.longitude
        );
            GameManager.Instance.ResolveHits(
                this,
                raycastHit,
                attackDir,
                enemyMask
            );
        }
    }

    public bool extraAction = false;
    int comboCounter = 0;
    public void Attack(Vector2 attackDir)
    {
        //if (GetType() == typeof(Enemy)) EnemiesManager.Instance.ImWaitingForNextAction((Enemy)this);

        if (IsAttackAvailable() && !IsCounterAttacking())
        {
            extraAction = true;
            comboCounter = 0;
            lastTimeAttack = Time.time;
            SlashAttack(attackDir, 0.66f);
            PlayAnim("Attack");
        }
        else if (IsComboAttackAvailable() && extraAction)
        {
            comboCounter++;
            lastTimeAttackHitWhenCombo = lastTimeAttackHit;
            lastTimeAttackCombo = lastTimeAttack = Time.time;
            SlashAttack(attackDir, 0.2f);
            PlayAnim(comboCounter % 2 == 0 ? "Attack" : "AttackCombo");
            SoundManager.Instance.PlayComboSuccess();
        }
        else if (comboCounter > 0)
        {
            // Si vuelves a atacar sin haber golpeado (El Spam no mola)
            if (isPlayer) SoundManager.Instance.PlayComboFailure();
            comboCounter = 0;
            extraAction = false;
        }

    }

    float lastTimeCounterAttack = 0f;

    public void PrepareCounterAttack()
    {
        if (!IsCounterAttacking())
        {
            lastTimeCounterAttack = Time.time;
            PlayAnim("Cast1H");
        }
    }

    public void CounterAttack(Vector2 attackDir)
    {
        // lastTimeAttack = Time.time;
        lastTimeDash = Time.time;
        rigidbody.AddForce(attackDir);
        SetOrientation(attackDir.x);
        CastAttack(attackDir);
        character.Animator.Play("AttackCounter");
    }

    void SlashAttack(Vector2 attackDir, float impulseForce)
    {
        if (Mathf.Abs(attackDir.x) > 0.0f) SetOrientation(attackDir.x);
        attackDirection = attackDir == Vector2.zero ? (Vector2)transform.right : attackDir;
        ApplyImpulse(attackDir * impulseForce);
    }

    public void AttackDash()
    {
        Vector2 direction = rigidbody.velocity;
        if (Mathf.Abs(direction.x) > 0.0f) SetOrientation(direction.x);
        lastTimeAttack = Time.time;
        ApplyImpulse(direction.normalized * 1.3f);
        character.Animator.Play("AttackLunge1H");
        SlashEffect();
    }

    public void Dash(Vector2 dashDir)
    {
        if (dashDir == Vector2.zero) return;
        if (IsDashAvailable() && !IsCounterAttacking())
        {
            PlayAnim("Dash");
            lastTimeDash = Time.time;
            ApplyImpulse(dashDir);
        }
    }

    public void PlayAnim(string clipName)
    {
        //if (clipName != "Alert" && clipName != "Walk") Debug.Log(clipName);

        if (clipName == "Dash") character.Animator.Play(clipName);
        else if (clipName == "Attack" && Time.time < lastTimeDash + dashCD / 2)
        {
            AttackDash();
            return;
        }

        if (clipName == "Walk") SetAnimVelocity(2);
        else SetAnimVelocity(1);

        if (clipName == "Attack" || clipName == "AttackCombo") SlashEffect();
        character.Animator.StopPlayback();
        character.Animator.Play(AnimationManager.GetAnimationName(clipName, character.WeaponType));
    }

    public void ApplyImpulse(Vector2 dashDir)
    {
        rigidbody.velocity = dashDir * 10;
    }

    public void ShootWeapon()
    {
        GameManager.Instance.ShootWeapon(transform.position, transform.right);
    }

    public void ThrowBomb()
    {
        GameManager.Instance.ShootBomb(transform);
    }

    public void Burst(Vector2 hitDir)
    {
        Dismember(BodyLimb.Head, hitDir);
        Die();
    }

    public void Dismember(BodyLimb limb, Vector2 hitDir)
    {
        Transform oldLimb;
        switch (limb)
        {
            case BodyLimb.Head:
                oldLimb = headT;
                break;
            case BodyLimb.ArmRight:
                oldLimb = armRightT;
                break;
            case BodyLimb.ArmLeft:
                oldLimb = armLeftT;
                break;
            case BodyLimb.LegLeft:
                oldLimb = legLeftT;
                break;
            case BodyLimb.LegRight:
                oldLimb = legRightT;
                break;
            default:
                oldLimb = headT;
                break;
        }
        // GameObject newGO = GameObject.Instantiate(oldLimb.gameObject);
        // Transform newLimb = newGO.transform;
        oldLimb.gameObject.SetActive(false);

        // newLimb.position = oldLimb.position;
        // newLimb.localScale = oldLimb.lossyScale;
        // newLimb.rotation = oldLimb.rotation;
        // Rigidbody2D rb = newLimb.gameObject.AddComponent<Rigidbody2D>();
        // newGO.SetActive(true);

        if (limb == BodyLimb.Head)
        {
            GameObject newGO = GameObject.Instantiate(oldLimb.gameObject);
            GameObject.Destroy(newGO, 2.5f);
            Transform newLimb = newGO.transform;

            newLimb.Find("Eyes").GetComponent<SpriteRenderer>().sprite = EquipManager.Instance.deadEyes;
            newLimb.Find("Mouth").GetComponent<SpriteRenderer>().sprite = EquipManager.Instance.deadMouth;

            newLimb.position = oldLimb.position;
            newLimb.localScale = oldLimb.lossyScale;
            newLimb.rotation = oldLimb.rotation;



            // Poner la cabeza y sus elementos siempre visible
            SpriteRenderer parentRenderer = newLimb.GetComponent<SpriteRenderer>();
            parentRenderer.sortingOrder = parentRenderer.sortingOrder + 9999;
            foreach (Transform child in newLimb)
            {
                SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = renderer.sortingOrder + 9999;
                }
            }

            newGO.SetActive(true);
            Rigidbody2D rb = newLimb.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 10f;
            rb.AddForce(Vector2.up * 10 + hitDir * 5, ForceMode2D.Impulse);
        }
        else
        {
            //rb.gravityScale = UnityEngine.Random.Range(5f, 10f);
            float force = UnityEngine.Random.Range(5f, 10f);
            //rb.AddForce(Vector2.up * 10 + hitDir * force, ForceMode2D.Impulse);
        }

    }

    public void StrikeEntity(Entity entity, Vector2 hitDir)
    {
        lastTimeAttackHit = Time.time;
        entity.GetStrike(stats.strength + weapon.damage, hitDir);
    }

    public void SetAnimVelocity(int newVelocity)
    {
        character.Animator.speed = newVelocity;
        AttackAnimator.speed = newVelocity;
    }

    public void SlashEffect()
    {
        //AttackAnimator.Play("Slash1", 0);
    }

    public void MoveToDirection(Vector2 direction)
    {
        if (!IsCounterAttacking() && !IsDashAvailable()) rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, direction, Time.deltaTime * stats.velocity);

        if (IsMoveAvailable())
        {
            if (Mathf.Abs(direction.x) > 0.0f) SetOrientation(direction.x);
            PlayAnim("Walk");
            transform.position = Vector2.Lerp(
                GetActualPosition(),
                GetActualPosition() + direction * 2,
                Time.deltaTime * stats.velocity
            );

        }
    }

    bool IsMoveAvailable()
    {
        if (!IsDashAvailable() || IsCounterAttacking() || this.status == Status.Dead) return false;
        return Time.time > lastTimeAttack + attackCD * 0.99f;
    }

    bool IsDashAvailable()
    {
        if (!IsAttackAvailable() && !IsComboAttackAvailable()) return false;
        return Time.time > lastTimeDash + dashCD;
    }

    bool IsAttackAvailable()
    {
        return Time.time > lastTimeAttack + attackCD;
    }

    float lastTimeAttackHitWhenCombo;
    bool IsComboAttackAvailable()
    {
        return Time.time < lastTimeAttackHit + attackCD / 2 &&
                lastTimeAttackHit != lastTimeAttackHitWhenCombo;
    }

    float attackTime = 0.5f;
    bool IsPlayerAttacking()
    {
        return Time.time < lastTimeAttack + attackTime;
        //return Dummy.Animator.GetCurrentAnimatorStateInfo(0).IsName(AnimationManager.GetAnimationName("Attack", Dummy.WeaponType));
    }

    public bool IsCounterAttacking()
    {
        return Time.time < lastTimeCounterAttack + 0.5f;
    }

    public void Idle()
    {
        if (this.status == Status.Dead) return;

        if (IsAttackAvailable() && !IsPlayerAttacking() && IsDashAvailable())
        {
            if (IsDashAvailable()) rigidbody.velocity = Vector2.zero;
            if (!IsCounterAttacking()) PlayAnim("Alert");
        }
    }

    public void ClampMyself(bool clampX, bool clampY)
    {
        transform.position = new Vector2(
            clampX ? Mathf.Clamp(transform.position.x, CameraController.Instance.maxDistanceLeft, CameraController.Instance.maxDistanceRight) : transform.position.x,
            clampY ? Mathf.Clamp(transform.position.y, GameManager.Instance.minY, GameManager.Instance.maxY) : transform.position.y
        );
    }

    public void SetOrientation(float directionX)
    {
        bool facingRight = directionX > 0 ? true : false;
        transform.rotation = Quaternion.AngleAxis(facingRight ? 0 : -180, Vector3.up);
    }

    private float GetCD()
    {
        return Mathf.Clamp(Time.time - lastTimeAttack + attackCD, 0f, 1f);
    }

    private Vector2 GetActualPosition()
    {
        return new Vector2(transform.position.x, transform.position.y);
    }

}

[Serializable]
public class Player : Entity
{
    public Player(Transform transform, Stats stats, Weapon weapon, string name)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
        this.character = transform.GetChild(0).GetComponent<Character>();
        this.AttackAnimator = transform.Find("Attack_Effect").GetComponent<Animator>();
        this.rigidbody = transform.GetComponent<Rigidbody2D>();
        this.SetAnimVelocity(1);
        this.enemyMask = LayerMask.NameToLayer("Enemy");
        this.isActive = true;
        this.character.isActive = true;
        this.SaveTransformReferences();
        this.EquipWeapon(weapon);
        if (GameManager.Instance.isPacificLevel) this.character.isActive = false;
        this.status = Status.Alive;
        this.isPlayer = true;
    }

    public override void Die()
    {
        transform.gameObject.layer = LayerMask.NameToLayer("Ghost");
        PlayAnim("Die");
        this.isActive = false;
        this.status = Status.Dead;
        GameManager.Instance.GameOver();
    }

    public override void Update()
    {
        if (isActive) ClampMyself(true, true);
    }

    float inmuneTimeAfterDash = 0.2f;

    public override void GetStrike(int strikeForce, Vector2 hitDir)
    {

        if (IsCounterAttacking())
        {
            CounterAttack(-hitDir.normalized);
            return;
        }

        if (lastTimeDash + inmuneTimeAfterDash > Time.time) return;

        stats.life -= strikeForce;
        CanvasManager.Instance.HealthbarTakeDamage(stats.life);
        SoundManager.Instance.PlayHitPlayer();
        if (stats.life > 0)
        {
            PlayAnim("Hit");
        }
        else
        {
            Burst(hitDir);
        }
    }
}

[Serializable]
public class Enemy : Entity
{
    public BehaviorIA behavior = BehaviorIA.Normal;
    public TypeIA typeIA = TypeIA.Normal;

    public Enemy(Transform transform, Stats stats, Weapon weapon, string name, int ID)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
        this.ID = ID;
        this.character = transform.GetChild(0).GetComponent<Character>();
        this.AttackAnimator = transform.Find("Attack_Effect").GetComponent<Animator>();
        this.rigidbody = transform.GetComponent<Rigidbody2D>();
        this.SetAnimVelocity(1);
        this.enemyMask = LayerMask.NameToLayer("Player");
        this.SaveTransformReferences();
        this.EquipWeapon(weapon);
    }

    public override void Die()
    {
        transform.gameObject.layer = LayerMask.NameToLayer("Ghost");
        GameManager.Instance.RemoveEnemy(this);
        PlayAnim("Die");
        this.isActive = false;
        this.status = Status.Dead;
    }


    public override void Update()
    {
        //ClampMyself(true, true);
    }

    public void WaitForNextAction(float waitTime)
    {
        EnemiesManager.Instance.ImWaitingForNextAction(this, waitTime);
    }

    public float lastTimeStriked = 0f;
    public override void GetStrike(int strikeForce, Vector2 hitDir)
    {
        SoundManager.Instance.PlayHitSlash();
        lastTimeStriked = Time.time;
        EnemiesManager.Instance.StopEnemyRoutine(this.ID);
        rigidbody.AddForce(hitDir.normalized * strikeForce / 6.66f, ForceMode2D.Impulse);
        stats.life -= strikeForce;
        if (stats.life > 0)
        {
            // No muero
            WaitForNextAction(UnityEngine.Random.Range(0.8f, 1.5f) / stats.velocity);
            PlayAnim("Hit");
        }
        else
        {
            ScoreUI.Instance.AddScore(1);
            Burst(hitDir);
        }
    }
}