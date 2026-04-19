using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float maxHealth = 20f; 
    private float currentHealth;

    private Transform playerTransform;
    private Rigidbody2D rb;
    
    public float moveSpeed = 0.75f; 
    
    private float updatePathInterval = 0.2f; 
    private float lastPathUpdateTime = 0f;
    private Vector2 moveDirection = Vector2.zero;

    private float lastChainsawHit = 0f;
    private float chainsawCooldown = 0.1f; 

    // NOVO: Controle de atordoamento do Knockback
    private float knockbackEndTime = 0f;

    void Start()
    {
        currentHealth = maxHealth; 
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
            
        gameObject.tag = "Inimigo";
    }

    void Update()
    {
        if (playerTransform == null || GameController.gameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // NOVO: O zumbi SÓ persegue o player se NÃO estiver tomando knockback
        if (Time.time >= knockbackEndTime)
        {
            if (Time.time - lastPathUpdateTime >= updatePathInterval)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                moveDirection = directionToPlayer;
                lastPathUpdateTime = Time.time;
            }
            
            rb.linearVelocity = moveDirection * moveSpeed;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.otherCollider is BoxCollider2D)
            {
                if (Time.time - lastChainsawHit >= chainsawCooldown)
                {
                    // BUG 3 RESOLVIDO: Dano dobrado! Agora dá 2 de dano por hit (20 DPS)
                    TakeDamage(2f); 
                    
                    // BUG 4 RESOLVIDO: O Knockback agora funciona porque o Update vai parar de brigar com ele
                    Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
                    rb.linearVelocity = knockbackDir * 2.5f; // Força do empurrão
                    
                    // Trava a mente do zumbi por 0.15 segundos para ele ser arremessado
                    knockbackEndTime = Time.time + 0.15f;
                    
                    lastChainsawHit = Time.time;
                }
            }
        }
    }
}