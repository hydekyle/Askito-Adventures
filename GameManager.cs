using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Doublsb.Dialog;
using EZObjectPools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
//using XInputDotNetPure;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isPacificLevel;

    public ScriptableWeapons tableWeapons;
    public ScriptableItems tableItems;

    public DialogManager dialogManager;
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

    public GameObject barrelPrefab, breakingBarrelPrefab, bombPrefab, bombEffectPrefab, shootPrefab, hitPrefab;
    public EZObjectPool breakingBarrelsPool, bulletsPool, bombsPool, hitsPool, barrelsPool;

    [HideInInspector]
    public EZObjectPool bombEffectPool;

    //public AdmobManager admob;

    // GamePadState state;
    // GamePadState prevState;

    public bool gameIsActive = false;
    public int maxEnemies = 10;
    int enemyCounter = 0;

    public float minY = -1.9f, maxY = 0f;

    Transform[] enemiesT;

    CullingManager cullingManager;

    Db db;

    Stats basicEnemyStats = new Stats() { life = 6, strength = 1, velocity = 1 };

    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        //admob = new AdmobManager();
        //db = new Db();
        if (!isPacificLevel)
        {
            GeneratePools();
            SetMapSpriteOrders();
            AllocateEnemies();
        }
        SpawnPlayer("Player");

        SceneManager.sceneLoaded += OnSceneLoaded;
        //cullingManager = new CullingManager();
        //StartCoroutine(RutinaEnemigos());
    }

    private void Start()
    {
        if (!isPacificLevel) LagSpikesResolver();
    }

    int enemySpawnAmount = 2;
    int enemiesKilledThisRound = 0;
    int totalBattles = 1;

    bool battleIsActive = false;

    public void BattleStart()
    {
        battleIsActive = true;
        totalBattles++;
        enemySpawnAmount = Mathf.Clamp(totalBattles, 1, maxEnemies);
        enemiesKilledThisRound = 0;
        CameraController.Instance.SetBattleMode(!CameraController.Instance.battleMode);
        Camera mainCamera = Camera.main;
        StartCoroutine(MrSpawner(enemySpawnAmount, () =>
        {
            EnemiesManager.Instance.SetActionForAllEnemies();
        }));
    }

    public float timeLastBattleEnd = -1f;
    public void BattleEnd()
    {
        timeLastBattleEnd = Time.time;
        battleIsActive = false;
        CameraController.Instance.SetBattleMode(false);
    }

    public IEnumerator MrSpawner(int amount, Action onEnded)
    {
        for (var x = 0; x < enemySpawnAmount; x++)
        {
            SpawnEnemy(x + 1);
            yield return new WaitForEndOfFrame();
        }
        onEnded.Invoke();
    }

    private void LagSpikesResolver()
    {
        Vector2 origin = Vector2.up * 100;
        ShowHitEffect(origin);
        ShootWeapon(origin, Vector2.right);
        ShootBomb(origin);
    }

    private void GeneratePools()
    {
        bulletsPool = EZObjectPool.CreateObjectPool(shootPrefab, "Shoot1", 20, true, true, true);
        bombsPool = EZObjectPool.CreateObjectPool(bombPrefab, "Bombs", 1, true, true, true);
        bombEffectPool = EZObjectPool.CreateObjectPool(bombEffectPrefab, "BombEffect", 1, true, true, true);
        hitsPool = EZObjectPool.CreateObjectPool(hitPrefab, "HitEffect", 6, true, true, true);
        breakingBarrelsPool = EZObjectPool.CreateObjectPool(breakingBarrelPrefab, "BreakingBarrels", 2, true, true, true);
        barrelsPool = EZObjectPool.CreateObjectPool(barrelPrefab, "Barrels", 6, true, true, true);
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
                basicEnemyStats,
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

    public void ShowDialog(string[] texts)
    {
        List<DialogData> dialogList = new List<DialogData>();
        foreach (var text in texts) dialogList.Add(new DialogData(text, "Askito"));
        dialogManager.Show(dialogList);
    }

    private void Update()
    {
        if (gameIsActive)
        {
            Controls();
            player.Update();
        }
        if (!isPacificLevel)
        {
            CheckForBattleStart();
        }
    }

    public int distanceForBattle = 20;
    private void CheckForBattleStart()
    {
        if (player.transform.position.x > totalBattles * distanceForBattle && !battleIsActive && Time.time > timeLastBattleEnd + 3f) BattleStart();
    }

    private int GetNextEnemyID()
    {
        return enemyCounter + 1 < enemies.Length ? enemyCounter++ : 0;
    }

    public void SpawnEnemy(int enemyNumber)
    {
        bool spawnOnLeft = false;
        if (spawnOnLeft) enemyNumber++;
        var mainCamera = Camera.main;
        var finalPos = new Vector3(
                spawnOnLeft ?
                CameraController.Instance.maxDistanceLeft - enemyNumber - UnityEngine.Random.Range(0f, 1f) :
                CameraController.Instance.maxDistanceRight + enemyNumber + UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(minY, maxY),
                0
        );

        int enemyID = GetNextEnemyID();
        Enemy enemy = enemies[enemyID];

        if (enemy.status == Status.Alive)
        {
            enemy = enemies.ToList().Find(e => e.status == Status.Dead);
            if (enemy == null)
            {
                Debug.LogWarning("Esto no puede ser");
                return;
            }
        }

        Stats randomStats = basicEnemyStats;
        randomStats.velocity = UnityEngine.Random.Range(1, 3);
        enemy.Spawn(finalPos, randomStats);
        EquipManager.Instance.SetRandomEquipment(enemy.character);
        cullingManager?.SetSphere(enemy.ID, finalPos);
    }

    private void SpawnPlayer(string playerName)
    {
        playerTransform = Instantiate(playerGO, Vector3.zero, transform.rotation).transform;

        Player newPlayer = new Player(
            playerTransform,
            new Stats() { life = 20, strength = 1, velocity = 2 },
            tableWeapons.common[0],
            playerName
        );
        player = newPlayer;
        playerTransform.name = playerName;
        gameIsActive = true;
    }

    public void DeleteEnemy(int enemyID)
    {
        cullingManager?.RemoveSphere(enemyID);
        enemies[enemyID].character.isActive = false;
        //enemies[enemyID].status = Status.Dead;
    }

    private void SetMapSpriteOrders()
    {
        foreach (Transform breakablesT in mapBreakables)
        {
            SpriteRenderer renderer = breakablesT.GetComponent<SpriteRenderer>();
            SetSpriteOrder(renderer);
        }
    }

    public void SetSpriteOrder(SpriteRenderer renderer)
    {
        renderer.sortingOrder = renderer.sortingOrder - (int)(renderer.transform.position.y * 100);
    }

    public void SetSpriteOrder(SpriteRenderer renderer, int defaultSpriteOrder)
    {
        renderer.sortingOrder = defaultSpriteOrder - (int)(renderer.transform.position.y * 100);
    }

    private void SetCheatStats()
    {
        player.stats = cheatStats;
    }

    public void RestartScene()
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

    public void ShootBomb(Vector2 position)
    {
        if (bombsPool.TryGetNextObject(position, transform.rotation, out GameObject bomb))
        {
            bomb.SetActive(true);
        }
    }

    public void BreakBreakable(Transform oldBreakable, Vector2 hitDir)
    {
        string breakableName = oldBreakable.name.Split(' ')[0];
        if (breakableName.Contains("Barrel"))
        {
            int breakableSortingOrder = oldBreakable.GetComponent<SpriteRenderer>().sortingOrder;
            if (breakingBarrelsPool.TryGetNextObject(oldBreakable.position, oldBreakable.rotation, out GameObject newBreaking))
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
        entity.PlayAnim("Alert");
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

    public void RemoveEnemy(Enemy enemy)
    {
        enemy.status = Status.Dead;
        StartCoroutine(ReciclateEntity(enemy));
        enemiesKilledThisRound++;
        if (enemiesKilledThisRound == enemySpawnAmount) BattleEnd();
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
        Vibration.Vibrate(16 + force);
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
                    GetEnemyByName(hit.transform.gameObject.name)?.GetStrike(10, hitDir);
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
                            VibrationForce(15);
                        }

                        else if (hitLayer == enemyMask)
                        {
                            if (myself.isPlayer)
                            {
                                Entity enemy = GameManager.Instance.GetEnemyByName(hit.transform.name);
                                if (enemy != null)
                                {
                                    VibrationForce(20);
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
                            myself.ApplyImpulse(-attackDir / 4);
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
        cullingManager?.Dispose();
    }

    public bool gameIsOver = false;
    float timeLastGameOver = 0f;
    public void GameOver()
    {
        gameIsOver = true;
        timeLastGameOver = Time.time;
        SoundManager.Instance.PlayGameOver();
        CanvasManager.Instance.ShowRetry();
        EnemiesManager.Instance.StopAllEnemies();
    }

    public bool isDialogActive = false;
    public void Interact()
    {
        if (isDialogActive) dialogManager.Click_Window();
        else
        {
            RaycastHit2D[] results = Physics2D.CircleCastAll(player.transform.position, 0.5f, player.transform.right, 1f, interactableMask);
            if (results.Length > 0)
            {
                results[0].transform.SendMessage("OnPlayerInteraction");
            }
        }
    }

    public void LoadNewScene(string sceneName)
    {
        player.isActive = false;
        player.Idle();
        StartCoroutine(CanvasManager.Instance.FadeIn(() =>
        {
            SceneManager.LoadScene(sceneName);
        }));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameManager.Instance.StartCoroutine(CanvasManager.Instance.FadeOut());
        player.isActive = true;
    }

    int wIndex = 0;
    public LayerMask interactableMask;

    private void Controls()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Attack") && gameIsOver && Time.time > timeLastGameOver + CanvasManager.Instance.delayRetryButton) RestartScene();

        if (Input.GetButtonDown("Attack") && isPacificLevel) Interact();

        if (player.isActive)
        {
            if (Mathf.Abs(xAxis) > 0.0f || Mathf.Abs(yAxis) > 0.0f)
            {
                if (Input.GetButton("Fire2")) player.MoveToDirection(new Vector2(xAxis * 1.5f, yAxis * 1.5f));
                else player.MoveToDirection(new Vector2(xAxis, yAxis));

            }
            else player.Idle();

            if (Input.GetButtonDown("Attack") && !isPacificLevel) player.Attack(new Vector2(xAxis, yAxis));
            else if (Input.GetButtonDown("Fire2"))
            {
                if (new Vector2(xAxis, yAxis).normalized == Vector2.zero)
                {
                    player.PrepareCounterAttack();
                }
                else
                {
                    player.Dash(new Vector2(xAxis, yAxis).normalized * 1.5f);
                }
            }
            //else if (Input.GetButtonDown("Fire3")) player.ShootWeapon();
        }
    }
}