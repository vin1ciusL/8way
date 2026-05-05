using UnityEngine;

public class ChainsawWeapon : MonoBehaviour
{
    // Controla o peso da arma. 25 é bem rápido, mas sólido.
    public float velocidadeGiro = 25f;
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("ChainsawWeapon: MainCamera não encontrada! Certifique-se de ter uma câmera com tag 'MainCamera'.");
    }

    void Update()
    {
        if (mainCamera == null) return; // Seguro contra câmera não encontrada

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
        
        // Gira fisicamente arrastando inimigos, em vez de teleportar
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, velocidadeGiro * Time.deltaTime);
    }
}