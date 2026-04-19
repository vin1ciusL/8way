using UnityEngine;

public class ComidaTimer : MonoBehaviour
{
    void Start()
    {
        // Destrói este objeto automaticamente depois de 10 segundos!
        Destroy(gameObject, 10f);
    }
}
