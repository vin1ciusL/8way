using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public float explosionRadius = 4f;
    public float damage = 50f;
    
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
                hit.GetComponent<EnemyController>().TakeDamage(damage);
            }
        }
        Destroy(gameObject); // Some depois de explodir
    }
}
