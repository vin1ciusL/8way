using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float maxHealth = 20f; // Vida suficiente para aguentar ~2 segs de motosserra
    private float currentHealth;

    private Transform playerTransform;
    private Rigidbody2D rb;
    public float moveSpeed = 3f; 
    private float updatePathInterval = 0.2f; 
    private float lastPathUpdateTime = 0f;
    private Vector2 moveDirection = Vector2.zero;

    void Start()
    {
        currentHealth = maxHealth; // Enche a vida ao nascer
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        
        gameObject.tag = "Inimigo";
    }

    void Update()
    {
        if (playerTransform == null || GameController.gameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Time.time - lastPathUpdateTime >= updatePathInterval)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            moveDirection = directionToPlayer;
            lastPathUpdateTime = Time.time;
        }
        
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    // Função chamada pela motosserra para tirar vida
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        // Se a vida zerar, o zumbi morre
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}