using UnityEngine;
using System;

public static class GameController
{
    private static int coinCount;
    private static int playerHealth = 30;
    private const int MAX_HEALTH = 30;
    private static float gameTime = 0f; 

    // --- NOVAS MECÂNICAS ---
    public static int zonesCompleted = 0;
    public static bool isCapturingZone = false;
    public static float chainsawMultiplier = 1f;
    public static float playerSpeedMultiplier = 1f;
    public static bool hasGrenade = false;

    public static event Action OnHealthChanged;
    public static event Action OnGameOver;
    public static event Action OnGameWin; // NOVO EVENTO
    public static event Action<string> OnZoneReward;

    // O jogo acaba se morrer OU se completar as 4 áreas
    public static bool gameOver { get { return playerHealth <= 0 || zonesCompleted >= 4; } }
    public static int PlayerHealth { get { return playerHealth; } }
    public static int CoinCount { get { return coinCount; } }
    public static float GameTime { get { return gameTime; } }

    public static void Init()
    {
        coinCount = 0;
        playerHealth = MAX_HEALTH;
        gameTime = 0f;
        
        zonesCompleted = 0;
        isCapturingZone = false;
        chainsawMultiplier = 1f;
        playerSpeedMultiplier = 1f;
        hasGrenade = false;
    }

    public static void UpdateGameTime(float deltaTime)
    {
        if (!gameOver) gameTime += deltaTime;
    }

    public static void TakeDamage(int damage = 1)
    {
        playerHealth -= damage;
        OnHealthChanged?.Invoke();
        if (playerHealth <= 0) { playerHealth = 0; OnGameOver?.Invoke(); }
    }

    // A cura agora recebe quanto quer curar
    public static void Heal(int amount)
    {
        playerHealth = Mathf.Min(playerHealth + amount, MAX_HEALTH);
        OnHealthChanged?.Invoke();
    }

    // Lida com as recompensas
    public static void CompleteZone()
    {
        zonesCompleted++;
        
        string aviso = "";
        if (zonesCompleted == 1) { hasGrenade = true; aviso = "GRANADA DESBLOQUEADA!"; }
        else if (zonesCompleted == 2) { chainsawMultiplier = 1.5f; aviso = "+50% DANO DA MOTOSSERRA!"; }
        else if (zonesCompleted == 3) { playerSpeedMultiplier = 1.5f; aviso = "+50% VELOCIDADE!"; }
        else if (zonesCompleted == 4) { OnGameWin?.Invoke(); aviso = "TODAS AS ZONAS CAPTURADAS!"; }

        OnZoneReward?.Invoke(aviso); // Manda o texto pra UI
    }
}