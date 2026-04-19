using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab; 
    public Transform player;
    public float spawnInterval = 12f; 
    
    // Distância para as moedas
    public float minSpawnRadius = 5f; 
    public float maxSpawnRadius = 15f;
    
    private float timer = 0f;

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
                Debug.LogError("CoinSpawner: PlayerReference não encontrada! Tente adicionar PlayerReference ao Player GameObject.");
            }
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

        Vector3 spawnPosition = MapConfig.GetRandomSpawnPosition(player, minSpawnRadius, maxSpawnRadius);
        Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
    }
}