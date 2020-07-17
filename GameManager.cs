using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using UnityEngine.SceneManagement;
using Doublsb.Dialog;
//using XInputDotNetPure;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ScriptableWeapons tableWeapons;

    public DialogManager dialogManager;

    //public Dictionary<int, Entity> enemiesRef = new Dictionary<int, Entity>();
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
    EZObjectPool barrelsPool, bulletsPool, bombsPool, hitsPool;

    [HideInInspector]
    public EZObjectPool bombEffectPool;

    //public AdmobManager admob;

    // GamePadState state;
    // GamePadState prevState;

    public bool gameIsActive = false;
    public int maxEnemies = 10;
    int enemyCounter = 0;

    Transform[] enemiesT;

    CullingManager cullingManager;

    Db db;

    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        //admob = new AdmobManager();
        db = new Db();
        GeneratePools();
        SetMapSpriteOrders();
        AddDefaultPlayer("Player");
        AllocateEnemies();
        cullingManager = new CullingManager();
    }

    private void GeneratePools()
    {
        bulletsPool = EZObjectPool.CreateObjectPool(shootPrefab, "Shoot1", 20, true, true, true);
        bombsPool = EZObjectPool.CreateObjectPool(bombPrefab, "Bombs", 1, true, true, true);
        bombEffectPool = EZObjectPool.CreateObjectPool(bombEffectPrefab, "BombEffect", 1, true, true, true);
        hitsPool = EZObjectPool.CreateObjectPool(hitPrefab, "HitEffect", 6, true, true, true);
        barrelsPool = EZObjectPool.CreateObjectPool(breakingBarrelPrefab, "Barrels", 3, true, true, true);
    }

    public void AllocateEnemies()
    {
        enemiesT = new Transform[maxEnemies];
        enemies = new Enemy[maxEnemies];

        for (var x = 0; x < maxEnemies; x++)
        {
            var newEnemy = Instantiate(enemyPrefab, Vector3.zero, transform.rotation);

            string enemyName = String.Concat("Enemy ", x.ToString());
            newEnemy.name = enemyName;
            enemies[x] = new Enemy(
                    newEnemy.transform,
                    basicStats,
                    tableWeapons.common[UnityEngine.Random.Range(0, 4)],
                    enemyName,
                    x
                );

            newEnemy.transform.parent = mapEnemies;
            enemiesT[x] = newEnemy.transform;
        }
    }

    public void ShowDialog(string text)
    {
        DialogData dialogData = new DialogData(text, "Askito");
        dialogManager.Show(dialogData);
    }

    private void Update()
    {
        if (gameIsActive) Controls();
        player.Update();
    }

    private void FixedUpdate()
    {
        for (var x = 0; x < enemies.Length; x++)
        {
            Enemy enemy = enemies[x];
            if (enemy.status == Status.Alive)
            {
                enemy.Update();
                //print(cullingManager.IsVisible(enemy.ID));
            }
        }
    }

    private int GetNextEnemyID()
    {
        return enemyCounter + 1 < enemies.Length ? enemyCounter++ : 0;
    }

    public void SpawnEnemyRandom()
    {
        int enemyID = GetNextEnemyID();
        Enemy enemy = enemies[enemyID];

        Vector2 randomPos = new Vector2(UnityEngine.Random.Range(-6f, 6f), UnityEngine.Random.Range(-6f, 6f));

        Vector2 finalPos = (Vector2)player.transform.position + randomPos;

        if (enemy.status != Status.Alive)
        {
            enemy.Spawn(finalPos, basicStats);
        }
        else
        {
            enemies.ToList().Find(e => e.status == Status.Dead)?.Spawn(finalPos, basicStats);
        }
        cullingManager.SetSphere(enemy.ID, finalPos);
    }

    Stats basicStats = new Stats() { life = 5, strength = 1, velocity = 1 };

    public void DeleteEnemy(int enemyID)
    {
        enemies[enemyID].status = Status.Dead;
    }

    private void SetMapSpriteOrders()
    {
        foreach (Transform breakablesT in mapBreakables)
        {
            SpriteRenderer renderer = breakablesT.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = renderer.sortingOrder - (int)(breakablesT.position.y * 100);
        }
    }

    private void AddDefaultPlayer(string playerName)
    {
        playerTransform = Instantiate(playerGO, Vector3.zero, transform.rotation).transform;

        Player newPlayer = new Player(
            playerTransform,
            new Stats() { life = 1, strength = 1, velocity = 2 },
            tableWeapons.common[UnityEngine.Random.Range(0, tableWeapons.common.Count)],
            playerName
        );
        player = newPlayer;
        playerTransform.name = playerName;
        gameIsActive = true;
    }

    private void SetCheatStats()
    {
        player.stats = cheatStats;
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
        string breakableName = oldBreakable.name.Split(' ')[0];
        if (breakableName.Equals("Barrel"))
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
            //Destroy(oldBreakable.gameObject);
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
        DeleteEnemy(entity.ID);
    }

    public void RemoveEnemy(Enemy entity)
    {
        StartCoroutine(ReciclateEntity(entity));
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

    public void ButtonFromCanvas(string actionButton)
    {
        Input.PressButtonDownMobile(actionButton);
    }

    public void EquipPlayerWeapon(Weapon weapon)
    {
        player.EquipWeapon(weapon);
    }

    public static bool CheckYProximity(Vector2 hitPosition, Vector2 attackPosition, Vector2 attackDir)
    {
        return Mathf.Abs(hitPosition.y - (attackPosition.y + attackDir.y)) < 1.33f;
    }

    public void VibrationForce(long force)
    {
        Vibration.Vibrate(13 + force);
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
                            VibrationForce(10);
                        }

                        else if (hitLayer == enemyMask)
                        {
                            if (myself.GetType() == typeof(Player)) // Si soy el jugador... (cambiar esto de sitio)
                            {
                                Entity enemy = GameManager.Instance.GetEnemyByName(hit.transform.name);
                                if (enemy != null)
                                {
                                    VibrationForce(18);
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

    private void OnDestroy()
    {
        cullingManager.Dispose();
    }

    // private void GenerateMapEnemiesRefs()
    // {
    //     foreach (Transform enemyT in mapEnemies)
    //     {
    //         int enemyID = GetNextEnemyID();
    //         string enemyName = enemyT.name.Split(' ')[0] + " " + enemyID;
    //         Enemy enemy = new Enemy(
    //             enemyT,
    //             new Stats() { life = 1, strength = 1, velocity = 1 },
    //             tableWeapons.weapons[UnityEngine.Random.Range(0, tableWeapons.weapons.Count)],
    //             enemyName,
    //             enemyID
    //         );
    //         enemyT.name = enemyName;
    //         enemy.SaveTransformReferences();
    //     }
    // }

    int wIndex = 0;

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
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (wIndex + 1 < tableWeapons.common.Count) wIndex++;
            else wIndex = 0;
            EquipPlayerWeapon(tableWeapons.common[wIndex]);
            List<Sprite> lista = EquipManager.Instance.skeleton;
            EquipManager.Instance.EquipShit(enemies[0].Dummy, lista);

        }
        if (Input.GetButtonDown("Jump")) SpawnEnemyRandom();
        if (Input.GetKeyDown(KeyCode.F12)) RestartScene();
#endif
    }

}




