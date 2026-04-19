using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public Transform player;
    
    public int initialEnemyCount = 10;
    public float spawnInterval = 1.5f;
    public int maxEnemies = 50;

    // Distância mínima e máxima do player onde os inimigos vão nascer
    public float minSpawnRadius = 10f; 
    public float maxSpawnRadius = 20f;
    
    private float lastSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private int lastDifficultyMinute = 0;

    void Start()
    {
        // Tenta usar PlayerReference singleton primeiro
        if (player == null)
        {
            if (PlayerReference.instance != null)
            {
                player = PlayerReference.instance;
            }
            else
            {
                Debug.LogError("EnemySpawner: PlayerReference não encontrada! Tente adicionar PlayerReference ao Player GameObject.");
            }
        }

        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (GameController.gameOver || player == null) return;

        // DIFICULDADE
        int currentMinute = Mathf.FloorToInt(GameController.GameTime / 60f);
        if (currentMinute > lastDifficultyMinute)
        {
            spawnInterval /= 1.5f; 
            spawnInterval = Mathf.Max(spawnInterval, 0.2f);
            lastDifficultyMinute = currentMinute;
        }

        // SPAWN
        if (currentEnemyCount < maxEnemies && Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;
        
        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        currentEnemyCount++;
        
        var destroyer = newEnemy.AddComponent<EnemyDestroyListener>();
        destroyer.spawner = this;
    }

    // Obtém uma posição de spawn válida usando MapConfig
    Vector3 GetValidSpawnPosition()
    {
        return MapConfig.GetRandomSpawnPosition(player, minSpawnRadius, maxSpawnRadius);
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