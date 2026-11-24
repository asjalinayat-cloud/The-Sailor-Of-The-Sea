
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class SailorOfTheSea : MonoBehaviour
{
    // ============================================================
    // SAILOR OF THE SEA
    // ADVANCED SINGLE-FILE 3D PIRATE RPG
    // ============================================================

    // -------------------- CONFIGURATION -------------------------

    [Header("PLAYER")]
    public string captainName = "Captain";
    public float walkSpeed = 5.5f;
    public float runSpeed = 9f;
    public float jumpPower = 8f;
    public float gravity = 22f;
    public float rotationSpeed = 14f;

    [Header("PLAYER STATS")]
    public float maxHealth = 150f;
    public float health = 150f;
    public float maxStamina = 120f;
    public float stamina = 120f;
    public float maxEnergy = 100f;
    public float energy = 100f;

    [Header("COMBAT")]
    public float swordDamage = 30f;
    public float swordRange = 4f;
    public float swordCooldown = 0.55f;
    public float abilityDamage = 65f;
    public float abilityRange = 13f;
    public float abilityCost = 25f;

    [Header("PROGRESSION")]
    public int level = 1;
    public int experience = 0;
    public int bounty = 0;
    public int berries = 500;
    public int skillPoints = 0;

    [Header("WORLD")]
    public int islandCount = 9;
    public int treesPerIsland = 24;
    public int enemiesPerIsland = 5;
    public float worldRadius = 350f;

    [Header("SHIP")]
    public float sailingSpeed = 22f;
    public float sailingTurnSpeed = 65f;

    // -------------------- OBJECT REFERENCES ---------------------

    private GameObject player;
    private GameObject characterModel;
    private GameObject cameraObject;
    private GameObject ship;

    private CharacterController characterController;
    private Transform cameraTransform;
    private Transform modelTransform;

    private Vector3 movementVelocity;
    private Vector3 lastMovement;
    private Vector3 spawnPosition;

    // -------------------- GAME STATE ----------------------------

    private bool initialized;
    private bool sailing;
    private bool inventoryOpen;
    private bool paused;
    private bool attacking;
    private bool jumping;

    private float cameraYaw;
    private float cameraPitch = 18f;
    private float attackTimer;
    private float abilityTimer;
    private float interactionTimer;
    private float animationTime;

    private int enemiesDefeated;
    private int treasureCount;
    private int islandsDiscovered;

    private string notification = "Welcome to the Grand Ocean.";
    private float notificationTimer = 5f;

    private string fruitName = "Storm Core";
    private string currentIsland = "Starting Island";

    // -------------------- WORLD DATA ----------------------------

    private readonly List<GameObject> islands =
        new List<GameObject>();

    private readonly List<GameObject> enemies =
        new List<GameObject>();

    private readonly List<GameObject> treasures =
        new List<GameObject>();

    private readonly List<GameObject> trees =
        new List<GameObject>();

    private readonly List<string> inventory =
        new List<string>();

    private readonly Dictionary<GameObject, float> enemyHealth =
        new Dictionary<GameObject, float>();

    private readonly Dictionary<GameObject, float> enemyCooldown =
        new Dictionary<GameObject, float>();

    private readonly Dictionary<GameObject, int> enemyRewards =
        new Dictionary<GameObject, int>();

    private readonly Dictionary<GameObject, string> islandNames =
        new Dictionary<GameObject, string>();

    private readonly string[] islandNamePool =
    {
        "Emerald Bay",
        "Whispering Palm",
        "Crimson Coast",
        "Golden Haven",
        "Stormbreaker",
        "Moonlight Island",
        "The Lost Kingdom",
        "Azure Paradise",
        "Skull Island"
    };

    private readonly string[] enemyNames =
    {
        "Rogue Pirate",
        "Sea Raider",
        "Island Bandit",
        "Black Flag Captain",
        "Deserted Soldier"
    };

    private readonly string[] treasureItems =
    {
        "Ancient Map",
        "Golden Compass",
        "Royal Crown",
        "Mystery Relic",
        "Pirate's Journal",
        "Golden Necklace"
    };

    // -------------------- MATERIALS -----------------------------

    private Material oceanMaterial;
    private Material sandMaterial;
    private Material grassMaterial;
    private Material woodMaterial;
    private Material leafMaterial;
    private Material playerMaterial;
    private Material enemyMaterial;
    private Material goldMaterial;
    private Material steelMaterial;

    // -------------------- UI STYLES -----------------------------

    private GUIStyle titleStyle;
    private GUIStyle headingStyle;
    private GUIStyle normalStyle;
    private GUIStyle smallStyle;
    private GUIStyle centerStyle;
    private GUIStyle buttonStyle;

    // ============================================================
    // INITIALIZATION
    // ============================================================

    void Start()
    {
        Application.targetFrameRate = 60;

        CreateMaterials();
        CreateLighting();
        CreateOcean();
        CreatePlayer();
        CreateCamera();
        CreateShip();
        CreateWorld();
        CreateInterface();

        initialized = true;

        Notify("YOUR ADVENTURE BEGINS");
    }

    void CreateMaterials()
    {
        oceanMaterial = MakeMaterial(
            new Color(0.025f, 0.20f, 0.36f), 0.9f
        );

        sandMaterial = MakeMaterial(
            new Color(0.78f, 0.66f, 0.42f), 0.1f
        );

        grassMaterial = MakeMaterial(
            new Color(0.19f, 0.39f, 0.17f), 0.1f
        );

        woodMaterial = MakeMaterial(
            new Color(0.32f, 0.16f, 0.075f), 0.15f
        );

        leafMaterial = MakeMaterial(
            new Color(0.10f, 0.36f, 0.12f), 0.1f
        );

        playerMaterial = MakeMaterial(
            new Color(0.12f, 0.19f, 0.25f), 0.15f
        );

        enemyMaterial = MakeMaterial(
            new Color(0.48f, 0.10f, 0.11f), 0.1f
        );

        goldMaterial = MakeMaterial(
            new Color(0.95f, 0.63f, 0.08f), 0.7f
        );

        steelMaterial = MakeMaterial(
            new Color(0.65f, 0.72f, 0.78f), 0.8f
        );
    }

    Material MakeMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");

        Material material = new Material(shader);

        material.color = color;

        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", smoothness);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", smoothness);

        return material;
    }

    void CreateLighting()
    {
        GameObject sun = new GameObject("World Sun");

        Light sunlight = sun.AddComponent<Light>();

        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.35f;
        sunlight.color = new Color(1f, 0.92f, 0.79f);

        sun.transform.rotation =
            Quaternion.Euler(48f, -35f, 0f);

        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Flat;

        RenderSettings.ambientLight =
            new Color(0.55f, 0.61f, 0.68f);

        RenderSettings.fog = true;
        RenderSettings.fogColor =
            new Color(0.55f, 0.73f, 0.83f);

        RenderSettings.fogDensity = 0.00035f;

        if (Camera.main != null)
            Camera.main.gameObject.SetActive(false);
    }

    // ============================================================
    // OCEAN
    // ============================================================

    void CreateOcean()
    {
        GameObject ocean = GameObject.CreatePrimitive(
            PrimitiveType.Plane
        );

        ocean.name = "Grand Ocean";

        ocean.transform.position =
            new Vector3(0f, -0.5f, 0f);

        ocean.transform.localScale =
            new Vector3(100f, 1f, 100f);

        ocean.GetComponent<Renderer>().material =
            oceanMaterial;

        // Remove collision from the ocean.
        Destroy(ocean.GetComponent<Collider>());
    }

    // ============================================================
    // PLAYER MODEL
    // ============================================================

    void CreatePlayer()
    {
        player = new GameObject("Player Controller");

        player.transform.position =
            new Vector3(0f, 2f, 0f);

        characterController =
            player.AddComponent<CharacterController>();

        characterController.height = 2.2f;
        characterController.radius = 0.4f;
        characterController.center =
            new Vector3(0f, 1.1f, 0f);

        characterController.slopeLimit = 50f;
        characterController.stepOffset = 0.35f;

        spawnPosition = player.transform.position;

        characterModel = new GameObject("Captain Character");

        characterModel.transform.SetParent(
            player.transform
        );

        characterModel.transform.localPosition =
            Vector3.zero;

        modelTransform = characterModel.transform;

        BuildCharacter();
    }

    void BuildCharacter()
    {
        // Main torso
        GameObject torso = MakePart(
            "Captain Torso",
            PrimitiveType.Capsule,
            modelTransform,
            new Vector3(0f, 1.15f, 0f),
            new Vector3(0.82f, 0.62f, 0.46f),
            playerMaterial
        );

        // Coat
        GameObject coat = MakePart(
            "Pirate Coat",
            PrimitiveType.Cube,
            modelTransform,
            new Vector3(0f, 1.05f, -0.12f),
            new Vector3(0.95f, 1.15f, 0.30f),
            MakeMaterial(new Color(0.055f, 0.15f, 0.20f), 0.1f)
        );

        // Head
        MakePart(
            "Head",
            PrimitiveType.Sphere,
            modelTransform,
            new Vector3(0f, 2.05f, 0f),
            new Vector3(0.58f, 0.68f, 0.56f),
            MakeMaterial(new Color(0.77f, 0.54f, 0.37f), 0.1f)
        );

        // Hair
        MakePart(
            "Hair",
            PrimitiveType.Sphere,
            modelTransform,
            new Vector3(0f, 2.34f, -0.03f),
            new Vector3(0.62f, 0.36f, 0.60f),
            MakeMaterial(new Color(0.13f, 0.075f, 0.04f), 0.1f)
        );

        // Pirate hat
        MakePart(
            "Pirate Hat",
            PrimitiveType.Cylinder,
            modelTransform,
            new Vector3(0f, 2.56f, 0f),
            new Vector3(0.82f, 0.11f, 0.82f),
            woodMaterial
        );

        MakePart(
            "Hat Crown",
            PrimitiveType.Cylinder,
            modelTransform,
            new Vector3(0f, 2.72f, 0f),
            new Vector3(0.45f, 0.20f, 0.45f),
            woodMaterial
        );

        // Eyes
        Material eyeMaterial =
            MakeMaterial(Color.black, 0.1f);

        MakePart(
            "Left Eye",
            PrimitiveType.Sphere,
            modelTransform,
            new Vector3(-0.14f, 2.10f, 0.255f),
            new Vector3(0.055f, 0.07f, 0.035f),
            eyeMaterial
        );

        MakePart(
            "Right Eye",
            PrimitiveType.Sphere,
            modelTransform,
            new Vector3(0.14f, 2.10f, 0.255f),
            new Vector3(0.055f, 0.07f, 0.035f),
            eyeMaterial
        );

        // Arms
        CreateLimb(
            "Left Arm",
            new Vector3(-0.58f, 1.43f, 0f),
            new Vector3(0.22f, 0.68f, 0.22f),
            playerMaterial
        );

        CreateLimb(
            "Right Arm",
            new Vector3(0.58f, 1.43f, 0f),
            new Vector3(0.22f, 0.68f, 0.22f),
            playerMaterial
        );

        // Hands
        MakePart(
            "Left Hand",
            PrimitiveType.Sphere,
            modelTransform,
            new Vector3(-0.58f, 0.80f, 0f),
            Vector3.one * 0.22f,
            playerMaterial
        );

        MakePart(
            "Right Hand",
            PrimitiveType.Sphere,
            modelTransform,
            new Vector3(0.58f, 0.80f, 0f),
            Vector3.one * 0.22f,
            playerMaterial
        );

        // Legs
        CreateLimb(
            "Left Leg",
            new Vector3(-0.22f, 0.37f, 0f),
            new Vector3(0.29f, 0.72f, 0.34f),
            woodMaterial
        );

        CreateLimb(
            "Right Leg",
            new Vector3(0.22f, 0.37f, 0f),
            new Vector3(0.29f, 0.72f, 0.34f),
            woodMaterial
        );

        // Boots
        MakePart(
            "Left Boot",
            PrimitiveType.Cube,
            modelTransform,
            new Vector3(-0.22f, 0.08f, 0.12f),
            new Vector3(0.36f, 0.18f, 0.50f),
            woodMaterial
        );

        MakePart(
            "Right Boot",
            PrimitiveType.Cube,
            modelTransform,
            new Vector3(0.22f, 0.08f, 0.12f),
            new Vector3(0.36f, 0.18f, 0.50f),
            woodMaterial
        );

        // Belt
        MakePart(
            "Golden Belt",
            PrimitiveType.Cube,
            modelTransform,
            new Vector3(0f, 0.78f, 0f),
            new Vector3(0.90f, 0.12f, 0.48f),
            goldMaterial
        );

        // Sword
        MakePart(
            "Sword Handle",
            PrimitiveType.Cylinder,
            modelTransform,
            new Vector3(0.72f, 0.85f, -0.15f),
            new Vector3(0.07f, 0.20f, 0.07f),
            woodMaterial
        );

        MakePart(
            "Sword Blade",
            PrimitiveType.Cube,
            modelTransform,
            new Vector3(0.73f, 1.18f, -0.15f),
            new Vector3(0.08f, 0.55f, 0.08f),
            steelMaterial
        );
    }

    void CreateLimb(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material
    )
    {
        MakePart(
            name,
            PrimitiveType.Capsule,
            modelTransform,
            position,
            scale,
            material
        );
    }

    GameObject MakePart(
        string name,
        PrimitiveType type,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material material
    )
    {
        GameObject part = GameObject.CreatePrimitive(type);

        part.name = name;

        part.transform.SetParent(parent);

        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        part.GetComponent<Renderer>().material = material;

        Collider collider = part.GetComponent<Collider>();

        if (collider != null)
            Destroy(collider);

        return part;
    }

    // ============================================================
    // CHARACTER ANIMATION
    // ============================================================

    void AnimateCharacter()
    {
        float movement = new Vector3(
            lastMovement.x,
            0f,
            lastMovement.z
        ).magnitude;

        bool moving = movement > 0.1f;

        animationTime += Time.deltaTime *
            (moving ? 11f : 2f);

        Transform leftLeg =
            modelTransform.Find("Left Leg");

        Transform rightLeg =
            modelTransform.Find("Right Leg");

        Transform leftArm =
            modelTransform.Find("Left Arm");

        Transform rightArm =
            modelTransform.Find("Right Arm");

        if (leftLeg != null && rightLeg != null)
        {
            float swing = moving
                ? Mathf.Sin(animationTime) * 30f
                : 0f;

            leftLeg.localRotation =
                Quaternion.Euler(swing, 0f, 0f);

            rightLeg.localRotation =
                Quaternion.Euler(-swing, 0f, 0f);
        }

        if (leftArm != null && rightArm != null)
        {
            float swing = moving
                ? Mathf.Sin(animationTime) * 24f
                : 0f;

            leftArm.localRotation =
                Quaternion.Euler(-swing, 0f, 0f);

            rightArm.localRotation =
                Quaternion.Euler(swing, 0f, 0f);
        }

        modelTransform.localPosition =
            new Vector3(
                0f,
                moving
                    ? Mathf.Abs(Mathf.Sin(animationTime * 2f)) * 0.06f
                    : Mathf.Sin(animationTime) * 0.015f,
                0f
            );
    }

    // ============================================================
    // CAMERA
    // ============================================================

    void CreateCamera()
    {
        cameraObject = new GameObject("Third Person Camera");

        Camera camera =
            cameraObject.AddComponent<Camera>();

        camera.fieldOfView = 72f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1200f;

        cameraObject.AddComponent<AudioListener>();

        cameraTransform = cameraObject.transform;

        cameraYaw = 0f;
        cameraPitch = 18f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateCamera()
    {
        if (inventoryOpen || paused)
            return;

        cameraYaw += Input.GetAxis("Mouse X") * 3f;

        cameraPitch -= Input.GetAxis("Mouse Y") * 2f;

        cameraPitch = Mathf.Clamp(
            cameraPitch, -5f, 65f
        );

        Quaternion rotation = Quaternion.Euler(
            cameraPitch,
            cameraYaw,
            0f
        );

        Vector3 target =
            player.transform.position +
            Vector3.up * 1.7f;

        float distance = sailing ? 12f : 7f;

        Vector3 desired =
            target - rotation * Vector3.forward * distance;

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            desired,
            Time.deltaTime * 10f
        );

        cameraTransform.rotation = Quaternion.Slerp(
            cameraTransform.rotation,
            Quaternion.LookRotation(
                target - cameraTransform.position
            ),
            Time.deltaTime * 12f
        );
    }

    // ============================================================
    // PROCEDURAL WORLD
    // ============================================================

    void CreateWorld()
    {
        for (int i = 0; i < islandCount; i++)
        {
            float angle =
                i * Mathf.PI * 2f / islandCount;

            float distance =
                80f + (i % 3) * 45f;

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            CreateIsland(position, i);
        }

        islandsDiscovered = 1;
    }

    void CreateIsland(Vector3 position, int index)
    {
        GameObject island = new GameObject(
            "Island " + (index + 1)
        );

        island.transform.position = position;

        float radius = UnityEngine.Random.Range(17f, 25f);

        // Sand base
        GameObject sand = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        sand.name = "Island Sand";

        sand.transform.SetParent(island.transform);

        sand.transform.localPosition =
            new Vector3(0f, -2.5f, 0f);

        sand.transform.localScale =
            new Vector3(radius * 2f, 5f, radius * 2f);

        sand.GetComponent<Renderer>().material =
            sandMaterial;

        // Grass surface
        GameObject grass = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        grass.name = "Island Surface";

        grass.transform.SetParent(island.transform);

        grass.transform.localPosition =
            new Vector3(0f, -1.2f, 0f);

        grass.transform.localScale =
            new Vector3(radius * 1.75f, 3.8f, radius * 1.75f);

        grass.GetComponent<Renderer>().material =
            grassMaterial;

        string islandName = islandNamePool[
            index % islandNamePool.Length
        ];

        islandNames[island] = islandName;

        islands.Add(island);

        CreateIslandTrees(position, radius);
        CreateIslandVillage(position, radius);
        CreateIslandDecorations(position, radius);
    }

    // ============================================================
    // TREES
    // ============================================================

    void CreateIslandTrees(Vector3 center, float radius)
    {
        for (int i = 0; i < treesPerIsland; i++)
        {
            float angle = UnityEngine.Random.Range(
                0f, Mathf.PI * 2f
            );

            float distance = UnityEngine.Random.Range(
                radius * 0.25f, radius * 0.78f
            );

            Vector3 position = center + new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            CreateTree(position);
        }
    }

    void CreateTree(Vector3 position)
    {
        GameObject tree = new GameObject("Palm Tree");

        tree.transform.position = position;

        float height = UnityEngine.Random.Range(3f, 5f);

        GameObject trunk = GameObject.CreatePrimitive(
            PrimitiveType.Cylinder
        );

        trunk.transform.SetParent(tree.transform);

        trunk.transform.localPosition =
            new Vector3(0f, height / 2f, 0f);

        trunk.transform.localScale =
            new Vector3(0.32f, height / 2f, 0.32f);

        trunk.GetComponent<Renderer>().material =
            woodMaterial;

        GameObject leaves = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        leaves.transform.SetParent(tree.transform);

        leaves.transform.localPosition =
            new Vector3(0f, height + 0.4f, 0f);

        leaves.transform.localScale =
            new Vector3(3.2f, 1.8f, 3.2f);

        leaves.GetComponent<Renderer>().material =
            leafMaterial;

        Destroy(trunk.GetComponent<Collider>());
        Destroy(leaves.GetComponent<Collider>());

        trees.Add(tree);
    }

    // ============================================================
    // VILLAGES AND BUILDINGS
    // ============================================================

    void CreateIslandVillage(Vector3 center, float radius)
    {
        Vector3 villageCenter =
            center + new Vector3(0f, 0f, -radius * 0.4f);

        for (int i = 0; i < 4; i++)
        {
            Vector3 position =
                villageCenter + new Vector3(
                    (i - 1.5f) * 6f,
                    0f,
                    UnityEngine.Random.Range(-2f, 2f)
                );

            CreateHouse(position);
        }

        CreateDock(
            center + new Vector3(radius + 4f, 0f, 0f)
        );
    }

    void CreateHouse(Vector3 position)
    {
        GameObject house = new GameObject("Village House");

        GameObject body = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        body.transform.SetParent(house.transform);

        body.transform.position =
            position + Vector3.up * 2f;

        body.transform.localScale =
            new Vector3(5f, 4f, 5f);

        body.GetComponent<Renderer>().material =
            woodMaterial;

        GameObject roof = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        roof.transform.SetParent(house.transform);

        roof.transform.position =
            position + Vector3.up * 4.4f;

        roof.transform.localScale =
            new Vector3(6f, 1f, 6f);

        roof.GetComponent<Renderer>().material =
            MakeMaterial(new Color(0.35f, 0.12f, 0.09f), 0.1f);
    }

    void CreateDock(Vector3 position)
    {
        GameObject dock = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        dock.name = "Wooden Dock";

        dock.transform.position =
            position + new Vector3(5f, 0f, 0f);

        dock.transform.localScale =
            new Vector3(12f, 0.5f, 5f);

        dock.GetComponent<Renderer>().material =
            woodMaterial;
    }

    void CreateIslandDecorations(
        Vector3 center,
        float radius
    )
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject rock = GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

            rock.name = "Island Rock";

            rock.transform.position = center +
                new Vector3(
                    UnityEngine.Random.Range(-radius, radius),
                    0f,
                    UnityEngine.Random.Range(-radius, radius)
                );

            rock.transform.localScale =
                Vector3.one * UnityEngine.Random.Range(1f, 3f);

            rock.GetComponent<Renderer>().material =
                MakeMaterial(new Color(0.36f, 0.37f, 0.34f), 0.1f);
        }
    }

    // ============================================================
    // SHIP SYSTEM
    // ============================================================

    void CreateShip()
    {
        ship = new GameObject("Captain's Ship");

        ship.transform.position =
            new Vector3(9f, 0.6f, 12f);

        GameObject hull = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        hull.transform.SetParent(ship.transform);

        hull.transform.localPosition =
            new Vector3(0f, 0f, 0f);

        hull.transform.localScale =
            new Vector3(4f, 1f, 9f);

        hull.GetComponent<Renderer>().material =
            woodMaterial;

        // Mast
        GameObject mast = GameObject.CreatePrimitive(
            PrimitiveType.Cylinder
        );

        mast.transform.SetParent(ship.transform);

        mast.transform.localPosition =
            new Vector3(0f, 2.8f, 0f);

        mast.transform.localScale =
            new Vector3(0.15f, 2.8f, 0.15f);

        mast.GetComponent<Renderer>().material =
            woodMaterial;

        // Sail
        GameObject sail = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        sail.transform.SetParent(ship.transform);

        sail.transform.localPosition =
            new Vector3(0f, 4.3f, 0f);

        sail.transform.localScale =
            new Vector3(2.7f, 2.8f, 0.12f);

        sail.GetComponent<Renderer>().material =
            MakeMaterial(Color.white, 0.1f);

        // Ship flag
        GameObject flag = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        flag.transform.SetParent(ship.transform);

        flag.transform.localPosition =
            new Vector3(0f, 6.7f, 0f);

        flag.transform.localScale =
            new Vector3(1.5f, 0.8f, 0.08f);

        flag.GetComponent<Renderer>().material =
            MakeMaterial(new Color(0.12f, 0.07f, 0.09f), 0.1f);

        // Ship collision is not needed for this prototype.
        foreach (Collider c in ship.GetComponentsInChildren<Collider>())
            Destroy(c);
    }

    void UpdateShip()
    {
        float forward = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        ship.transform.Translate(
            Vector3.forward * forward *
            sailingSpeed * Time.deltaTime
        );

        ship.transform.Rotate(
            Vector3.up * turn *
            sailingTurnSpeed * Time.deltaTime
        );

        float wave = Mathf.Sin(Time.time * 1.7f) * 0.15f;

        ship.transform.position = new Vector3(
            ship.transform.position.x,
            0.6f + wave,
            ship.transform.position.z
        );

        player.transform.position =
            ship.transform.position +
            Vector3.up * 1.5f;

        if (Input.GetKeyDown(KeyCode.E))
        {
            sailing = false;

            player.transform.position =
                ship.transform.position +
                ship.transform.right * 5f +
                Vector3.up * 2f;

            Notify("You left your ship.");
        }
    }

    void BoardShip()
    {
        if (Vector3.Distance(
            player.transform.position,
            ship.transform.position
        ) > 9f)
            return;

        sailing = true;

        movementVelocity = Vector3.zero;

        Notify("SHIP CONTROL ACTIVATED");
    }

    // ============================================================
    // ENEMY GENERATION
    // ============================================================

    void GenerateIslandEnemies()
    {
        foreach (GameObject island in islands)
        {
            Vector3 center =
                island.transform.position;

            for (int i = 0; i < enemiesPerIsland; i++)
            {
                Vector3 position = center +
                    new Vector3(
                        UnityEngine.Random.Range(-16f, 16f),
                        2f,
                        UnityEngine.Random.Range(-16f, 16f)
                    );

                CreateEnemy(position);
            }
        }
    }

    void CreateEnemy(Vector3 position)
    {
        GameObject enemy = new GameObject("Enemy Pirate");

        enemy.transform.position = position;

        GameObject body = GameObject.CreatePrimitive(
            PrimitiveType.Capsule
        );

        body.name = "Pirate Body";

        body.transform.SetParent(enemy.transform);

        body.transform.localPosition =
            new Vector3(0f, 1f, 0f);

        body.transform.localScale =
            new Vector3(0.8f, 1f, 0.8f);

        body.GetComponent<Renderer>().material =
            enemyMaterial;

        GameObject head = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        head.transform.SetParent(enemy.transform);

        head.transform.localPosition =
            new Vector3(0f, 2.15f, 0f);

        head.transform.localScale =
            Vector3.one * 0.6f;

        head.GetComponent<Renderer>().material =
            MakeMaterial(new Color(0.72f, 0.49f, 0.34f), 0.1f);

        CapsuleCollider collider =
            enemy.AddComponent<CapsuleCollider>();

        collider.height = 2.6f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0f, 1.2f, 0f);

        enemy.AddComponent<Rigidbody>().isKinematic = true;

        enemies.Add(enemy);

        enemyHealth[enemy] = 100f;
        enemyCooldown[enemy] = 0f;
        enemyRewards[enemy] = UnityEngine.Random.Range(50, 120);
    }

    // ============================================================
    // ENEMY AI
    // ============================================================

    void UpdateEnemyAI()
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

            Vector3 difference =
                player.transform.position -
                enemy.transform.position;

            difference.y = 0f;

            float distance = difference.magnitude;

            if (distance < 22f && distance > 2.3f)
            {
                Vector3 direction = difference.normalized;

                enemy.transform.rotation =
                    Quaternion.Slerp(
                        enemy.transform.rotation,
                        Quaternion.LookRotation(direction),
                        Time.deltaTime * 5f
                    );

                enemy.transform.position =
                    Vector3.MoveTowards(
                        enemy.transform.position,
                        new Vector3(
                            player.transform.position.x,
                            enemy.transform.position.y,
                            player.transform.position.z
                        ),
                        2.5f * Time.deltaTime
                    );
            }

            if (distance <= 2.5f &&
                Time.time >= enemyCooldown[enemy])
            {
                enemyCooldown[enemy] =
                    Time.time + 1.5f;

                health -= 8f;

                Notify("A pirate attacked you!");

                if (health <= 0f)
                    Respawn();
            }
        }
    }

    // ============================================================
    // SWORD COMBAT
    // ============================================================

    void SwordAttack()
    {
        if (sailing || Time.time < attackTimer)
            return;

        attackTimer = Time.time + swordCooldown;

        attacking = true;

        GameObject target = FindEnemyTarget(swordRange, 75f);

        if (target != null)
        {
            DamageEnemy(target, swordDamage);

            Notify("SWORD STRIKE");
        }
        else
        {
            Notify("Attack missed.");
        }
    }

    GameObject FindEnemyTarget(
        float range,
        float angleLimit
    )
    {
        GameObject target = null;

        float closestDistance = range;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 difference =
                enemy.transform.position -
                player.transform.position;

            float distance = difference.magnitude;

            float angle = Vector3.Angle(
                player.transform.forward,
                difference
            );

            if (distance < closestDistance &&
                angle < angleLimit)
            {
                closestDistance = distance;
                target = enemy;
            }
        }

        return target;
    }

    void DamageEnemy(GameObject enemy, float damage)
    {
        if (enemy == null ||
            !enemyHealth.ContainsKey(enemy))
            return;

        enemyHealth[enemy] -= damage;

        Renderer renderer =
            enemy.GetComponentInChildren<Renderer>();

        if (renderer != null)
            renderer.material.color = Color.yellow;

        if (enemyHealth[enemy] <= 0f)
            DefeatEnemy(enemy);
    }

    void DefeatEnemy(GameObject enemy)
    {
        int reward = enemyRewards[enemy];

        berries += reward;
        bounty += 100;

        enemiesDefeated++;

        GainExperience(60);

        enemyHealth.Remove(enemy);
        enemyCooldown.Remove(enemy);
        enemyRewards.Remove(enemy);

        enemies.Remove(enemy);

        Destroy(enemy);

        Notify("PIRATE DEFEATED  +" + reward + " BERRIES");

        CheckQuest();
    }

    // ============================================================
    // SPECIAL ABILITY
    // ============================================================

    void UseAbility()
    {
        if (sailing || Time.time < abilityTimer)
            return;

        if (energy < abilityCost)
        {
            Notify("Not enough energy!");
            return;
        }

        energy -= abilityCost;

        abilityTimer = Time.time + 6f;

        List<GameObject> targets =
            new List<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(
                player.transform.position,
                enemy.transform.position
            );

            if (distance <= abilityRange)
                targets.Add(enemy);
        }

        foreach (GameObject enemy in targets)
        {
            DamageEnemy(enemy, abilityDamage);
        }

        CreateStormEffect();

        Notify("STORM BURST ACTIVATED");
    }

    void CreateStormEffect()
    {
        GameObject effect = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        effect.name = "Storm Ability Effect";

        effect.transform.position =
            player.transform.position +
            Vector3.up * 1f;

        effect.transform.localScale =
            Vector3.one * abilityRange * 2f;

        effect.GetComponent<Renderer>().material =
            MakeMaterial(
                new Color(0.1f, 0.55f, 1f, 0.35f),
                0.9f
            );

        Destroy(effect.GetComponent<Collider>());

        Destroy(effect, 0.35f);
    }

    // ============================================================
    // TREASURE SYSTEM
    // ============================================================

    void GenerateTreasures()
    {
        foreach (GameObject island in islands)
        {
            Vector3 position =
                island.transform.position +
                new Vector3(4f, 5f, 12f);

            GameObject chest = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            chest.name = "Treasure Chest";

            chest.transform.position = position;

            chest.transform.localScale =
                new Vector3(1.8f, 1.2f, 1.3f);

            chest.GetComponent<Renderer>().material =
                woodMaterial;

            treasures.Add(chest);
        }
    }

    void LootTreasure()
    {
        for (int i = treasures.Count - 1; i >= 0; i--)
        {
            GameObject chest = treasures[i];

            if (chest == null) continue;

            float distance = Vector3.Distance(
                player.transform.position,
                chest.transform.position
            );

            if (distance > 5f)
                continue;

            int reward = UnityEngine.Random.Range(100, 500);

            string item = treasureItems[
                UnityEngine.Random.Range(
                    0, treasureItems.Length
                )
            ];

            berries += reward;

            treasureCount++;

            inventory.Add(item);

            treasures.RemoveAt(i);

            Destroy(chest);

            GainExperience(80);

            Notify("TREASURE FOUND: " + item);

            return;
        }
    }

    // ============================================================
    // ISLAND DISCOVERY
    // ============================================================

    void DiscoverIsland()
    {
        foreach (GameObject island in islands)
        {
            if (island == null) continue;

            float distance = Vector3.Distance(
                player.transform.position,
                island.transform.position
            );

            if (distance > 25f)
                continue;

            string name = islandNames[island];

            if (currentIsland == name)
                return;

            currentIsland = name;

            islandsDiscovered++;

            GainExperience(50);

            Notify("DISCOVERED: " + name);

            return;
        }
    }

    // ============================================================
    // QUEST SYSTEM
    // ============================================================

    void CheckQuest()
    {
        if (enemiesDefeated >= 3 &&
            !inventory.Contains("First Voyage Complete"))
        {
            inventory.Add("First Voyage Complete");

            berries += 500;
            bounty += 500;

            GainExperience(200);

            Notify("FIRST VOYAGE QUEST COMPLETE");
        }
    }

    // ============================================================
    // LEVELING SYSTEM
    // ============================================================

    void GainExperience(int amount)
    {
        experience += amount;

        while (experience >= level * 100)
        {
            experience -= level * 100;

            level++;

            skillPoints++;

            maxHealth += 15f;
            maxStamina += 5f;

            swordDamage += 3f;

            health = maxHealth;

            Notify("LEVEL UP! LEVEL " + level);
        }
    }

    void UpgradeHealth()
    {
        if (skillPoints <= 0)
            return;

        skillPoints--;

        maxHealth += 25f;
        health = maxHealth;

        Notify("MAX HEALTH UPGRADED");
    }

    void UpgradeDamage()
    {
        if (skillPoints <= 0)
            return;

        skillPoints--;

        swordDamage += 10f;

        Notify("SWORD DAMAGE UPGRADED");
    }

    // ============================================================
    // PLAYER INTERACTION
    // ============================================================

    void Interact()
    {
        if (sailing)
            return;

        float shipDistance = Vector3.Distance(
            player.transform.position,
            ship.transform.position
        );

        if (shipDistance <= 9f)
        {
            BoardShip();
            return;
        }

        LootTreasure();
    }

    // ============================================================
    // INVENTORY
    // ============================================================

    void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;

        Cursor.visible = inventoryOpen || paused;

        Cursor.lockState =
            inventoryOpen || paused
                ? CursorLockMode.None
                : CursorLockMode.Locked;
    }

    // ============================================================
    // RESPAWN
    // ============================================================

    void Respawn()
    {
        health = maxHealth;
        stamina = maxStamina;
        energy = maxEnergy;

        sailing = false;

        player.transform.position = spawnPosition;

        movementVelocity = Vector3.zero;

        Notify("RETURNED TO YOUR HOME ISLAND");
    }

    // ============================================================
    // PLAYER MOVEMENT
    // ============================================================

    void UpdateMovement()
    {
        if (sailing)
        {
            UpdateShip();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward =
            Quaternion.Euler(0f, cameraYaw, 0f) *
            Vector3.forward;

        Vector3 right =
            Quaternion.Euler(0f, cameraYaw, 0f) *
            Vector3.right;

        Vector3 direction =
            forward * vertical + right * horizontal;

        direction = Vector3.ClampMagnitude(direction, 1f);

        bool sprinting =
            Input.GetKey(KeyCode.LeftShift) &&
            stamina > 0f &&
            direction.magnitude > 0.1f;

        float speed = sprinting
            ? runSpeed
            : walkSpeed;

        if (sprinting)
            stamina -= 25f * Time.deltaTime;
        else
            stamina += 15f * Time.deltaTime;

        stamina = Mathf.Clamp(
            stamina, 0f, maxStamina
        );

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            player.transform.rotation =
                Quaternion.Slerp(
                    player.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }

        bool grounded = characterController.isGrounded;

        if (grounded && movementVelocity.y < 0f)
            movementVelocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            movementVelocity.y = jumpPower;
            jumping = true;
        }

        movementVelocity.y -= gravity * Time.deltaTime;

        Vector3 horizontalMovement =
            direction * speed;

        lastMovement = horizontalMovement;

        Vector3 finalMovement =
            horizontalMovement +
            Vector3.up * movementVelocity.y;

        characterController.Move(
            finalMovement * Time.deltaTime
        );

        if (characterController.isGrounded)
            jumping = false;

        if (player.transform.position.y < -25f)
            Respawn();
    }

    // ============================================================
    // PAUSE SYSTEM
    // ============================================================

    void TogglePause()
    {
        paused = !paused;

        Cursor.visible = paused;
        Cursor.lockState = paused
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    // ============================================================
    // NOTIFICATIONS
    // ============================================================

    void Notify(string message)
    {
        notification = message;
        notificationTimer = 3.5f;
    }

    // ============================================================
    // GAME LOOP
    // ============================================================

    void Update()
    {
        if (!initialized)
            return;

        if (notificationTimer > 0f)
            notificationTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (paused)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();

        if (inventoryOpen)
            return;

        if (Input.GetMouseButtonDown(0))
            SwordAttack();

        if (Input.GetKeyDown(KeyCode.Q))
            UseAbility();

        if (Input.GetKeyDown(KeyCode.E))
            Interact();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            UpgradeHealth();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UpgradeDamage();

        UpdateMovement();
        UpdateCamera();
        UpdateEnemyAI();
        AnimateCharacter();
        DiscoverIsland();

        energy = Mathf.Min(
            maxEnergy,
            energy + 5f * Time.deltaTime
        );

        if (health <= 0f)
            Respawn();
    }

    // ============================================================
    // USER INTERFACE
    // ============================================================

    void CreateInterface()
    {
        titleStyle = new GUIStyle(GUI.skin.label);

        titleStyle.fontSize = 23;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        headingStyle = new GUIStyle(GUI.skin.label);

        headingStyle.fontSize = 17;
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.normal.textColor = Color.white;

        normalStyle = new GUIStyle(GUI.skin.label);

        normalStyle.fontSize = 14;
        normalStyle.normal.textColor = Color.white;

        smallStyle = new GUIStyle(GUI.skin.label);

        smallStyle.fontSize = 12;
        smallStyle.normal.textColor = Color.white;

        centerStyle = new GUIStyle(normalStyle);

        centerStyle.alignment = TextAnchor.MiddleCenter;

        buttonStyle = new GUIStyle(GUI.skin.button);

        buttonStyle.fontSize = 15;
    }

    void DrawPanel(
        float x,
        float y,
        float width,
        float height
    )
    {
        GUI.color = new Color(0.035f, 0.055f, 0.075f, 0.88f);

        GUI.Box(
            new Rect(x, y, width, height),
            ""
        );

        GUI.color = Color.white;
    }

    void DrawBar(
        string label,
        float value,
        float maximum,
        float x,
        float y,
        Color color
    )
    {
        GUI.color = new Color(0f, 0f, 0f, 0.75f);

        GUI.Box(
            new Rect(x, y, 220f, 20f),
            ""
        );

        GUI.color = color;

        GUI.DrawTexture(
            new Rect(
                x + 3f,
                y + 3f,
                214f * Mathf.Clamp01(value / maximum),
                14f
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        GUI.Label(
            new Rect(x + 6f, y, 210f, 20f),
            label + "  " +
            Mathf.CeilToInt(value) + "/" +
            Mathf.CeilToInt(maximum),
            smallStyle
        );
    }

    // ============================================================
    // HUD
    // ============================================================

    void DrawHUD()
    {
        DrawPanel(15f, 15f, 245f, 190f);

        GUI.Label(
            new Rect(27f, 22f, 220f, 30f),
            "SAILOR OF THE SEA",
            titleStyle
        );

        DrawBar(
            "HEALTH", health, maxHealth,
            27f, 62f, Color.red
        );

        DrawBar(
            "STAMINA", stamina, maxStamina,
            27f, 89f, Color.green
        );

        DrawBar(
            "ENERGY", energy, maxEnergy,
            27f, 116f, Color.cyan
        );

        GUI.Label(
            new Rect(27f, 148f, 220f, 25f),
            "LEVEL " + level + "  |  XP " + experience,
            normalStyle
        );

        GUI.Label(
            new Rect(27f, 174f, 220f, 25f),
            "SKILL POINTS: " + skillPoints,
            smallStyle
        );

        // Economy
        DrawPanel(15f, 215f, 245f, 105f);

        GUI.Label(
            new Rect(27f, 224f, 220f, 25f),
            "BERRIES: " + berries,
            normalStyle
        );

        GUI.Label(
            new Rect(27f, 252f, 220f, 25f),
            "BOUNTY: " + bounty,
            normalStyle
        );

        GUI.Label(
            new Rect(27f, 280f, 220f, 25f),
            "PIRATES DEFEATED: " + enemiesDefeated,
            smallStyle
        );

        // Island location
        DrawPanel(
            Screen.width / 2f - 130f,
            15f,
            260f,
            48f
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 120f,
                24f,
                240f,
                30f
            ),
            currentIsland.ToUpper(),
            centerStyle
        );

        // Quest
        DrawPanel(
            Screen.width - 270f,
            15f,
            255f,
            125f
        );

        GUI.Label(
            new Rect(
                Screen.width - 255f,
                25f,
                225f,
                25f
            ),
            "ACTIVE QUEST",
            headingStyle
        );

        GUI.Label(
            new Rect(
                Screen.width - 255f,
                60f,
                225f,
                65f
            ),
            "Defeat 3 rogue pirates\n" +
            "Progress: " +
            Mathf.Min(enemiesDefeated, 3) + "/3",
            normalStyle
        );

        // Controls
        DrawPanel(
            Screen.width - 225f,
            Screen.height - 190f,
            210f,
            175f
        );

        GUI.Label(
            new Rect(
                Screen.width - 212f,
                Screen.height - 180f,
                195f,
                160f
            ),
            "CONTROLS\n\n" +
            "WASD   Move\n" +
            "SHIFT   Sprint\n" +
            "SPACE   Jump\n" +
            "MOUSE   Sword\n" +
            "Q   Special Ability\n" +
            "E   Interact\n" +
            "TAB   Inventory\n" +
            "ESC   Pause",
            smallStyle
        );

        // Notification
        if (notificationTimer > 0f)
        {
            DrawPanel(
                Screen.width / 2f - 220f,
                75f,
                440f,
                48f
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 210f,
                    84f,
                    420f,
                    30f
                ),
                notification,
                centerStyle
            );
        }

        // Crosshair
        GUI.Label(
            new Rect(
                Screen.width / 2f - 8f,
                Screen.height / 2f - 12f,
                20f,
                25f
            ),
            "+",
            titleStyle
        );
    }

    // ============================================================
    // INVENTORY UI
    // ============================================================

    void DrawInventory()
    {
        DrawPanel(
            Screen.width / 2f - 240f,
            Screen.height / 2f - 220f,
            480f,
            440f
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 210f,
                Screen.height / 2f - 200f,
                420f,
                40f
            ),
            "CAPTAIN'S INVENTORY",
            titleStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 210f,
                Screen.height / 2f - 150f,
                420f,
                25f
            ),
            "DEVIL FRUIT: " + fruitName,
            normalStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 210f,
                Screen.height / 2f - 115f,
                420f,
                25f
            ),
            "BERRIES: " + berries,
            normalStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 210f,
                Screen.height / 2f - 80f,
                420f,
                25f
            ),
            "TREASURES: " + treasureCount,
            normalStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 210f,
                Screen.height / 2f - 45f,
                420f,
                25f
            ),
            "DISCOVERED ISLANDS: " + islandsDiscovered,
            normalStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 210f,
                Screen.height / 2f,
                420f,
                160f
            ),
            "ITEMS\n" + string.Join("\n", inventory),
            normalStyle
        );

        if (GUI.Button(
            new Rect(
                Screen.width / 2f - 85f,
                Screen.height / 2f + 170f,
                170f,
                35f
            ),
            "CLOSE INVENTORY",
            buttonStyle
        ))
        {
            ToggleInventory();
        }
    }

    // ============================================================
    // PAUSE MENU
    // ============================================================

    void DrawPauseMenu()
    {
        DrawPanel(
            Screen.width / 2f - 200f,
            Screen.height / 2f - 150f,
            400f,
            300f
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 170f,
                Screen.height / 2f - 125f,
                340f,
                45f
            ),
            "GAME PAUSED",
            titleStyle
        );

        if (GUI.Button(
            new Rect(
                Screen.width / 2f - 100f,
                Screen.height / 2f - 45f,
                200f,
                40f
            ),
            "RESUME",
            buttonStyle
        ))
        {
            TogglePause();
        }

        if (GUI.Button(
            new Rect(
                Screen.width / 2f - 100f,
                Screen.height / 2f + 15f,
                200f,
                40f
            ),
            "RESPAWN",
            buttonStyle
        ))
        {
            Respawn();
            TogglePause();
        }

        GUI.Label(
            new Rect(
                Screen.width / 2f - 170f,
                Screen.height / 2f + 90f,
                340f,
                30f
            ),
            "ESC TO RETURN TO THE GAME",
            centerStyle
        );
    }

    // ============================================================
    // FINAL UI DRAW
    // ============================================================

    void OnGUI()
    {
        if (!initialized)
            return;

        GUI.color = Color.white;

        if (paused)
        {
            DrawPauseMenu();
            return;
        }

        if (inventoryOpen)
        {
            DrawInventory();
            return;
        }

        DrawHUD();
    }
}
