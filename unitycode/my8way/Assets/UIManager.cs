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
        
        // Validação de referências
        if (healthText == null)
            Debug.LogError("UIManager: healthText não configurada!");
        if (coinsText == null)
            Debug.LogError("UIManager: coinsText não configurada!");
        if (timerText == null)
            Debug.LogError("UIManager: timerText não configurada!");
        if (finalTimeText == null)
            Debug.LogWarning("UIManager: finalTimeText não configurada (opcional)");
        if (endGamePanel == null)
            Debug.LogError("UIManager: endGamePanel não configurada!");
        
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
        // Atualiza o timer a cada frame
        if (!GameController.gameOver)
        {
            GameController.UpdateGameTime(Time.deltaTime);
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Calcula minutos e segundos
            int minutes = Mathf.FloorToInt(GameController.GameTime / 60F);
            int seconds = Mathf.FloorToInt(GameController.GameTime - minutes * 60);
            
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

        // Pega o tempo final e exibe no painel de fim de jogo
        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(GameController.GameTime / 60F);
            int seconds = Mathf.FloorToInt(GameController.GameTime - minutes * 60);
            
            finalTimeText.text = "Tempo: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}