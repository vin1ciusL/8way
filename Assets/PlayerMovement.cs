using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private AudioSource audioSource;
    public float speed;
    public float suavidade = 25f; 
    private Vector2 velocidadeAtual;

    private float lastDamageTime = 0f;
    private float damageCooldown = 0.4f;
    private int enemiesInContact = 0;
    private int danoAcumuladoDosInimigos = 0;

    // NOVO: Referência para a animação
    public Animator animator; 
    private string currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (GameController.gameOver) { velocidadeAtual = Vector2.zero; return; }

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        float velocidadeFinal = speed * GameController.playerSpeedMultiplier; 

        Vector2 velocidadeAlvo = new Vector2(moveHorizontal, moveVertical).normalized * velocidadeFinal;
        velocidadeAtual = Vector2.Lerp(velocidadeAtual, velocidadeAlvo, suavidade * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + velocidadeAtual * Time.fixedDeltaTime);
    }

    void Update()
    {
        // Sistema de Dano
        if (enemiesInContact > 0 && Time.time - lastDamageTime >= damageCooldown)
        {
            // O dano não é mais fixo em "2", é o dano acumulado dos inimigos que encostaram
            GameController.TakeDamage(danoAcumuladoDosInimigos); 
            lastDamageTime = Time.time;
        }

        // --- SISTEMA DE ANIMAÇÃO 4-WAY ---
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        // Se esquecer de colocar o Animator ou morrer, não faz nada
        if (animator == null || GameController.gameOver) return;

        // 1. Descobre se está andando (Run) ou parado (Idle)
        bool isMoving = velocidadeAtual.magnitude > 0.1f;

        // 2. Descobre para onde o mouse está apontando
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDir = (mousePos - transform.position).normalized;

        string animCategory = isMoving ? "Run_" : "Idle_";
        string animDirection = "";

        // 3. Divide a tela num "X" para saber a direção exata
        if (Mathf.Abs(aimDir.x) > Mathf.Abs(aimDir.y))
        {
            // O mouse está mais para os lados do que para cima/baixo
            if (aimDir.x > 0) animDirection = "Dir";
            else animDirection = "Esq";
        }
        else
        {
            // O mouse está mais para cima/baixo do que para os lados
            if (aimDir.y > 0) animDirection = "Cima";
            else animDirection = "Baixo";
        }

        // 4. Junta as palavras (Ex: "Run_" + "Dir" = "Run_Dir")
        string newState = animCategory + animDirection;

        // 5. Toca a animação apenas se ela for diferente da que já está tocando
        if (currentState != newState)
        {
            animator.Play(newState);
            currentState = newState;
        }
    }

    // --- (MANTÉM SUAS FUNÇÕES DE COLISÃO INTACTAS ABAIXO) ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coletavel"))
        {
            audioSource.Play(); 
            GameController.Heal(10); 
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo") && collision.otherCollider is CircleCollider2D)
        {
            enemiesInContact++; 
            // Pega o dano do zumbi específico que encostou
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null) danoAcumuladoDosInimigos += enemy.damageToPlayer;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo") && collision.otherCollider is CircleCollider2D)
        {
            enemiesInContact--; 
            // Remove o dano do zumbi que saiu
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null) danoAcumuladoDosInimigos -= enemy.damageToPlayer;

            if (enemiesInContact <= 0) 
            {
                enemiesInContact = 0;
                danoAcumuladoDosInimigos = 0; // Limpa para evitar dano fantasma
            }
        }
    }
}