using System.ComponentModel;
using System.Linq;
using System;
using Assets.FantasyHeroes.Scripts;
using UnityEngine;

[Serializable]
public struct Stats
{
    public int velocity, strength, life;
}

public enum BodyLimb { Head, LegLeft, LegRight, ArmLeft, ArmRight }

[Serializable]
public abstract class Entity
{
    public string name;
    public int health = 9;
    public Character Dummy;
    public Transform transform;
    public Stats stats;
    public float lastTimeAttack = 0f;
    public float attackCD = 0.1f;
    public Animator AttackAnimator;
    public Rigidbody2D rigidbody;

    Transform headT, armLeftT, armRightT, legLeftT, legRightT;
    BoxCollider2D triggerCollider;
    BoxCollider2D rigidCollider;

    public LayerMask enemyMask;

    public void Start()
    {
        Transform pelvisT = transform.Find("Animation").Find("Pelvis");
        triggerCollider = pelvisT.GetComponent<BoxCollider2D>();
        rigidCollider = transform.GetComponent<BoxCollider2D>();
        Transform torsoT = pelvisT.Find("Torso");
        headT = torsoT.Find("Head");
        armLeftT = torsoT.Find("ArmL");
        armRightT = torsoT.Find("ArmR");
        legLeftT = pelvisT.Find("LegL");
        legRightT = pelvisT.Find("LegR");

    }

    public void Update()
    {

    }

    public void Attack()
    {
        if (IsAttackAvailable())
        {
            CameraController.Instance.ZoomOption(ZoomOptions.Focus);
            lastTimeAttack = Time.time;
            PlayAnim("Attack");
            SlashAnim();
        }
    }

    public void CastAttack()
    {
        var raycastHit = Physics2D.CircleCastAll(transform.position + transform.right / 2, 0.4f, transform.right, 0.9f);
        foreach (var hit in raycastHit)
        {
            LayerMask hitLayer = hit.transform.gameObject.layer;
            Vector2 hitDir = (hit.transform.position - transform.position).normalized;

            if (hitLayer == LayerMask.NameToLayer("Breakable"))
            {
                GameManager.BreakBreakable(hit.transform, hitDir);
            }
            else if (hitLayer == enemyMask)
            {
                if (this.GetType() == typeof(Player))
                {
                    Entity enemy = GameManager.Instance.enemies.Find(e => e.name == hit.transform.name);
                    if (enemy != null) StrikeEntity(enemy, hitDir);
                }
                else
                {
                    StrikeEntity(GameManager.Instance.player, hitDir);
                }
            }
            else if (hitLayer == LayerMask.NameToLayer("Movible"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                hit.transform.GetComponent<Rigidbody2D>()?.AddForce(hitDir * 200 * stats.strength / Mathf.Pow(distance, 3), ForceMode2D.Impulse);
            }

        }
    }

    public abstract void GetStrike(int strikeForce, Vector2 hitDir);

    public void Die()
    {
        triggerCollider.enabled = false;
        transform.gameObject.layer = LayerMask.NameToLayer("Ghost");
        PlayAnim("Die");
    }

    public void ThrowBomb()
    {
        GameObject.Instantiate(GameManager.Instance.bombPrefab, transform.position, transform.rotation);
    }

    public void Burst(Vector2 hitDir)
    {
        Dismember(BodyLimb.ArmLeft, hitDir);
        Dismember(BodyLimb.ArmRight, hitDir);
        Dismember(BodyLimb.LegLeft, hitDir);
        Dismember(BodyLimb.LegRight, hitDir);
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
        GameObject newGO = GameObject.Instantiate(oldLimb.gameObject);
        Transform newLimb = newGO.transform;
        oldLimb.gameObject.SetActive(false);

        newLimb.position = oldLimb.position;
        newLimb.localScale = oldLimb.lossyScale;
        newLimb.rotation = oldLimb.rotation;
        Rigidbody2D rb = newLimb.gameObject.AddComponent<Rigidbody2D>();
        newGO.SetActive(true);

        if (limb == BodyLimb.Head)
        {
            rb.gravityScale = 20f;
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
            rb.gravityScale = UnityEngine.Random.Range(5f, 10f);
            float force = UnityEngine.Random.Range(5f, 10f);
            rb.AddForce(Vector2.up * 10 + hitDir * force, ForceMode2D.Impulse);
        }
        CleanMyself(newLimb);
    }

    public void StrikeEntity(Entity entity, Vector2 hitDir)
    {
        entity.GetStrike(stats.strength * 10, hitDir);
    }

    public void CleanMyself(Transform limb)
    {
        GameManager.Instance.enemies.Remove(this);
        GameObject.Destroy(transform.gameObject, 2.5f);
        GameObject.Destroy(limb.gameObject, 2.5f);
    }

    public void SetAnimVelocity(int newVelocity)
    {
        Dummy.Animator.speed = newVelocity;
        AttackAnimator.speed = newVelocity;
    }

    public void PlayAnim(string clipName)
    {
        if (clipName == "Alert") SetAnimVelocity(1);
        else SetAnimVelocity(2);
        Dummy.Animator.StopPlayback();
        Dummy.Animator.Play(ResolveAnimationClip(clipName));
    }

    public void SlashAnim()
    {
        AttackAnimator.Play("Slash1", 0);
    }

    public void MoveToDirection(Vector2 direction)
    {
        if (!IsPlayerAttacking())
        {
            PlayAnim("Walk");
            transform.position = Vector2.Lerp(
                GetActualPosition(),
                GetActualPosition() + direction * 2,
                Time.deltaTime * stats.velocity
            );

            if (Mathf.Abs(direction.x) > 0.0f)
            {
                SetOrientation(direction.x);
                CameraController.Instance.ZoomOption(ZoomOptions.Normal);
            }
        }
    }

    bool IsAttackAvailable()
    {
        return Time.time > lastTimeAttack + attackCD;
    }

    bool IsPlayerAttacking()
    {
        return Dummy.Animator.GetCurrentAnimatorStateInfo(0).IsName(ResolveAnimationClip("Attack"));
    }

    public void Idle()
    {
        if (!IsPlayerAttacking() && IsAttackAvailable()) PlayAnim("Alert");
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

    private string ResolveAnimationClip(string clipName)
    {
        switch (clipName)
        {
            case "Alert":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow:
                        return "Alert1H";
                    case WeaponType.Melee2H:
                        return "Alert2H";
                    default:
                        throw new NotImplementedException();
                }
            case "Attack":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                        return "Attack1H";
                    case WeaponType.Melee2H:
                        return "Attack2H";
                    case WeaponType.MeleeTween:
                        return "AttackTween";
                    case WeaponType.Bow:
                        return "Shot";
                    default:
                        throw new NotImplementedException();
                }
            case "AttackLunge":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.Melee2H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow:
                        return "AttackLunge1H";
                    default:
                        throw new NotImplementedException();
                }
            case "Cast":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.Melee2H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow:
                        return "Cast1H";
                    default:
                        throw new NotImplementedException();
                }
            default:
                return clipName;
        }
    }
}

[Serializable]
public class Player : Entity
{
    public Player(Transform transform, Stats stats, string name)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
        this.Dummy = transform.GetComponent<Character>();
        this.AttackAnimator = transform.Find("Attack_Effect").GetComponent<Animator>();
        this.rigidbody = transform.GetComponent<Rigidbody2D>();
        this.SetAnimVelocity(2);
        this.enemyMask = LayerMask.NameToLayer("Enemy");
    }

    public override void GetStrike(int strikeForce, Vector2 hitDir)
    {
        rigidbody.AddForce(hitDir.normalized * strikeForce, ForceMode2D.Impulse);
        health -= strikeForce;
        if (health > 0)
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
    public Enemy(Transform transform, Stats stats, string name)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
        this.Dummy = transform.GetComponent<Character>();
        this.AttackAnimator = transform.Find("Attack_Effect").GetComponent<Animator>();
        this.rigidbody = transform.GetComponent<Rigidbody2D>();
        this.SetAnimVelocity(1);
        this.enemyMask = LayerMask.NameToLayer("Player");
    }

    public override void GetStrike(int strikeForce, Vector2 hitDir)
    {
        rigidbody.AddForce(hitDir.normalized * strikeForce, ForceMode2D.Impulse);
        health -= strikeForce;
        if (health > 0)
        {
            // No muero
        }
        else
        {
            Burst(hitDir);
        }
    }
}