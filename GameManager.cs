using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.FantasyHeroes.Scripts;
using EZObjectPools;
//using XInputDotNetPure;
using UnityEngine.SceneManagement;
using Doublsb.Dialog;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public DialogManager dialogManager;

    public Dictionary<int, Entity> enemiesRef = new Dictionary<int, Entity>();
    public Enemy[] enemies;

    public Player player;

    public GameObject playerGO;
    [HideInInspector]
    public Transform playerTransform;

    public static LayerMask enemyLayerMask;
    public static LayerMask ghostLayerMask;
    public static LayerMask breakableLayerMask;

    public Stats cheatStats;

    public GameObject enemyPrefab;

    public Transform mapEnemies;
    public Transform mapBreakables;

    public GameObject breakingBarrelPrefab, bombPrefab, bombEffectPrefab, shootPrefab, hitPrefab;
    EZObjectPool barrelsPool, bulletsPool, enemiesPool, bombsPool, hitsPool;

    [HideInInspector]
    public EZObjectPool bombEffectPool;

    //public AdmobManager admob;

    // GamePadState state;
    // GamePadState prevState;

    public int maxEnemies = 10;
    int enemyCounterID = 0;

    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        //admob = new AdmobManager();

        enemies = new Enemy[maxEnemies];

        bulletsPool = EZObjectPool.CreateObjectPool(shootPrefab, "Shoot1", 20, true, true, true);
        enemiesPool = EZObjectPool.CreateObjectPool(enemyPrefab, "Enemies", maxEnemies, true, true, true);
        bombsPool = EZObjectPool.CreateObjectPool(bombPrefab, "Bombs", 1, true, true, true);
        bombEffectPool = EZObjectPool.CreateObjectPool(bombEffectPrefab, "BombEffect", 1, true, true, true);
        hitsPool = EZObjectPool.CreateObjectPool(hitPrefab, "HitEffect", 6, true, true, true);
        barrelsPool = EZObjectPool.CreateObjectPool(breakingBarrelPrefab, "Barrels", 3, true, true, true);

        FixMapSpriteOrders();
        AddDefaultPlayer("Player");
        GenerateMapEnemiesRefs();
    }

    private void Start()
    {
        string text = string.Format("Soy Askito y soy...{0} todo-poderoso", "/click/");
        ShowDialog(text);
    }

    public void ShowDialog(string text)
    {
        DialogData dialogData = new DialogData(text, "Askito");
        dialogManager.Show(dialogData);
    }

    private void Update()
    {
        if (player != null)
        {
            Controls();
            player.Update();
        }
        foreach (Entity enemy in enemiesRef.Values) enemy.Update();
    }

    private int GetNextEnemyID()
    {
        if (enemyCounterID + 1 >= enemies.Length) enemyCounterID = 0;
        else enemyCounterID++;

        if (enemiesRef.TryGetValue(enemyCounterID, out Entity storedEntity))
        {
            Debug.LogWarningFormat("La entidad {0} aún existe", storedEntity.name);
            storedEntity.Burst(Vector2.zero);
        }

        return enemyCounterID;
    }

    public void SpawnEnemyRandom()
    {
        Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0);
        if (enemiesPool.TryGetNextObject(player.transform.position + randomPos, Quaternion.identity, out GameObject go))
        {
            int enemyID = GetNextEnemyID();
            string enemyName = "Enemy " + enemyID;

            Enemy enemy = new Enemy(
                go.transform,
                new Stats() { life = 1, strength = 1, velocity = 1 },
                enemyName,
                enemyID
            );

            go.name = enemyName;
            enemy.Start();
            AddEnemyRefs(enemyID, enemy);
        }
    }

    private void GenerateMapEnemiesRefs()
    {
        foreach (Transform enemyT in mapEnemies)
        {
            int enemyID = GetNextEnemyID();
            string enemyName = enemyT.name.Split(' ')[0] + " " + enemyID;
            Enemy enemy = new Enemy(
                enemyT,
                new Stats() { life = 1, strength = 1, velocity = 1 },
                enemyName,
                enemyID
            );
            enemyT.name = enemyName;
            AddEnemyRefs(enemyID, enemy);
            enemy.Start();
        }
    }

    public void AddEnemyRefs(int enemyID, Enemy enemy)
    {
        enemiesRef.Add(enemyID, enemy);
        enemies[enemyID] = enemy;
    }

    public void DeleteEnemyRefs(int enemyID)
    {
        enemiesRef.Remove(enemyID);
        enemies[enemyID] = null;
    }

    private void FixMapSpriteOrders()
    {
        foreach (Transform breakablesT in mapBreakables)
        {
            SpriteRenderer renderer = breakablesT.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = renderer.sortingOrder - (int)(breakablesT.position.y * 100);
        }
    }

    private void SetCheatStats()
    {
        player.stats = cheatStats;
    }

    private void AddDefaultPlayer(string playerName)
    {
        playerTransform = Instantiate(playerGO, Vector3.zero, transform.rotation).transform;

        Player newPlayer = new Player(
            playerTransform,
            new Stats() { life = 1, strength = 1, velocity = 2 },
            playerName
        );
        player = newPlayer;
        playerTransform.name = playerName;
        //entities.Add(player);
    }

    private void RandomEnemyAttack()
    {
        bool pinga = UnityEngine.Random.Range(0, 2) > 0;
        foreach (var entity in enemiesRef.Values)
        {
            entity.PlayAnim(pinga ? "Attack" : "Die");
        }
    }

    private void RestartScene()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }

    public void ShootWeapon(Vector2 sPosition, Vector2 sDirection)
    {
        if (bulletsPool.TryGetNextObject(sPosition, transform.rotation, out GameObject go))
        {
            go.SetActive(true);
            go.GetComponent<Rigidbody2D>()?.AddForce(sDirection.normalized * Time.deltaTime / 2, ForceMode2D.Impulse);
        }
    }

    public void ShootBomb(Transform shooter)
    {
        if (bombsPool.TryGetNextObject(shooter.position, shooter.rotation, out GameObject bomb))
        {
            bomb.SetActive(true);
        }
    }

    public void BreakBreakable(Transform oldBreakable, Vector2 hitDir)
    {
        if (oldBreakable.name.Split(' ')[0] == "Barrel")
        {
            int breakableSortingOrder = oldBreakable.GetComponent<SpriteRenderer>().sortingOrder;
            if (barrelsPool.TryGetNextObject(oldBreakable.position, oldBreakable.rotation, out GameObject newBreaking))
            {
                newBreaking.transform.position = oldBreakable.position;
                oldBreakable.gameObject.SetActive(false);
                StartCoroutine(ReciclateBarrel(newBreaking, hitDir, breakableSortingOrder));
            }
        }
        else
        {
            Destroy(oldBreakable.gameObject);
        }
    }

    public void ShowHitEffect(Vector2 hitPosition)
    {
        if (hitsPool.TryGetNextObject(hitPosition, transform.rotation, out GameObject hitEffect))
        {
            hitEffect.SetActive(true);
        }
    }

    IEnumerator<WaitForSeconds> ReciclateBarrel(GameObject newBreaking, Vector2 hitDir, int sortingOrder)
    {
        Transform pieceTop = newBreaking.transform.GetChild(0);
        Transform pieceBot = newBreaking.transform.GetChild(1);
        pieceBot.localPosition = Vector3.zero;
        pieceTop.localPosition = Vector3.zero;
        newBreaking.SetActive(true);
        pieceTop.GetComponent<SpriteRenderer>().sortingOrder = pieceBot.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        pieceTop.GetComponent<Rigidbody2D>().AddForce((hitDir * 8 + Vector2.up * 6) * 40, ForceMode2D.Force);
        pieceBot.GetComponent<Rigidbody2D>().AddForce((hitDir * 5 + Vector2.up) * 30, ForceMode2D.Force);
        yield return new WaitForSeconds(3f);
        newBreaking.SetActive(false);
    }

    IEnumerator<WaitForSeconds> ReciclateEntity(Entity entity)
    {
        DeleteEnemyRefs(entity.ID);
        yield return new WaitForSeconds(2.5f);
        entity.transform.position -= Vector3.right * 100; // Evitando bug de rotación en la animación de forma poco elegante
        entity.PlayAnim("Attack");
        yield return new WaitForSeconds(0.1f);
        entity.headT.gameObject.SetActive(true);
        entity.armRightT.gameObject.SetActive(true);
        entity.armLeftT.gameObject.SetActive(true);
        entity.legRightT.gameObject.SetActive(true);
        entity.legLeftT.gameObject.SetActive(true);
        entity.transform.gameObject.SetActive(false);

        entity.transform.gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity.GetType() == typeof(Enemy))
        {
            StartCoroutine(ReciclateEntity(entity));
        }
        else Debug.Log("¡Se muere el player!");
    }

    public static string GetAnimationName(string clipName, WeaponType weaponType)
    {
        switch (clipName)
        {
            case "Alert":
                switch (weaponType)
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
                switch (weaponType)
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
                switch (weaponType)
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
                switch (weaponType)
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

    public Entity GetEnemyByName(string enemyName)
    {
        int ID = int.Parse(enemyName.Split(' ')[1]);
        return enemies[ID];
    }

    public void PadVibration(float vForce)
    {
        //GamePad.SetVibration(PlayerIndex.One, vForce, vForce);
    }

    private void Controls()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        if (Mathf.Abs(xAxis) > 0.0f || Mathf.Abs(yAxis) > 0.0f)
        {
            player.MoveToDirection(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }
        else player.Idle();

        if (Input.GetButtonDown("Attack")) player.Attack(new Vector2(xAxis, yAxis));
        else if (Input.GetButtonDown("Fire2")) player.Dash(new Vector2(xAxis, yAxis).normalized * 1.5f);
        else if (Input.GetButtonDown("Jump")) SpawnEnemyRandom();
        else if (Input.GetButtonDown("Fire3")) player.ThrowBomb();

        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(0);


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) SetCheatStats();
        if (Input.GetButtonDown("Jump")) SpawnEnemyRandom();
        if (Input.GetKeyDown(KeyCode.F3)) RandomEnemyAttack();
        if (Input.GetKeyDown(KeyCode.F12)) RestartScene();
#endif
    }

    public void ButtonFromCanvas(string actionButton)
    {
        Input.PressButtonDownMobile(actionButton);
    }

    public static bool CheckYProximity(Vector2 hitPosition, Vector2 attackPosition, Vector2 attackDir)
    {
        return Mathf.Abs(hitPosition.y - (attackPosition.y + attackDir.y)) < 1f;
    }

    public void ResolveHits(Entity myself, RaycastHit2D[] raycastHit, Vector2 attackDir, LayerMask enemyMask)
    {
        StartCoroutine(ResolveHitsSlow(myself, raycastHit, attackDir, enemyMask));
    }

    public void ResolveExplosion(Transform explosionT, RaycastHit2D[] hits)
    {
        StartCoroutine(ResolveExplosionHits(explosionT, hits));
    }

    IEnumerator<WaitForEndOfFrame> ResolveExplosionHits(Transform explosionT, RaycastHit2D[] hits)
    {
        Vector2 explosionPosition = explosionT.transform.position;
        foreach (var hit in hits)
        {
            try
            {
                float distance = Vector2.Distance(hit.transform.position, explosionPosition);
                Vector2 hitDir = ((Vector2)hit.transform.position - explosionPosition).normalized;
                var hitMask = hit.transform.gameObject.layer;
                if (hitMask == LayerMask.NameToLayer("Enemy"))
                {
                    GetEnemyByName(hit.transform.gameObject.name)?.Burst(hitDir);
                }
                else if (hitMask == LayerMask.NameToLayer("Breakable"))
                {
                    BreakBreakable(hit.transform, hitDir);
                }
                else if (hitMask == LayerMask.NameToLayer("Player"))
                {
                    //hit.transform.GetComponent<Rigidbody2D>().AddForce(hitDir * 2, ForceMode2D.Impulse);
                }
                else if (hitMask == LayerMask.NameToLayer("Movible"))
                {
                    hit.transform.GetComponent<Rigidbody2D>().AddForce(hitDir * 999 / Mathf.Pow(distance, 2), ForceMode2D.Impulse);
                }
            }
            catch
            {
                Debug.LogWarning("Uff");
            }
            yield return new WaitForEndOfFrame();
        }

    }

    private IEnumerator<WaitForEndOfFrame> ResolveHitsSlow(Entity myself, RaycastHit2D[] raycastHit, Vector2 attackDir, LayerMask enemyMask)
    {
        foreach (var hit in raycastHit)
        {
            try
            {
                if (hit.collider.isTrigger)
                {
                    if (CheckYProximity(hit.transform.position, myself.transform.position, attackDir))
                    {
                        LayerMask hitLayer = hit.transform.gameObject.layer;
                        Vector2 hitDir = (hit.transform.position - myself.transform.position).normalized;
                        byte enemyCount = 0;
                        byte breakableCount = 0;

                        if (hitLayer == LayerMask.NameToLayer("Breakable"))
                        {
                            GameManager.Instance.BreakBreakable(hit.transform, hitDir);
                            breakableCount++;
                        }

                        else if (hitLayer == enemyMask)
                        {
                            if (myself.GetType() == typeof(Player)) // Si soy el jugador... (cambiar esto de sitio)
                            {
                                Entity enemy = GameManager.Instance.GetEnemyByName(hit.transform.name);
                                if (enemy != null)
                                {
                                    ShowHitEffect(hit.transform.position);
                                    myself.StrikeEntity(enemy, hitDir);
                                    enemyCount++;
                                }
                            }
                            else
                            {
                                myself.StrikeEntity(GameManager.Instance.player, hitDir);
                                enemyCount++;
                            }
                        }
                        else if (hitLayer == LayerMask.NameToLayer("Movible"))
                        {
                            ShowHitEffect(hit.transform.position);
                            float distance = Vector2.Distance(myself.transform.position, hit.transform.position);
                            hit.transform.GetComponent<Rigidbody2D>()?.AddForce(hitDir * 300 * myself.stats.strength / Mathf.Pow(distance, 3), ForceMode2D.Impulse);
                        }

                        if (enemyCount > 0) // Retroceso al golpear
                        {
                            myself.ApplyImpulse(-attackDir / 2);
                            //myself.padVibration = 0.666f;
                        }
                        else if (enemyCount < breakableCount)
                        {
                            //myself.padVibration = 0.333f;
                        }

                    }
                }
            }
            catch { }
            yield return new WaitForEndOfFrame();
        }
    }

}




