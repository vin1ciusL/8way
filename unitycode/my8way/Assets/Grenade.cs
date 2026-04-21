using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float speed = 40f;
    public float lifeTime = 2f;
    public float explosionRadius = 4f;
    public float damage = 50f;
    
    // --- NOVA VARIÁVEL PARA O SOM ---
    public AudioClip explosionSound;
    public float volumeDaExplosao = 0.8f; // Um pouco alto pra dar impacto!

    private Vector2 direction;
    private float timer = 0f;

    public void Setup(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Se bater 2 segundos, explode
        if (timer >= lifeTime) Explode();
        else transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se bater num inimigo antes dos 2 seg, explode
        if (other.CompareTag("Inimigo")) Explode();
    }

    void Explode()
    {
        // Acha todo mundo na área e dá dano massivo
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach(var hit in hits)
        {
            if (hit.CompareTag("Inimigo"))
            {
                // Cuidado com inimigos que já podem ter morrido no mesmo frame
                EnemyController enemy = hit.GetComponent<EnemyController>();
                if (enemy != null) enemy.TakeDamage(damage);
            }
        }

        // --- TOCA O SOM DA EXPLOSÃO AQUI ---
        if (explosionSound != null)
        {
            // Cria um áudio temporário na posição atual que sobrevive à destruição da granada
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, volumeDaExplosao);
        }

        Destroy(gameObject); // A granada some, mas o som continua!
    }
}