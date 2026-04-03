using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    AudioSource audio;
    public float speed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Variaveis para capturar o input do jogador
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Cria um vetor de movimento com base no input do jogador
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        // Move o jogador aplicando o vetor de movimento ao Rigidbody2D
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }

    // Detecta colisões com objetos marcados como "Coletavel"
    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto colidido tem a tag "Coletavel"
        if (other.gameObject.CompareTag("Coletavel"))
        {
            audio.Play();
            Destroy(other.gameObject);
        }
    }
}
