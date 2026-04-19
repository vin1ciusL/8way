using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;

    public int initialEnemyCount = 20;
    public float spawnInterval = 1.2f;
    public int maxEnemies = 150;

    public float minSpawnRadius = 12f;
    public float maxSpawnRadius = 22f;

    private float lastSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private int lastDifficultyMinute = 0;

    // Rotaciona o quadrante de spawn pra encerclar o player
    private int spawnQuadrant = 0;
    private int spawnsSinceQuadrantChange = 0;
    private const int SPAWNS_PER_QUADRANT = 3;

    void Start()
    {
        if (player == null)
        {
            if (PlayerReference.instance != null)
                player = PlayerReference.instance;
            else
                Debug.LogError("EnemySpawner: PlayerReference não encontrada!");
        }

        for (int i = 0; i < initialEnemyCount; i++)
            SpawnEnemy();
    }

    void Update()
    {
        if (GameController.gameOver || player == null) return;

        // Dificuldade progressiva
        int minute = Mathf.FloorToInt(GameController.GameTime / 60f);
        if (minute > lastDifficultyMinute)
        {
            spawnInterval = Mathf.Max(spawnInterval / 1.5f, 0.2f);
            lastDifficultyMinute = minute;
        }

        float intervaloReal = GameController.isCapturingZone
            ? spawnInterval / 1.33f
            : spawnInterval;

        if (currentEnemyCount < maxEnemies && Time.time - lastSpawnTime >= intervaloReal)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 pos = GetDirectionalSpawnPosition();
        var newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        currentEnemyCount++;

        var listener = newEnemy.AddComponent<EnemyDestroyListener>();
        listener.spawner = this;

        // Avança o quadrante a cada N spawns
        spawnsSinceQuadrantChange++;
        if (spawnsSinceQuadrantChange >= SPAWNS_PER_QUADRANT)
        {
            spawnQuadrant = (spawnQuadrant + 1) % 4;
            spawnsSinceQuadrantChange = 0;
        }
    }

    // Spawn rotativo por quadrante (N/S/L/O) com variação de ±35°
    // Isso faz os zumbis chegarem de direções diferentes, encercando o player
    Vector3 GetDirectionalSpawnPosition()
    {
        float baseAngle = spawnQuadrant * 90f + Random.Range(-35f, 35f);
        float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
        float rad = baseAngle * Mathf.Deg2Rad;

        Vector3 pos = player.position + new Vector3(
            Mathf.Cos(rad) * radius,
            Mathf.Sin(rad) * radius,
            0f
        );

        return MapConfig.ClampPosition(pos);
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }
}

public class EnemyDestroyListener : MonoBehaviour
{
    public EnemySpawner spawner;

    void OnDestroy()
    {
        if (spawner != null)
            spawner.OnEnemyDestroyed();
    }
}