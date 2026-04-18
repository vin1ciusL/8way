using UnityEngine;

public class ChainsawWeapon : MonoBehaviour
{
    public float damagePerSecond = 10f;

    void Update()
    {
        // Pega a posição do mouse na tela e converte para o mundo do jogo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Garante que não bagunce o 2D

        // Calcula a direção do player até o mouse
        Vector3 direction = mousePos - transform.position;
        
        // Calcula o ângulo em graus e rotaciona a arma
        // Calcula o ângulo em graus
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // NOVO: Subtrai 90 graus para forçar o eixo Y a ser a "frente"
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
    }
    // Aplica o dano contínuo a cada frame que o inimigo ficar encostado
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Inimigo"))
        {
            // Tenta pegar o script do inimigo e aplicar o dano usando Time.deltaTime
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}