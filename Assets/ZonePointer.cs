using UnityEngine;

public class ZonePointer : MonoBehaviour
{
    private Transform originPoint; // O ponto base (Player ou BaseMotosserra)
    private SpriteRenderer spriteRenderer;
    
    // Mesma suavidade da Motosserra
    public float velocidadeGiro = 25f;

    void Start()
    {
        // Como a seta é filha do Player/BaseMotosserra, pegamos a posição do PAI para a matemática
        originPoint = transform.parent;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (originPoint == null)
            Debug.LogError("ZonePointer: A Seta precisa ser filha do Player ou BaseMotosserra na Hierarchy!");
    }

    void Update()
    {
        if (originPoint == null) return;

        // Acha todas as zonas na cena
        Zone[] zonas = FindObjectsByType<Zone>(FindObjectsSortMode.None);
        Zone zonaMaisProxima = null;
        float menorDistancia = Mathf.Infinity;

        // Procura a mais perto que ainda NÃO foi completa
        foreach (Zone z in zonas)
        {
            if (!z.isCompleted)
            {
                // Calcula a distância saindo do CENTRO DO PLAYER, e não de onde a seta está
                float dist = Vector3.Distance(originPoint.position, z.transform.position);
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
            spriteRenderer.enabled = true;
            
            // MATEMÁTICA DA MOTOSSERRA: Direção = Alvo - Origem
            Vector3 direction = zonaMaisProxima.transform.position - originPoint.position;
            
            // Descobre o ângulo
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Vira o eixo (-90f porque o triângulo aponta pra cima originalmente)
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            
            // Gira fisicamente com o Lerp (arrastando a seta suavemente)
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, velocidadeGiro * Time.deltaTime);
        }
        else
        {
            // Se não tem mais zona (venceu o jogo), esconde a seta
            spriteRenderer.enabled = false;
        }
    }
}