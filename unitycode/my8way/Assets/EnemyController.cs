using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Small, Big, Axe, Flanker, Giant, GhostAxe }
    public EnemyType type = EnemyType.Small;

    public float maxHealth = 20f;
    private float currentHealth;
    public float moveSpeed = 1f;
    public int damageToPlayer = 2;

    private Transform playerTransform;
    private Rigidbody2D rb;

    private float updatePathInterval = 0.1f; 
    private float lastPathUpdateTime = 0f;
    private Vector2 moveDirection = Vector2.zero;

    private float lastChainsawHit = 0f;
    private float chainsawCooldown = 0.1f;
    private float knockbackEndTime = 0f;

    private const float SEPARATION_RADIUS = 1.2f; 
    private Vector2 separationForce = Vector2.zero;

    // Constantes para perseguição de longa distância
    private const float MAX_DISTANCE_LONG_RANGE = 25f; // Diminuí a distância para o buff ativar mais cedo
    private const float CATCH_UP_MULTIPLIER = 3.5f;    // Aumentado (era 2.2f)

    public Animator animator;
    private string currentState;
    private SpriteRenderer spriteRenderer;

    // --- VARIÁVEIS DE ÁUDIO ---
    public AudioClip[] zombieSounds; // Array para guardar os 4 sons
    private AudioSource audioSource;

    public AudioClip chainsawHitSound;
    private float soundInterval = 2f;
    private float lastSoundCheckTime = 0f;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "Inimigo";

        if (PlayerReference.instance != null)
            playerTransform = PlayerReference.instance;
        else
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        ApplyStatsByType();
        currentHealth = maxHealth;
        ApplyZoneBuffs();

        lastPathUpdateTime = Random.Range(0f, updatePathInterval);

        audioSource = GetComponent<AudioSource>();
        
        // Sorteia o tempo inicial para eles não gemerem todos juntos no mesmo frame sincronizado
        lastSoundCheckTime = Time.time + Random.Range(0f, 2f);
    }

    void ApplyStatsByType()
    {
        transform.localScale = Vector3.one;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        switch (type)
        {
            case EnemyType.Small: 
                maxHealth = 15f; 
                moveSpeed = Random.Range(2.4f, 2.8f); // Aumentado (era 1.8-2.2)
                damageToPlayer = 1; 
                break;

            case EnemyType.Big: 
                maxHealth = 60f; 
                moveSpeed = Random.Range(1.0f, 1.3f); // Aumentado (era 0.6-0.8)
                damageToPlayer = 3; 
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                break;

            case EnemyType.Axe: 
                maxHealth = 25f; 
                moveSpeed = Random.Range(1.6f, 1.9f); // Aumentado (era 1.2-1.4)
                damageToPlayer = 5; 
                break;

            case EnemyType.Flanker: 
                maxHealth = 12f; 
                moveSpeed = Random.Range(3.8f, 4.3f); // Aumentado (era 3.0-3.5)
                damageToPlayer = 2;
                break;

            case EnemyType.Giant: 
                maxHealth = 240f; 
                moveSpeed = 0.8f; // Aumentado (era 0.5)
                damageToPlayer = 15; 
                transform.localScale = new Vector3(3.0f, 3.0f, 1f);
                if (spriteRenderer != null)
                    spriteRenderer.color = new Color(0.6f, 0.6f, 0.6f);
                break;

            case EnemyType.GhostAxe: 
                maxHealth = 30f;
                moveSpeed = 1.7f; // Aumentado (era 1.2)
                damageToPlayer = 6; 
                if (spriteRenderer != null)
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                gameObject.layer = LayerMask.NameToLayer("Fantasma");
                break;
        }
    }

    void ApplyZoneBuffs()
    {
        float zoneBuffMultiplier = 1f + (GameController.zonesCompleted * 0.2f);
        if (GameController.isCapturingZone)
            zoneBuffMultiplier *= 1.15f; 
        
        maxHealth *= zoneBuffMultiplier;
        damageToPlayer = Mathf.CeilToInt(damageToPlayer * zoneBuffMultiplier);
    }

    void Update()
    {
        if (playerTransform == null || GameController.gameOver)
        {
            if(rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Time.time < knockbackEndTime) return;

        if (Time.time - lastPathUpdateTime >= updatePathInterval)
        {
            CalculateMovement();
            lastPathUpdateTime = Time.time;
        }

        // --- LÓGICA DE VELOCIDADE ATUALIZADA ---
        float speedMultiplier = 1f;
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Se o zumbi estiver muito longe, ele corre MUITO mais para não sumir da tela
        if (distToPlayer > MAX_DISTANCE_LONG_RANGE) 
        {
            speedMultiplier = CATCH_UP_MULTIPLIER;
        }

        float finalSpeed = GameController.isCapturingZone ? moveSpeed * 1.33f : moveSpeed;
        rb.linearVelocity = moveDirection * finalSpeed * speedMultiplier;

        // --- RELÓGIO DOS SONS AMBIENTES ---
        if (Time.time - lastSoundCheckTime >= soundInterval)
        {
            TryPlayZombieSound();
            lastSoundCheckTime = Time.time;
        }

        UpdateAnimation();
    }

void CalculateMovement()
    {
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;

        // Movimento Base
        if (type == EnemyType.Flanker && dist > 2f)
        {
            Vector2 orbitDir = new Vector2(-toPlayer.y, toPlayer.x).normalized;
            moveDirection = (toPlayer.normalized + orbitDir * 1.5f).normalized;
        }
        else
        {
            moveDirection = toPlayer.normalized;
        }

        CalculateSeparation();
        
        // --- NOVA LÓGICA: ANTENAS DE DESVIO ---
        Vector2 avoidanceForce = CalculateObstacleAvoidance();

        // Mistura as três forças: Ir pro player + Separar dos amigos + Desviar da parede
        moveDirection = (moveDirection + separationForce * 0.4f + avoidanceForce).normalized;
    }

    Vector2 CalculateObstacleAvoidance()
    {
        Vector2 force = Vector2.zero;
        
        // O Fantasma não precisa desviar, ele atravessa os outros!
        if (type == EnemyType.GhostAxe) return force;

        float lookAhead = 1.5f; // Distância que o zumbi enxerga pra frente

        // Cria uma máscara para o raio só bater nas paredes (Layer Default)
        int layerMask = LayerMask.GetMask("Default"); 

        // Atira o laser invisível pra frente
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, lookAhead, layerMask);

        // Se bater numa parede e não for o player...
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            Vector2 normal = hit.normal; // A direção pra onde a parede "aponta"
            
            // Calcula a direção paralela à parede para ele poder escorregar
            Vector2 slideDir = new Vector2(-normal.y, normal.x); 
            
            // Verifica qual lado do escorregão deixa ele mais perto do Player
            Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            if (Vector2.Dot(slideDir, toPlayer) < 0) 
            {
                slideDir = -slideDir; // Inverte pro outro lado se for o caminho mais rápido
            }

            // Quanto mais perto da parede, mais força ele faz pra desviar
            float multiplier = 1f - (hit.distance / lookAhead); 
            force = slideDir * multiplier * 2.5f; // Peso do desvio
        }

        return force;
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        string animDirection = "";
        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            if (moveDirection.x > 0) animDirection = "Dir";
            else animDirection = "Esq";
        }
        else
        {
            if (moveDirection.y > 0) animDirection = "Cima";
            else animDirection = "Baixo";
        }

        string newState = "Run_" + animDirection;
        if (currentState != newState)
        {
            animator.Play(newState);
            currentState = newState;
        }
    }

    void CalculateSeparation()
    {
        separationForce = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, SEPARATION_RADIUS);
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject || !col.CompareTag("Inimigo")) continue;
            Vector2 away = (Vector2)transform.position - (Vector2)col.transform.position;
            separationForce += away.normalized / (away.magnitude + 0.1f);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!(collision.otherCollider is BoxCollider2D)) return;

        if (Time.time - lastChainsawHit < chainsawCooldown) return;

        // 1. TOCA O SOM PRIMEIRO!
        if (audioSource != null && audioSource.isActiveAndEnabled && chainsawHitSound != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(chainsawHitSound, 0.45f); 
        }

        // 2. TOMA O DANO DEPOIS
        TakeDamage(4f * GameController.chainsawMultiplier);

        // 3. SÓ APLICA RECUO SE O ZUMBI AINDA ESTIVER VIVO E NÃO FOR FANTASMA
        if (currentHealth > 0 && type != EnemyType.GhostAxe)
        {
            Vector2 knockDir = (transform.position - collision.transform.position).normalized;
            rb.linearVelocity = knockDir * 2.5f;
            knockbackEndTime = Time.time + 0.15f;
        }

        lastChainsawHit = Time.time;
    }

    void TryPlayZombieSound()
    {
        // Se não tiver componente de áudio ou clipes configurados, ignora
        if (audioSource == null || zombieSounds == null || zombieSounds.Length == 0) return;

        // 1º SORTEIO: 25% de chance de fazer QUALQUER som
        if (Random.value > 0.25f) return; 

        // 2º SORTEIO: Qual som vai tocar? (Sorteio de 0 a 1)
        float roll = Random.value;
        int soundIndex = 0;

        if (roll <= 0.20f) 
        {
            soundIndex = 0; // 20% de chance (Som 1)
        }
        else if (roll <= 0.40f) 
        {
            soundIndex = 1; // 20% de chance (Som 2)
        }
        else if (roll <= 0.70f) 
        {
            soundIndex = 2; // 30% de chance (Som 3)
        }
        else 
        {
            soundIndex = 3; // 30% de chance (Som 4)
        }

        // Toca o som escolhido (usamos PlayOneShot para não cortar um som no meio se outro tentar tocar)
        audioSource.PlayOneShot(zombieSounds[soundIndex]);
    }
}