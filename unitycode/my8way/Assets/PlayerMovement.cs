using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private AudioSource audioSource;
    public float speed;
    public float suavidade = 25f; 
    private Vector2 velocidadeAtual;

    private float lastDamageTime = 0f;
    private float damageCooldown = 0.4f; // Reduzido de 0.5s para dano mais frequente
    
    // Contador de quantos zumbis estão encostados (suporta múltiplos inimigos)
    private int enemiesInContact = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (GameController.gameOver)
        {
            velocidadeAtual = Vector2.zero;
            return;
        }

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 velocidadeAlvo = new Vector2(moveHorizontal, moveVertical).normalized * speed;
        velocidadeAtual = Vector2.Lerp(velocidadeAtual, velocidadeAlvo, suavidade * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + velocidadeAtual * Time.fixedDeltaTime);
    }

    void Update()
    {
        // Aplica dano com cooldown enquanto há 1 ou mais inimigos em contato
        if (enemiesInContact > 0 && Time.time - lastDamageTime >= damageCooldown)
        {
            GameController.TakeDamage(2); // Dano aumentado de 1 para 2
            lastDamageTime = Time.time;
        }
    }

    // Moedas continuam sendo Trigger (Atravessam)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coletavel"))
        {
            audioSource.Play(); 
            GameController.getCoin();
            Destroy(other.gameObject);
        }
    }

    // Detecta entrada em colisão com inimigo
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo"))
        {
            if (collision.otherCollider is CircleCollider2D)
            {
                enemiesInContact++; // Incrementa contador
            }
        }
    }

    // Detecta saída de colisão com inimigo
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo"))
        {
            if (collision.otherCollider is CircleCollider2D)
            {
                enemiesInContact--; // Decrementa contador
                
                // Segurança: evita que o número fique negativo por algum bug de física
                if (enemiesInContact < 0)
                    enemiesInContact = 0;
            }
        }
    }
}