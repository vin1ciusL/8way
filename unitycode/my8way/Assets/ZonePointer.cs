using UnityEngine;

public class ZonePointer : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        player = transform.parent; // Pega o Player que é pai da seta
    }

    void Update()
    {
        // Acha todas as zonas na cena
        Zone[] zonas = FindObjectsByType<Zone>(FindObjectsSortMode.None);
        Zone zonaMaisProxima = null;
        float menorDistancia = Mathf.Infinity;

        // Procura a mais perto que ainda NÃO foi completa
        foreach (Zone z in zonas)
        {
            if (!z.isCompleted)
            {
                float dist = Vector2.Distance(player.position, z.transform.position);
                if (dist < menorDistancia)
                {
                    menorDistancia = dist;
                    zonaMaisProxima = z;
                }
            }
        }

        // Se achou uma, aponta pra ela
        if (zonaMaisProxima != null)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            Vector2 dir = zonaMaisProxima.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
        else
        {
            // Se não tem mais zona, esconde a seta
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
