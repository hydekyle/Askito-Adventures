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
        combinedMask = 1 << LayerMask.NameToLayer("Enemy");

        // var animClip = Dummy.Animator.runtimeAnimatorController.animationClips.ToList().Find(clip => clip.name == ResolveAnimationClip("Attack"));
        // AnimationEvent newEvent = new AnimationEvent();
        // newEvent.stringParameter = "Pooolla";
        // animClip.AddEvent(newEvent);
        // Debug.Log(animClip.events.Length);
        //animClip.events.ElementAt(0).stringParameter = "Pooolla";
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

    public void BreakBreakable(Transform breakableT, Vector2 hitDir)
    {
        int breakableSortingOrder = breakableT.GetComponent<SpriteRenderer>().sortingOrder;
        var go = GameObject.Instantiate(GameManager.Instance.breakingBarrelPrefab);
        go.transform.position = breakableT.position;
        GameObject.Destroy(breakableT.gameObject);
        go.gameObject.SetActive(true);
        Transform pieceTop = go.transform.GetChild(0);
        Transform pieceBot = go.transform.GetChild(1);
        pieceTop.GetComponent<SpriteRenderer>().sortingOrder = pieceBot.GetComponent<SpriteRenderer>().sortingOrder = breakableSortingOrder;
        pieceTop.GetComponent<Rigidbody2D>().AddForce((hitDir * 8 + Vector2.up * 6) * stats.strength * 40, ForceMode2D.Force);
        pieceBot.GetComponent<Rigidbody2D>().AddForce((hitDir * 5 + Vector2.up) * stats.strength * 30, ForceMode2D.Force);
        GameObject.Destroy(go, 3f);
    }

    int combinedMask;

    public void CastRay()
    {
        var golpeados = Physics2D.CircleCastAll(transform.position + transform.right / 2, 0.4f, transform.right, 0.9f, GameManager.Instance.entityLayerMask | GameManager.Instance.breakableLayerMask);
        foreach (var go in golpeados)
        {
            Vector2 hitDir = go.transform.position - transform.position;
            if (go.transform.gameObject.layer == LayerMask.NameToLayer("Breakable"))
            {
                BreakBreakable(go.transform, hitDir);
            }
            else
            {
                Entity enemy = GameManager.Instance.enemies.Find(e => e.name == go.transform.name);
                if (enemy != null) StrikeEntity(enemy, hitDir);
            }
        }
    }

    public void GetStrike(int force, Vector2 hitDir)
    {
        rigidbody.AddForce(hitDir.normalized * force, ForceMode2D.Impulse);
        health -= force;
        if (health > 0)
        {
            // No muero   
        }
        else
        {
            Dismember(BodyLimb.ArmLeft, hitDir);
            Dismember(BodyLimb.ArmRight, hitDir);
            Dismember(BodyLimb.LegLeft, hitDir);
            Dismember(BodyLimb.LegRight, hitDir);
            Dismember(BodyLimb.Head, hitDir);
            Die();
        }
    }

    public void Die()
    {
        triggerCollider.enabled = false;
        transform.gameObject.layer = LayerMask.NameToLayer("Ghost");
        PlayAnim("Die");
    }

    public void Exposion()
    {
        //Dismember(BodyLimb.Head, hitDir + new Vector2(UnityEngine.Random.Range()))
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
                oldLimb = null;
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
    }
}