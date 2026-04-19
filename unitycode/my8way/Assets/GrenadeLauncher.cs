using UnityEngine;

public class GrenadeLauncher : MonoBehaviour
{
    public GameObject grenadePrefab;
    private float cooldown = 15f;
    private float timer = 0f;

    void Update()
    {
        if (!GameController.hasGrenade || GameController.gameOver) return;

        timer += Time.deltaTime;
        if (timer >= cooldown)
        {
            ThrowGrenade();
            timer = 0f; // Reseta os 15 segundos
        }
    }

    void ThrowGrenade()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 dir = (mousePos - transform.position).normalized;

        GameObject g = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
        g.GetComponent<Grenade>().Setup(dir);
    }
}
