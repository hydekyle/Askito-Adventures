using System;
using UnityEngine;
using Assets.FantasyHeroes.Scripts;

[System.Serializable]
public struct Stats
{
    public int velocity, strength, life;
}

public abstract class Entity
{
    public string name;
    public Character Dummy;
    public Transform transform;
    public Stats stats;
    public float lastTimeAttack = 0f;
    public float attackCD = 0.1f;

    public void Update()
    {

    }

    public void SetVelocity(int newVelocity)
    {
        stats = new Stats()
        {
            life = stats.life,
            strength = stats.strength,
            velocity = newVelocity
        };
        Dummy.Animator.speed = newVelocity;
    }

    public void AnimPlay(string clipName)
    {
        Dummy.Animator.Play(ResolveAnimatiobClip(clipName));
    }

    private string ResolveAnimatiobClip(string clipName)
    {
        switch (clipName)
        {
            case "Alert":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow: return "Alert1H";
                    case WeaponType.Melee2H: return "Alert2H";
                    default: throw new NotImplementedException();
                }
            case "Attack":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H: return "Attack1H";
                    case WeaponType.Melee2H: return "Attack2H";
                    case WeaponType.MeleeTween: return "AttackTween";
                    case WeaponType.Bow: return "Shot";
                    default: throw new NotImplementedException();
                }
            case "AttackLunge":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.Melee2H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow: return "AttackLunge1H";
                    default: throw new NotImplementedException();
                }
            case "Cast":
                switch (Dummy.WeaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.Melee2H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow: return "Cast1H";
                    default: throw new NotImplementedException();
                }
            default: return clipName;
        }
    }

    public void Attack()
    {
        if (IsAttackAvailable())
        {
            lastTimeAttack = Time.time;
            Dummy.Animator.StopPlayback();
            AnimPlay("Attack");
            CameraController.Instance.ZoomOption(ZoomOptions.Focus);
        }
    }

    public void MoveToDirection(Vector2 direction)
    {
        if (!IsPlayerAttacking())
        {
            AnimPlay("Walk");
            transform.position = Vector2.Lerp(
                GetActualPosition(),
                GetActualPosition() + direction * 2,
                Time.deltaTime * stats.velocity * 10
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
        return Dummy.Animator.GetCurrentAnimatorStateInfo(0).IsName(ResolveAnimatiobClip("Attack"));
    }

    public void Idle()
    {
        if (!IsPlayerAttacking() && IsAttackAvailable()) AnimPlay("Alert");
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

public class Player : Entity
{
    public Player(Transform transform, Stats stats, string name)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
        this.Dummy = transform.GetComponent<Character>();
    }
}

public class Enemy : Entity
{
    public Enemy(Transform transform, Stats stats, string name)
    {
        this.transform = transform;
        this.stats = stats;
        this.name = name;
    }
}
