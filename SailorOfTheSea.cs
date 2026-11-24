
using UnityEngine;
using System.Collections.Generic;

public class SailorOfTheSea : MonoBehaviour
{
    // =========================================================
    // SAILOR OF THE SEA
    // A procedural 3D pirate adventure RPG
    // Single-file Unity implementation
    // =========================================================

    [Header("PLAYER")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 8f;
    public float gravity = 22f;
    public float maxHealth = 100f;
    public float health = 100f;
    public float maxStamina = 100f;
    public float stamina = 100f;

    [Header("COMBAT")]
    public float swordDamage = 25f;
    public float attackRange = 4f;
    public float attackCooldown = 0.5f;
    public float abilityDamage = 50f;
    public float abilityRange = 12f;
    public float abilityCooldown = 8f;

    [Header("PROGRESSION")]
    public int level = 1;
    public int xp = 0;
    public int bounty = 0;
    public int coins = 100;

    [Header("WORLD")]
    public int islandCount = 8;
    public float oceanSize = 1200f;
    public int enemiesPerIsland = 5;

    [Header("SHIP")]
    public float shipSpeed = 18f;
    public float shipTurnSpeed = 65f;

    // =========================================================
    // INTERNAL VARIABLES
    // =========================================================

    private CharacterController controller;
    private GameObject player;
    private GameObject mainCamera;
    private GameObject ship;
    private Transform cameraTransform;

    private Vector3 velocity;
    private Vector3 spawnPoint;

    private bool grounded;
    private bool sailing;
    private bool inventoryOpen;
    private bool questActive = true;

    private float nextAttack;
    private float nextAbility;
    private float enemyAttackTimer;
    private float cameraYaw;
    private float cameraPitch = 20f;

    private int enemiesDefeated;
    private int questTarget = 3;
    private int treasuresFound;

    private string notification = "Welcome, sailor!";
    private float notificationTimer = 4f;

    private string playerName = "Unknown Captain";
    private string fruitName = "Storm Core";

    private List<GameObject> islands = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();
    private List<GameObject> treasures = new List<GameObject>();
    private List<string> inventory = new List<string>();

    private Dictionary<GameObject, float> enemyHealth =
        new Dictionary<GameObject, float>();

    private Dictionary<GameObject, float> enemyAttackTimes =
        new Dictionary<GameObject, float>();

    private Dictionary<GameObject, int> enemyRewards =
        new Dictionary<GameObject, int>();

    private GUIStyle titleStyle;
    private GUIStyle normalStyle;
    private GUIStyle smallStyle;
    private GUIStyle buttonStyle;

    private bool initialized;

    // =========================================================
    // GAME INITIALIZATION
    // =========================================================

    void Start()
    {
        Application.targetFrameRate = 60;

        InitializeWorld();
        InitializePlayer();
        InitializeCamera();
        InitializeShip();
        InitializeUI();

        GenerateIslands();
        GenerateEnemies();
        GenerateTreasures();

        initialized = true;

        Notify("SAILOR OF THE SEA - YOUR JOURNEY BEGINS");
    }

    void InitializeWorld()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor =
            new Color(0.55f, 0.72f, 0.82f);

        RenderSettings.fogDensity = 0.0007f;

        RenderSettings.ambientLight =
            new Color(0.65f, 0.72f, 0.8f);

        GameObject sun = new GameObject("Sun");
        Light light = sun.AddComponent<Light>();

        light.type = LightType.Directional;
        light.intensity = 1.3f;

        sun.transform.rotation =
            Quaternion.Euler(45f, -35f, 0f);

        // Ocean
        GameObject ocean = GameObject.CreatePrimitive(
            PrimitiveType.Plane
        );

        ocean.name = "Infinite Ocean";

        ocean.transform.position = Vector3.zero;
        ocean.transform.localScale =
            new Vector3(oceanSize / 10f, 1f, oceanSize / 10f);

        Renderer oceanRenderer = ocean.GetComponent<Renderer>();

        Material water = new Material(
            Shader.Find("Standard")
        );

        water.color = new Color(0.025f, 0.28f, 0.48f);
        water.SetFloat("_Glossiness", 0.9f);

        oceanRenderer.material = water;

        // Sky
        Camera.main?.gameObject.SetActive(false);
    }

    // =========================================================
    // PLAYER CREATION
    // =========================================================

    void InitializePlayer()
    {
        player = GameObject.CreatePrimitive(
            PrimitiveType.Capsule
        );

        player.name = "Captain";

        player.transform.position =
            new Vector3(0f, 4f, 0f);

        Destroy(player.GetComponent<CapsuleCollider>());

        controller = player.AddComponent<CharacterController>();

        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = Vector3.zero;
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.3f;

        Renderer renderer = player.GetComponent<Renderer>();

        renderer.material.color =
            new Color(0.12f, 0.16f, 0.22f);

        spawnPoint = player.transform.position;

        player.tag = "Player";
    }

    // =========================================================
    // CAMERA SYSTEM
    // =========================================================

    void InitializeCamera()
    {
        mainCamera = new GameObject("Game Camera");

        Camera cam = mainCamera.AddComponent<Camera>();

        cam.fieldOfView = 70f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1500f;

        mainCamera.AddComponent<AudioListener>();

        cameraTransform = mainCamera.transform;

        cameraYaw = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateCamera()
    {
        if (inventoryOpen)
            return;

        if (Input.GetMouseButton(1))
        {
            cameraYaw += Input.GetAxis("Mouse X") * 3f;
            cameraPitch -= Input.GetAxis("Mouse Y") * 2f;

            cameraPitch = Mathf.Clamp(
                cameraPitch, 5f, 65f
            );
        }

        Quaternion rotation = Quaternion.Euler(
            cameraPitch,
            cameraYaw,
            0f
        );

        Vector3 target = player.transform.position +
                         Vector3.up * 1.5f;

        Vector3 cameraPosition = target -
            rotation * Vector3.forward * 7f;

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            cameraPosition,
            Time.deltaTime * 10f
        );

        cameraTransform.LookAt(target);
    }

    // =========================================================
    // PLAYER MOVEMENT
    // =========================================================

    void UpdatePlayer()
    {
        if (sailing)
        {
            UpdateShip();
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = Quaternion.Euler(
            0f, cameraYaw, 0f
        ) * Vector3.forward;

        Vector3 right = Quaternion.Euler(
            0f, cameraYaw, 0f
        ) * Vector3.right;

        Vector3 direction =
            forward * vertical + right * horizontal;

        direction = Vector3.ClampMagnitude(direction, 1f);

        bool sprinting =
            Input.GetKey(KeyCode.LeftShift) &&
            stamina > 0f &&
            direction.magnitude > 0.1f;

        float speed = sprinting
            ? sprintSpeed
            : walkSpeed;

        if (sprinting)
        {
            stamina -= 20f * Time.deltaTime;
        }
        else
        {
            stamina += 12f * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            player.transform.rotation =
                Quaternion.Slerp(
                    player.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 12f
                );
        }

        grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            velocity.y = jumpForce;
        }

        velocity.y -= gravity * Time.deltaTime;

        Vector3 movement =
            direction * speed + Vector3.up * velocity.y;

        controller.Move(movement * Time.deltaTime);

        if (player.transform.position.y < -20f)
            Respawn();
    }

    // =========================================================
    // SHIP SYSTEM
    // =========================================================

    void InitializeShip()
    {
        ship = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        ship.name = "Captain's Ship";

        ship.transform.position =
            new Vector3(8f, 1f, 12f);

        ship.transform.localScale =
            new Vector3(4f, 1f, 9f);

        ship.GetComponent<Renderer>().material.color =
            new Color(0.35f, 0.17f, 0.07f);

        GameObject mast = GameObject.CreatePrimitive(
            PrimitiveType.Cylinder
        );

        mast.name = "Ship Mast";
        mast.transform.SetParent(ship.transform);

        mast.transform.localPosition =
            new Vector3(0f, 2.5f, 0f);

        mast.transform.localScale =
            new Vector3(0.15f, 2.5f, 0.15f);

        mast.GetComponent<Renderer>().material.color =
            new Color(0.22f, 0.12f, 0.06f);

        GameObject sail = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        sail.transform.SetParent(ship.transform);

        sail.transform.localPosition =
            new Vector3(0f, 4f, 0f);

        sail.transform.localScale =
            new Vector3(2.5f, 2f, 0.1f);

        sail.GetComponent<Renderer>().material.color =
            Color.white;
    }

    void UpdateShip()
    {
        float forward = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        ship.transform.Translate(
            Vector3.forward * forward *
            shipSpeed * Time.deltaTime
        );

        ship.transform.Rotate(
            Vector3.up * turn *
            shipTurnSpeed * Time.deltaTime
        );

        player.transform.position =
            ship.transform.position +
            ship.transform.up * 1.5f;

        if (Input.GetKeyDown(KeyCode.E))
        {
            sailing = false;

            player.transform.position =
                ship.transform.position +
                ship.transform.right * 5f +
                Vector3.up * 2f;

            Notify("You left the ship.");
        }
    }

    void TryBoardShip()
    {
        if (Vector3.Distance(
            player.transform.position,
            ship.transform.position
        ) < 8f)
        {
            sailing = true;
            velocity = Vector3.zero;

            Notify("You boarded your ship. Sail!");
        }
    }

    // =========================================================
    // PROCEDURAL ISLAND GENERATION
    // =========================================================

    void GenerateIslands()
    {
        for (int i = 0; i < islandCount; i++)
        {
            float angle =
                i * Mathf.PI * 2f / islandCount;

            float distance = 100f + Random.Range(0f, 50f);

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            CreateIsland(position, i);
        }
    }

    void CreateIsland(Vector3 position, int index)
    {
        GameObject island = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        island.name = "Island " + (index + 1);

        float size = Random.Range(18f, 30f);

        island.transform.position =
            position + Vector3.down * 5f;

        island.transform.localScale =
            new Vector3(size * 2f, 10f, size * 2f);

        island.GetComponent<Renderer>().material.color =
            new Color(0.75f, 0.65f, 0.35f);

        islands.Add(island);

        // Grassland
        GameObject grass = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        grass.name = "Island Grass";

        grass.transform.position =
            position + Vector3.down * 1.5f;

        grass.transform.localScale =
            new Vector3(size * 1.7f, 4f, size * 1.7f);

        grass.GetComponent<Renderer>().material.color =
            new Color(
                Random.Range(0.1f, 0.25f),
                Random.Range(0.35f, 0.65f),
                0.16f
            );

        // Trees
        for (int t = 0; t < 12; t++)
        {
            Vector3 treePosition =
                position + new Vector3(
                    Random.Range(-size * 0.7f, size * 0.7f),
                    0f,
                    Random.Range(-size * 0.7f, size * 0.7f)
                );

            CreateTree(treePosition);
        }

        // Village
        CreateVillage(position, size);
    }

    void CreateTree(Vector3 position)
    {
        GameObject trunk = GameObject.CreatePrimitive(
            PrimitiveType.Cylinder
        );

        trunk.name = "Palm Tree Trunk";

        trunk.transform.position =
            position + Vector3.up * 2f;

        trunk.transform.localScale =
            new Vector3(0.5f, 2f, 0.5f);

        trunk.GetComponent<Renderer>().material.color =
            new Color(0.32f, 0.17f, 0.07f);

        GameObject leaves = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        leaves.name = "Tree Leaves";

        leaves.transform.position =
            position + Vector3.up * 5f;

        leaves.transform.localScale =
            new Vector3(3f, 2.5f, 3f);

        leaves.GetComponent<Renderer>().material.color =
            new Color(0.1f, 0.38f, 0.12f);
    }

    void CreateVillage(Vector3 position, float size)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject house = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            house.name = "Island House";

            house.transform.position =
                position + new Vector3(
                    i * 6f - 6f,
                    3f,
                    -size * 0.4f
                );

            house.transform.localScale =
                new Vector3(5f, 6f, 5f);

            house.GetComponent<Renderer>().material.color =
                new Color(0.65f, 0.4f, 0.2f);
        }
    }

    // =========================================================
    // ENEMY SYSTEM
    // =========================================================

    void GenerateEnemies()
    {
        for (int i = 0; i < islandCount; i++)
        {
            Vector3 center =
                islands[i].transform.position +
                Vector3.up * 8f;

            for (int j = 0; j < enemiesPerIsland; j++)
            {
                Vector3 position = center +
                    new Vector3(
                        Random.Range(-18f, 18f),
                        0f,
                        Random.Range(-18f, 18f)
                    );

                CreateEnemy(position);
            }
        }
    }

    void CreateEnemy(Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(
            PrimitiveType.Capsule
        );

        enemy.name = "Rogue Pirate";

        enemy.transform.position = position;

        enemy.GetComponent<Renderer>().material.color =
            new Color(0.55f, 0.12f, 0.12f);

        enemies.Add(enemy);

        enemyHealth[enemy] = 100f;
        enemyAttackTimes[enemy] = 0f;
        enemyRewards[enemy] = 50;
    }

    void UpdateEnemies()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = enemies[i];

            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            if (sailing)
                continue;

            float distance = Vector3.Distance(
                enemy.transform.position,
                player.transform.position
            );

            if (distance < 20f && distance > 2f)
            {
                Vector3 direction =
                    player.transform.position -
                    enemy.transform.position;

                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                {
                    enemy.transform.rotation =
                        Quaternion.Slerp(
                            enemy.transform.rotation,
                            Quaternion.LookRotation(direction),
                            Time.deltaTime * 5f
                        );

                    enemy.transform.position =
                        Vector3.MoveTowards(
                            enemy.transform.position,
                            player.transform.position,
                            2.5f * Time.deltaTime
                        );
                }
            }

            if (distance <= 2.5f &&
                Time.time > enemyAttackTimes[enemy])
            {
                health -= 8f;

                enemyAttackTimes[enemy] =
                    Time.time + 1.5f;

                Notify("An enemy pirate attacked you!");

                if (health <= 0)
                    Respawn();
            }
        }
    }

    // =========================================================
    // SWORD COMBAT
    // =========================================================

    void PlayerAttack()
    {
        if (Time.time < nextAttack || sailing)
            return;

        nextAttack = Time.time + attackCooldown;

        GameObject target = null;
        float closest = attackRange;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(
                player.transform.position,
                enemy.transform.position
            );

            Vector3 direction =
                enemy.transform.position -
                player.transform.position;

            float angle = Vector3.Angle(
                player.transform.forward,
                direction
            );

            if (distance < closest && angle < 65f)
            {
                closest = distance;
                target = enemy;
            }
        }

        if (target != null)
        {
            DamageEnemy(target, swordDamage);
            Notify("Sword strike!");
        }
    }

    void DamageEnemy(GameObject enemy, float damage)
    {
        if (enemy == null ||
            !enemyHealth.ContainsKey(enemy))
            return;

        enemyHealth[enemy] -= damage;

        // Damage feedback
        Renderer renderer = enemy.GetComponent<Renderer>();

        if (renderer != null)
            renderer.material.color = Color.yellow;

        if (enemyHealth[enemy] <= 0)
            DefeatEnemy(enemy);
    }

    void DefeatEnemy(GameObject enemy)
    {
        int reward = enemyRewards[enemy];

        coins += reward;
        bounty += 100;
        enemiesDefeated++;

        GainXP(50);

        enemyHealth.Remove(enemy);
        enemyAttackTimes.Remove(enemy);
        enemyRewards.Remove(enemy);

        enemies.Remove(enemy);

        Destroy(enemy);

        Notify("Pirate defeated! +" + reward + " coins");

        CheckQuest();
    }

    // =========================================================
    // SPECIAL ABILITY
    // =========================================================

    void UseAbility()
    {
        if (Time.time < nextAbility || sailing)
            return;

        nextAbility = Time.time + abilityCooldown;

        int hits = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(
                player.transform.position,
                enemy.transform.position
            );

            if (distance <= abilityRange)
            {
                DamageEnemy(enemy, abilityDamage);
                hits++;
            }
        }

        Notify(
            fruitName + " activated! Enemies hit: " + hits
        );

        CreateAbilityEffect();
    }

    void CreateAbilityEffect()
    {
        GameObject effect = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        effect.name = "Storm Burst";

        effect.transform.position =
            player.transform.position;

        effect.transform.localScale =
            Vector3.one * abilityRange * 2f;

        Collider collider = effect.GetComponent<Collider>();

        Destroy(collider);

        effect.GetComponent<Renderer>().material.color =
            new Color(0.1f, 0.7f, 1f, 0.35f);

        Destroy(effect, 0.25f);
    }

    // =========================================================
    // TREASURE SYSTEM
    // =========================================================

    void GenerateTreasures()
    {
        foreach (GameObject island in islands)
        {
            Vector3 position =
                island.transform.position +
                new Vector3(0f, 9f, 12f);

            GameObject chest = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            chest.name = "Treasure Chest";

            chest.transform.position = position;

            chest.transform.localScale =
                new Vector3(2f, 1.5f, 1.5f);

            chest.GetComponent<Renderer>().material.color =
                new Color(0.55f, 0.32f, 0.08f);

            treasures.Add(chest);
        }
    }

    void TryLootTreasure()
    {
        for (int i = treasures.Count - 1; i >= 0; i--)
        {
            GameObject chest = treasures[i];

            if (chest == null) continue;

            float distance = Vector3.Distance(
                player.transform.position,
                chest.transform.position
            );

            if (distance <= 5f)
            {
                int reward = Random.Range(100, 501);

                coins += reward;
                treasuresFound++;

                inventory.Add("Treasure");

                Destroy(chest);
                treasures.RemoveAt(i);

                Notify("Treasure discovered! +" +
                       reward + " coins");

                return;
            }
        }
    }

    // =========================================================
    // QUEST SYSTEM
    // =========================================================

    void CheckQuest()
    {
        if (!questActive) return;

        if (enemiesDefeated >= questTarget)
        {
            questActive = false;

            coins += 500;
            bounty += 500;

            GainXP(200);

            Notify("QUEST COMPLETE! 500 bonus coins!");
        }
    }

    // =========================================================
    // LEVELING
    // =========================================================

    void GainXP(int amount)
    {
        xp += amount;

        while (xp >= level * 100)
        {
            xp -= level * 100;
            level++;

            maxHealth += 10f;
            health = maxHealth;

            swordDamage += 2f;

            Notify("LEVEL UP! Level " + level);
        }
    }

    // =========================================================
    // INVENTORY
    // =========================================================

    void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;

        Cursor.visible = inventoryOpen;

        Cursor.lockState = inventoryOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    // =========================================================
    // RESPAWN
    // =========================================================

    void Respawn()
    {
        health = maxHealth;
        stamina = maxStamina;

        sailing = false;

        player.transform.position = spawnPoint;

        velocity = Vector3.zero;

        Notify("You returned to your starting island.");
    }

    // =========================================================
    // NOTIFICATIONS
    // =========================================================

    void Notify(string message)
    {
        notification = message;
        notificationTimer = 3.5f;
    }

    // =========================================================
    // INPUT MANAGEMENT
    // =========================================================

    void Update()
    {
        if (!initialized) return;

        if (notificationTimer > 0)
            notificationTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();

        if (inventoryOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            PlayerAttack();

        if (Input.GetKeyDown(KeyCode.Q))
            UseAbility();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (sailing)
                return;

            float shipDistance = Vector3.Distance(
                player.transform.position,
                ship.transform.position
            );

            if (shipDistance < 8f)
                TryBoardShip();
            else
                TryLootTreasure();
        }

        UpdatePlayer();
        UpdateCamera();
        UpdateEnemies();
    }

    // =========================================================
    // UI INITIALIZATION
    // =========================================================

    void InitializeUI()
    {
        titleStyle = new GUIStyle();

        titleStyle.fontSize = 27;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        normalStyle = new GUIStyle();

        normalStyle.fontSize = 17;
        normalStyle.normal.textColor = Color.white;

        smallStyle = new GUIStyle();

        smallStyle.fontSize = 13;
        smallStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button);

        buttonStyle.fontSize = 16;
    }

    // =========================================================
    // HUD
    // =========================================================

    void DrawBar(
        string label,
        float value,
        float maximum,
        float x,
        float y,
        Color color
    )
    {
        GUI.color = new Color(0f, 0f, 0f, 0.7f);

        GUI.Box(
            new Rect(x, y, 220, 22),
            ""
        );

        GUI.color = color;

        GUI.DrawTexture(
            new Rect(
                x + 3,
                y + 3,
                214f * Mathf.Clamp01(value / maximum),
                16
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        GUI.Label(
            new Rect(x + 7, y, 210, 22),
            label + ": " +
            Mathf.CeilToInt(value) + "/" +
            maximum,
            smallStyle
        );
    }

    void OnGUI()
    {
        if (!initialized) return;

        GUI.color = Color.white;

        // Main HUD
        GUI.Box(
            new Rect(15, 15, 245, 145),
            ""
        );

        GUI.Label(
            new Rect(25, 20, 230, 35),
            "SAILOR OF THE SEA",
            titleStyle
        );

        DrawBar(
            "Health", health, maxHealth,
            25, 65, Color.red
        );

        DrawBar(
            "Stamina", stamina, maxStamina,
            25, 92, Color.green
        );

        GUI.Label(
            new Rect(25, 120, 230, 25),
            "LEVEL " + level + "  |  XP " + xp,
            normalStyle
        );

        // Currency and bounty
        GUI.Box(
            new Rect(15, 170, 245, 95),
            ""
        );

        GUI.Label(
            new Rect(25, 180, 220, 25),
            "Coins: " + coins,
            normalStyle
        );

        GUI.Label(
            new Rect(25, 210, 220, 25),
            "Bounty: " + bounty + " Berries",
            normalStyle
        );

        GUI.Label(
            new Rect(25, 238, 220, 25),
            "Defeated: " + enemiesDefeated,
            smallStyle
        );

        // Quest
        GUI.Box(
            new Rect(Screen.width - 280, 15, 265, 120),
            ""
        );

        GUI.Label(
            new Rect(
                Screen.width - 265,
                25,
                245,
                30
            ),
            "CURRENT QUEST",
            titleStyle
        );

        GUI.Label(
            new Rect(
                Screen.width - 265,
                65,
                245,
                55
            ),
            questActive
                ? "Defeat 3 Rogue Pirates\nProgress: " +
                  Mathf.Min(enemiesDefeated, questTarget) +
                  "/" + questTarget
                : "Quest completed!",
            normalStyle
        );

        // Controls
        GUI.Box(
            new Rect(
                Screen.width - 230,
                Screen.height - 185,
                215,
                170
            ),
            ""
        );

        GUI.Label(
            new Rect(
                Screen.width - 215,
                Screen.height - 175,
                200,
                150
            ),
            "CONTROLS\n\n" +
            "WASD - Movement\n" +
            "SHIFT - Sprint\n" +
            "SPACE - Jump\n" +
            "MOUSE - Sword Attack\n" +
            "Q - Storm Ability\n" +
            "E - Interact / Ship\n" +
            "TAB - Inventory\n" +
            "RIGHT CLICK - Camera",
            smallStyle
        );

        // Notifications
        if (notificationTimer > 0)
        {
            GUI.color = new Color(1f, 1f, 1f, 1f);

            GUI.Box(
                new Rect(
                    Screen.width / 2f - 200,
                    80,
                    400,
                    45
                ),
                ""
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 190,
                    90,
                    380,
                    30
                ),
                notification,
                normalStyle
            );
        }

        // Inventory
        if (inventoryOpen)
        {
            GUI.color = Color.white;

            GUI.Box(
                new Rect(
                    Screen.width / 2f - 220,
                    Screen.height / 2f - 180,
                    440,
                    360
                ),
                ""
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 195,
                    Screen.height / 2f - 160,
                    390,
                    40
                ),
                "CAPTAIN'S INVENTORY",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 195,
                    Screen.height / 2f - 105,
                    390,
                    30
                ),
                "Coins: " + coins,
                normalStyle
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 195,
                    Screen.height / 2f - 65,
                    390,
                    30
                ),
                "Fruit: " + fruitName,
                normalStyle
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 195,
                    Screen.height / 2f - 25,
                    390,
                    30
                ),
                "Treasures: " + treasuresFound,
                normalStyle
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 195,
                    Screen.height / 2f + 20,
                    390,
                    100
                ),
                "Items:\n" + string.Join("\n", inventory),
                normalStyle
            );

            if (GUI.Button(
                new Rect(
                    Screen.width / 2f - 70,
                    Screen.height / 2f + 125,
                    140,
                    35
                ),
                "CLOSE"
            ))
            {
                ToggleInventory();
            }
        }

        // Sailing indicator
        if (sailing)
        {
            GUI.Label(
                new Rect(
                    Screen.width / 2f - 100,
                    Screen.height - 65,
                    200,
                    30
                ),
                "SAILING MODE",
                titleStyle
            );
        }

        GUI.color = Color.white;
    }
}
