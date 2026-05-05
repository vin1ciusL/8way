using UnityEngine;

/// <summary>
/// Singleton que fornece uma referência centralizada ao Transform do Player.
/// Evita múltiplas chamadas a FindGameObjectWithTag que são O(n).
/// </summary>
public class PlayerReference : MonoBehaviour
{
    /// <summary>
    /// Instância singleton do Transform do Player
    /// </summary>
    public static Transform instance { get; private set; }

    void Awake()
    {
        // Implementa padrão singleton com proteção contra duplicatas
        if (instance != null && instance != transform)
        {
            Debug.LogWarning("Múltiplas instâncias de PlayerReference encontradas. Destruindo cópia.");
            Destroy(gameObject);
            return;
        }

        instance = transform;
    }

    void OnDestroy()
    {
        if (instance == transform)
        {
            instance = null;
        }
    }
}
