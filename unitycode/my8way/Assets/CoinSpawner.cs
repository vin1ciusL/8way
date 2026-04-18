using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab; 
    public Transform player;
    public float spawnInterval = 5f; 
    
    // Distância para as moedas
    public float minSpawnRadius = 5f; 
    public float maxSpawnRadius = 15f;

    // Limites da arena (Devem ser iguais aos do EnemySpawner)
    public float mapMinX = -100f;
    public float mapMaxX = 100f;
    public float mapMinY = -100f;
    public float mapMaxY = 100f;
    
    private float timer = 0f;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (GameController.gameOver || player == null) return;

        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnCoin();
            timer = 0f; 
        }
    }

    void SpawnCoin()
    {
        if (coinPrefab == null) return;

        float randomAngle = Random.Range(0f, Mathf.PI * 2);
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);

        float spawnX = player.position.x + (Mathf.Cos(randomAngle) * randomRadius);
        float spawnY = player.position.y + (Mathf.Sin(randomAngle) * randomRadius);

        // Prende a moeda dentro das paredes
        spawnX = Mathf.Clamp(spawnX, mapMinX, mapMaxX);
        spawnY = Mathf.Clamp(spawnY, mapMinY, mapMaxY);
        
        Instantiate(coinPrefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
    }
}