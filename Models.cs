using System;
using Assets.FantasyHeroes.Scripts;
using UnityEngine;

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

[Serializable]
public abstract class Entity
{
    public int ID;
    public Status status = Status.Dead;

    public Character Dummy;
    public Weapon weapon;
    public Transform transform;
    public Stats stats;
    public Animator AttackAnimator;
    public Rigidbody2D rigidbody;
    public LayerMask enemyMask;
    public Vector2 attackDirection = Vector2.right;
    public string name;
    public float lastTimeAttack = 0f;
    public float attackCD = 0.4f;
    public bool isActive = false;
    //public float padVibration = 0f;

    float lastTimeDash;
    float dashCD = 0.5f;

    public Transform headT, armLeftT, armRightT, legLeftT, legRightT;

    public abstract void Die();

    public void EquipWeapon(Weapon newWeapon)
    {
        weapon = newWeapon;
        this.Dummy.WeaponType = this.weapon.type;
        Dummy.SetWeaponSprite(newWeapon.sprite);
    }

    public void Spawn(Vector2 position, Stats newStats)
    {
        this.stats = newStats;
        this.transform.position = position;
        this.status = Status.Alive;
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
        var raycastHit = Physics2D.CircleCastAll(
            transform.position + transform.right,
            0.5f + 0.1f * weapon.radius,
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

    public void Attack(Vector2 attackDir)
    {
        if (IsAttackAvailable())
        {
            lastTimeAttack = Time.time;
            attackDirection = attackDir == Vector2.zero ? (Vector2)transform.right : attackDir;
            ApplyImpulse(attackDir / 2);
            PlayAnim("Attack"); // La animación llamará a CastAttack
        }
    }

    public void AttackDash()
    {
        Vector2 direction = rigidbody.velocity;
        if (Mathf.Abs(direction.x) > 0.0f) SetOrientation(direction.x);
        lastTimeAttack = lastTimeDash = Time.time;
        ApplyImpulse(direction.normalized * 1.3f);
        Dummy.Animator.Play("AttackLunge1H");
        SlashAnim();
    }

    public void Dash(Vector2 dashDir)
    {
        if (dashDir == Vector2.zero) return;
        if (IsDashAvailable())
        {
            PlayAnim("Dash");
            lastTimeDash = Time.time;
            ApplyImpulse(dashDir);
        }
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
            Transform newLimb = newGO.transform;

            newLimb.position = oldLimb.position;
            newLimb.localScale = oldLimb.lossyScale;
            newLimb.rotation = oldLimb.rotation;
            Rigidbody2D rb = newLimb.gameObject.AddComponent<Rigidbody2D>();
            newGO.SetActive(true);
            GameObject.Destroy(newLimb.gameObject, 2.5f);

            rb.gravityScale = 10f;
            rb.AddForce(Vector2.up * 10 + hitDir * 5, ForceMode2D.Impulse);

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
        entity.GetStrike(stats.strength + weapon.damage, hitDir);
    }

    public void SetAnimVelocity(int newVelocity)
    {
        Dummy.Animator.speed = newVelocity;
        AttackAnimator.speed = newVelocity;
    }

    public void PlayAnim(string clipName)
    {
        if (clipName == "Dash") Dummy.Animator.Play(clipName);
        else if (Time.time < lastTimeDash + dashCD / 2)
        {
            if (clipName == "Attack") AttackDash();
            return;
        }

        if (clipName == "Walk") SetAnimVelocity(2);
        else SetAnimVelocity(1);
        if (clipName == "Attack") SlashAnim();
        Dummy.Animator.StopPlayback();
        Dummy.Animator.Play(GameManager.GetAnimationName(clipName, Dummy.WeaponType));
    }

    public void SlashAnim()
    {
        AttackAnimator.Play("Slash1", 0);
    }

    public void MoveToDirection(Vector2 direction)
    {
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, direction, Time.deltaTime * stats.velocity);

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
        if (!IsDashAvailable()) return false;
        return Time.time > lastTimeAttack + attackCD * 0.99f;
    }

    bool IsDashAvailable()
    {
        if (!IsAttackAvailable()) return false;
        return Time.time > lastTimeDash + dashCD;
    }

    bool IsAttackAvailable()
    {
        return Time.time > lastTimeAttack + attackCD;
    }

    float attackTime = 0.5f;
    bool IsPlayerAttacking()
    {
        return Time.time < lastTimeAttack + attackTime;
        //return Dummy.Animator.GetCurrentAnimatorStateInfo(0).IsName(GameManager.GetAnimationName("Attack", Dummy.WeaponType));
    }

    public void Idle()
    {
        if (IsAttackAvailable() && !IsPlayerAttacking())
        {
            if (IsDashAvailable()) rigidbody.velocity = Vector2.zero;
            PlayAnim("Alert");
        }
    }

    public void ClampMyself(bool clampX, bool clampY)
    {
        transform.position = new Vector2(
            clampX ? Mathf.Clamp(transform.position.x, CameraController.Instance.maxPlayerDistanceLeft, Mathf.Infinity) : transform.position.x,
            clampY ? Mathf.Clamp(transform.position.y, -2.77f, 1f) : transform.position.y // HardCoded map boundaries :D
        );
    }

    private void SetOrientation(float directionX)
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
        this.Dummy = transform.GetComponent<Character>();
        this.AttackAnimator = transform.Find("Attack_Effect").GetComponent<Animator>();
        this.rigidbody = transform.GetComponent<Rigidbody2D>();
        this.SetAnimVelocity(1);
        this.enemyMask = LayerMask.NameToLayer("Enemy");
        this.isActive = true;
        this.SaveTransformReferences();
        this.EquipWeapon(weapon);
    }

    public override void Die()
    {
        Debug.Log("Legends never dies");
    }

    public override void Update()
    {
        //WorkVibration();
        if (isActive) ClampMyself(true, true);
    }

    // private void WorkVibration()
    // {
    //     padVibration = Mathf.Lerp(padVibration, 0.0f, Time.deltaTime * 15);
    //     GameManager.Instance.PadVibration(padVibration);
    // }

    public override void GetStrike(int strikeForce, Vector2 hitDir)
    {
        rigidbody.AddForce(hitDir.normalized * strikeForce, ForceMode2D.Impulse);
        stats.life -= strikeForce;
        if (stats.life > 0)
        {
            Debug.Log("Me hacen pupa");
        }
        else
        {
            Debug.Log("Legends never die");
        }
    }
}

[Serializable]
public class Enemy : Entity
{
    public Enemy(Transform transform, Stats stats, Weapon weapon, string name, int ID)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
        this.ID = ID;
        this.Dummy = transform.GetComponent<Character>();
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
        PlayAnim("Die");
        GameManager.Instance.RemoveEnemy(this);
    }

    public override void Update()
    {
        ClampMyself(false, true);
    }

    public override void GetStrike(int strikeForce, Vector2 hitDir)
    {
        rigidbody.AddForce(hitDir.normalized * strikeForce, ForceMode2D.Impulse);
        stats.life -= strikeForce;
        if (stats.life > 0)
        {
            // No muero
        }
        else
        {
            Burst(hitDir);
        }
    }
}