using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; 
    public Transform player;

    public int initialEnemyCount = 20;
    public float spawnInterval = 1.2f;
    public int maxEnemies = 150;

    public float minSpawnRadius = 12f;
    public float maxSpawnRadius = 22f;

    private float lastSpawnTime = 0f;
    private int currentEnemyCount = 0;
    private int lastDifficultyMinute = 0;

    // Controle de quadrantes para o cerco
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

        // Dificuldade progressiva pelo tempo
        int minute = Mathf.FloorToInt(GameController.GameTime / 60f);
        if (minute > lastDifficultyMinute)
        {
            spawnInterval = Mathf.Max(spawnInterval / 1.5f, 0.2f);
            lastDifficultyMinute = minute;
        }

        float intervaloReal = GameController.isCapturingZone ? spawnInterval / 1.33f : spawnInterval;

        if (currentEnemyCount < maxEnemies && Time.time - lastSpawnTime >= intervaloReal)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        Vector3 pos = GetDirectionalSpawnPosition();
        float roll = Random.value * 100f; // Sorteio de 0 a 100

        int indexEscolhido = 2; // Padrão: Small
        EnemyController.EnemyType tipoParaAplicar = EnemyController.EnemyType.Small;

        int zones = GameController.zonesCompleted;

        // --- TABELA DE PORCENTAGENS POR ZONA ---
        if (zones == 0)
        {
            if      (roll < 5f)  { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.GhostAxe; }
            else if (roll < 10f) { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Giant; }
            else if (roll < 20f) { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Flanker; }
            else if (roll < 35f) { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Big; }
            else if (roll < 50f) { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.Axe; }
            else                 { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Small; }
        }
        else if (zones == 1)
        {
            if      (roll < 5f)  { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.GhostAxe; }
            else if (roll < 10f) { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Giant; }
            else if (roll < 25f) { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Flanker; }
            else if (roll < 40f) { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Big; }
            else if (roll < 55f) { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.Axe; }
            else                 { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Small; }
        }
        else if (zones == 2)
        {
            if      (roll < 7.5f) { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.GhostAxe; }
            else if (roll < 15f)  { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Giant; }
            else if (roll < 30f)  { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Flanker; }
            else if (roll < 40f)  { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Big; }
            else if (roll < 55f)  { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.Axe; }
            else                  { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Small; }
        }
        else // 3 ou 4 Zonas concluídas
        {
            if      (roll < 10f)   { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.GhostAxe; }
            else if (roll < 17.5f) { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Giant; }
            else if (roll < 35f)   { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Flanker; }
            else if (roll < 45f)   { indexEscolhido = 0; tipoParaAplicar = EnemyController.EnemyType.Big; }
            else if (roll < 60f)   { indexEscolhido = 1; tipoParaAplicar = EnemyController.EnemyType.Axe; }
            else                   { indexEscolhido = 2; tipoParaAplicar = EnemyController.EnemyType.Small; }
        }

        // Instanciação
        GameObject prefabEscolhido = enemyPrefabs[indexEscolhido];
        var newEnemy = Instantiate(prefabEscolhido, pos, Quaternion.identity);
        
        // Aplica o tipo
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        if (controller != null) controller.type = tipoParaAplicar;

        // Visual do Flanker (Avermelhado)
        if (tipoParaAplicar == EnemyController.EnemyType.Flanker)
        {
            SpriteRenderer sr = newEnemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 0.7f, 0.7f);
        }

        currentEnemyCount++;

        // Listener de destruição
        var listener = newEnemy.AddComponent<EnemyDestroyListener>();
        listener.spawner = this;

        // Rotação de quadrantes
        spawnsSinceQuadrantChange++;
        if (spawnsSinceQuadrantChange >= SPAWNS_PER_QUADRANT)
        {
            spawnQuadrant = (spawnQuadrant + 1) % 4;
            spawnsSinceQuadrantChange = 0;
        }
    }

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
        if (spawner != null) spawner.OnEnemyDestroyed();
    }
}