using UnityEngine;
using System;

public static class GameController
{
    private static int coinCount;
    private static int playerHealth = 30;
    private const int MAX_HEALTH = 30;
    
    // NOVO: Variável para guardar o tempo
    public static float gameTime; 

    // Events para UI e outros sistemas
    public static event Action OnHealthChanged;
    public static event Action OnGameOver;

    public static bool gameOver
    {
        get { return playerHealth <= 0; }
    }

    public static int PlayerHealth
    {
        get { return playerHealth; }
    }

    public static int CoinCount
    {
        get { return coinCount; }
    }

    public static void Init()
    {
        coinCount = 6;
        playerHealth = MAX_HEALTH;
        gameTime = 0f;
    }

    public static void getCoin()
    {
        coinCount--;
    }

    public static void TakeDamage(int damage = 1)
    {
        playerHealth -= damage;
        OnHealthChanged?.Invoke();
        
        if (playerHealth <= 0)
        {
            playerHealth = 0;
            OnGameOver?.Invoke();
        }
    }

    public static void Heal(int amount = 1)
    {
        playerHealth = Mathf.Min(playerHealth + amount, MAX_HEALTH);
        OnHealthChanged?.Invoke();
    }
}