using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float maxHealth = 20f;
    private float currentHealth;

    private Transform playerTransform;
    private Rigidbody2D rb;
    public float moveSpeed = 1f;

    private float updatePathInterval = 0.2f;
    private float lastPathUpdateTime = 0f;
    private Vector2 moveDirection = Vector2.zero;

    private float lastChainsawHit = 0f;
    private float chainsawCooldown = 0.1f;
    private float knockbackEndTime = 0f;

    // Flanqueamento: ângulo fixo de desvio do caminho reto
    private float flankAngle = 0f;

    // Separação entre zumbis
    private const float SEPARATION_RADIUS = 1.8f;
    private Vector2 separationForce = Vector2.zero;
    private float lastSeparationTime = 0f;

    // Sistema de "burst": aceleração aleatória
    private float burstEndTime = 0f;
    private float nextBurstTime = 0f;
    private const float BURST_DURATION = 2.5f;
    private const float MAX_DISTANCE_LONG_RANGE = 30f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        gameObject.tag = "Inimigo";

        // Busca o player
        if (PlayerReference.instance != null)
            playerTransform = PlayerReference.instance;
        else
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        // Tier de velocidade: 30% lentos, 45% médios, 25% rápidos
        float roll = Random.value;
        if      (roll < 0.30f) moveSpeed = Random.Range(0.40f, 0.65f);
        else if (roll < 0.75f) moveSpeed = Random.Range(0.75f, 1.10f);
        else                   moveSpeed = Random.Range(1.20f, 1.70f);

        // Ângulo de flanqueamento:
        // 40% vão direto → sem desvio
        // 30% flanqueiam direita
        // 30% flanqueiam esquerda
        float flankRoll = Random.value;
        if      (flankRoll < 0.40f) flankAngle = 0f;
        else if (flankRoll < 0.70f) flankAngle =  Random.Range(20f, 52f);
        else                        flankAngle = -Random.Range(20f, 52f);

        // Stagger nos timers pra não calcular tudo no mesmo frame
        lastPathUpdateTime  = Random.Range(0f, updatePathInterval);
        lastSeparationTime  = Random.Range(0f, 0.35f);
        
        // Inicia um burst aleatório entre 3-6 segundos
        nextBurstTime = Time.time + Random.Range(3f, 6f);
    }

    void Update()
    {
        if (playerTransform == null || GameController.gameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Time.time < knockbackEndTime) return;

        // Separação: calcula com menos frequência que o pathfinding
        if (Time.time - lastSeparationTime >= 0.35f)
        {
            CalculateSeparation();
            lastSeparationTime = Time.time;
        }

        if (Time.time - lastPathUpdateTime >= updatePathInterval)
        {
            Vector2 toPlayer = (playerTransform.position - transform.position);
            float dist = toPlayer.magnitude;

            // Aplica o ângulo de flanqueamento
            // Quando está perto (< 3u), vai mais reto pra fechar o gap
            float flankWeight = Mathf.Clamp01((dist - 1.5f) / 3f);
            float angulo = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float anguloDesviado = angulo + flankAngle * flankWeight;

            Vector2 direcaoFlank = new Vector2(
                Mathf.Cos(anguloDesviado * Mathf.Deg2Rad),
                Mathf.Sin(anguloDesviado * Mathf.Deg2Rad)
            );

            // 75% perseguição com flank, 25% separação dos vizinhos
            Vector2 combined = direcaoFlank * 0.75f + separationForce * 0.25f;
            moveDirection = combined.sqrMagnitude > 0.001f ? combined.normalized : direcaoFlank;

            lastPathUpdateTime = Time.time;
        }

        // Calcula multiplicador de velocidade
        float speedMultiplier = 1f;
        
        // Se muito longe, acelera pra fechar o gap
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer > MAX_DISTANCE_LONG_RANGE)
            speedMultiplier = 1.8f;
        
        // De vez em quando, faz um "burst" de velocidade
        if (Time.time >= nextBurstTime && Time.time < burstEndTime)
        {
            speedMultiplier *= 1.6f; // Fica 60% mais rápido durante burst
        }
        else if (Time.time >= nextBurstTime)
        {
            // Inicia um novo burst
            burstEndTime = Time.time + BURST_DURATION;
            nextBurstTime = Time.time + BURST_DURATION + Random.Range(2f, 4f);
            speedMultiplier *= 1.6f;
        }

        float speed = GameController.isCapturingZone ? moveSpeed * 1.33f : moveSpeed;
        rb.linearVelocity = moveDirection * speed * speedMultiplier;
    }

    void CalculateSeparation()
    {
        separationForce = Vector2.zero;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, SEPARATION_RADIUS);

        foreach (Collider2D col in nearbyColliders)
        {
            if (col == null) continue;
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Inimigo")) continue;

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            float dist = away.magnitude;

            if (dist > 0.01f)
                separationForce += away.normalized * (1f - dist / SEPARATION_RADIUS);
        }

        if (separationForce.sqrMagnitude > 0.01f)
            separationForce.Normalize();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!(collision.otherCollider is BoxCollider2D)) return;

        if (Time.time - lastChainsawHit < chainsawCooldown) return;

        TakeDamage(3.5f * GameController.chainsawMultiplier);

        Vector2 knockDir = (transform.position - collision.transform.position).normalized;
        rb.linearVelocity = knockDir * 2.5f;
        knockbackEndTime = Time.time + 0.15f;
        lastChainsawHit = Time.time;
    }
}