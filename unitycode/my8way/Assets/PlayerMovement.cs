using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private AudioSource audioSource;
    public float speed;
    
    // NOVO: Controla a rapidez com que ele acelera e freia. 
    // Valores altos (ex: 15-20) = mais duro. Valores baixos (ex: 5) = desliza mais.
    public float suavidade = 15f; 
    private Vector2 velocidadeAtual; // Guarda a força do movimento frame a frame

    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        // Se o jogador está morto, zera a velocidade e não mexe
        if (GameController.gameOver)
        {
            velocidadeAtual = Vector2.zero;
            return;
        }

        // Mantemos o GetAxisRaw para captar o clique exato
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // Esta é a velocidade máxima que o jogador quer alcançar nesta direção
        Vector2 velocidadeAlvo = new Vector2(moveHorizontal, moveVertical).normalized * speed;

        // O 'Lerp' empurra a 'velocidadeAtual' em direção à 'velocidadeAlvo' de forma suave
        velocidadeAtual = Vector2.Lerp(velocidadeAtual, velocidadeAlvo, suavidade * Time.fixedDeltaTime);

        // Move o jogador usando essa velocidade suavizada
        rb.MovePosition(rb.position + velocidadeAtual * Time.fixedDeltaTime);
    }

    // Detecta colisões com objetos marcados como "Coletavel" ou "Inimigo"
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coletavel"))
        {
            audioSource.Play(); 
            GameController.getCoin();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Inimigo"))
        {
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                GameController.TakeDamage(1);
                lastDamageTime = Time.time;
            }
        }
    }
}