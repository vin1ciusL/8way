using UnityEngine;

/// <summary>
/// Configuração centralizada dos limites da arena/mapa.
/// Todos os spawners devem usar essa classe para manter consistência.
/// </summary>
public static class MapConfig
{
    // Limites da arena (devem ser iguais em todos os spawners)
    public const float MinX = -100f;
    public const float MaxX = 100f;
    public const float MinY = -100f;
    public const float MaxY = 100f;

    /// <summary>
    /// Prende uma posição dentro dos limites da arena.
    /// </summary>
    public static Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, MinX, MaxX);
        position.y = Mathf.Clamp(position.y, MinY, MaxY);
        return position;
    }

    /// <summary>
    /// Calcula uma posição aleatória em um círculo ao redor de um ponto central,
    /// e a prende dentro dos limites da arena.
    /// </summary>
    public static Vector3 GetRandomSpawnPosition(
        Transform centerTransform,
        float minRadius,
        float maxRadius)
    {
        // Escolhe uma direção aleatória em 360 graus
        float randomAngle = Random.Range(0f, Mathf.PI * 2);

        // Escolhe uma distância aleatória entre o raio mínimo e máximo
        float randomRadius = Random.Range(minRadius, maxRadius);

        // Calcula o X e Y baseado no ponto central
        float spawnX = centerTransform.position.x + (Mathf.Cos(randomAngle) * randomRadius);
        float spawnY = centerTransform.position.y + (Mathf.Sin(randomAngle) * randomRadius);

        // Prende dentro dos limites da arena
        spawnX = Mathf.Clamp(spawnX, MinX, MaxX);
        spawnY = Mathf.Clamp(spawnY, MinY, MaxY);

        return new Vector3(spawnX, spawnY, 0f);
    }
}
