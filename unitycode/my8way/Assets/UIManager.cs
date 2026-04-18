using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject endGamePanel;
    public TextMeshProUGUI healthText; 
    public TextMeshProUGUI coinsText;  

    // NOVAS REFERÊNCIAS DE TEXTO
    public TextMeshProUGUI timerText;      // Texto do tempo na tela do jogo
    public TextMeshProUGUI finalTimeText;  // Texto do tempo na tela de Game Over

    void Start()
    {
        GameController.Init();
        UpdateHealthDisplay();
        
        // Inscreve-se nos eventos do GameController
        GameController.OnHealthChanged += UpdateHealthDisplay;
        GameController.OnGameOver += OnGameOver;
    }

    void OnDestroy()
    {
        // Desinscreve-se dos eventos
        GameController.OnHealthChanged -= UpdateHealthDisplay;
        GameController.OnGameOver -= OnGameOver;
    }

    void Update() 
    {
        // NOVO: Só conta o tempo se o jogo não acabou
        if (!GameController.gameOver)
        {
            GameController.gameTime += Time.deltaTime;
            UpdateTimerDisplay();
        }

        // Mantém display de vida atualizado (redundante mas seguro)
        UpdateHealthDisplay();
    }

    // NOVO: Formata o tempo e atualiza na tela
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Calcula minutos e segundos
            int minutes = Mathf.FloorToInt(GameController.gameTime / 60F);
            int seconds = Mathf.FloorToInt(GameController.gameTime - minutes * 60);
            
            // Formata para ficar no estilo 00:00
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + GameController.PlayerHealth + "/30";
        }
        if (coinsText != null)
        {
            coinsText.text = "Moedas: " + GameController.CoinCount;
        }
    }

    void OnGameOver()
    {
        endGamePanel.SetActive(true);

        // NOVO: Pega o tempo final e exibe no painel de fim de jogo
        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(GameController.gameTime / 60F);
            int seconds = Mathf.FloorToInt(GameController.gameTime - minutes * 60);
            
            finalTimeText.text = "Tempo: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}