using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public Transform player; // NOVO: Referência ao jogador
    
    public int initialEnemyCount = 10;
    public float spawnInterval = 1.5f;
    public int maxEnemies = 50;

    // NOVO: Distância mínima e máxima do player onde os inimigos vão nascer
    public float minSpawnRadius = 10f; 
    public float maxSpawnRadius = 20f;

    // NOVO: Limites da sua arena imensa (ajuste esses valores no Inspector)
    public float mapMinX = -100f;
    public float mapMaxX = 100f;
    public float mapMinY = -100f;
    public float mapMaxY = 100f;
    
    private float lastSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private int lastDifficultyMinute = 0;

    void Start()
    {
        // Tenta achar o player automaticamente caso você esqueça de arrastar no Inspector
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
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
        int currentMinute = Mathf.FloorToInt(GameController.gameTime / 60f);
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

    // Função matemática que sorteia um ponto em volta do player e prende dentro das paredes
    Vector3 GetValidSpawnPosition()
    {
        // 1. Escolhe uma direção aleatória em 360 graus
        float randomAngle = Random.Range(0f, Mathf.PI * 2);
        
        // 2. Escolhe uma distância aleatória entre o raio mínimo e máximo
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);

        // 3. Calcula o X e Y baseado no player
        float spawnX = player.position.x + (Mathf.Cos(randomAngle) * randomRadius);
        float spawnY = player.position.y + (Mathf.Sin(randomAngle) * randomRadius);

        // 4. PRENDE nas paredes da arena (Impede que o X e Y passem dos limites que você configurou)
        spawnX = Mathf.Clamp(spawnX, mapMinX, mapMaxX);
        spawnY = Mathf.Clamp(spawnY, mapMinY, mapMaxY);

        return new Vector3(spawnX, spawnY, 0f);
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