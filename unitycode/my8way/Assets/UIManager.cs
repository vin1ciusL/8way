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
    
    // NOVO: Textos das Zonas
    public TextMeshProUGUI zoneProgressText; // Ex: "Capturando: 12.5s"
    public TextMeshProUGUI zoneCountText;    // Ex: "Zonas: 1/4"
    public TextMeshProUGUI popupText;        // Ex: "Granada Desbloqueada!"

    private float popupTimer = 0f;

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
        
        if (zoneProgressText != null) zoneProgressText.text = "";
        if (popupText != null) popupText.text = "";

        UpdateHealthDisplay();
        
        // Inscreve-se nos eventos do GameController
        GameController.OnHealthChanged += UpdateHealthDisplay;
        GameController.OnGameOver += OnGameOver;
        
        // Novos eventos
        Zone.OnCaptureProgress += UpdateCaptureText;
        GameController.OnZoneReward += ShowPopup;
    }

    void OnDestroy()
    {
        // Desinscreve-se dos eventos
        GameController.OnHealthChanged -= UpdateHealthDisplay;
        GameController.OnGameOver -= OnGameOver;
        Zone.OnCaptureProgress -= UpdateCaptureText;
        GameController.OnZoneReward -= ShowPopup;
    }

    void Update() 
    {
        // Atualiza o timer a cada frame
        if (!GameController.gameOver)
        {
            GameController.UpdateGameTime(Time.deltaTime);
            UpdateTimerDisplay();

            // Apaga o aviso depois de 3 segundos
            if (popupTimer > 0)
            {
                popupTimer -= Time.deltaTime;
                if (popupTimer <= 0 && popupText != null) popupText.text = "";
            }
        }
    }

    void UpdateCaptureText(float current, float total)
    {
        if (zoneProgressText == null) return;
        
        if (current <= 0 || current >= total) zoneProgressText.text = "";
        else zoneProgressText.text = "Capturando: " + (total - current).ToString("F1") + "s";
    }

    void ShowPopup(string mensagem)
    {
        if (popupText != null)
        {
            popupText.text = mensagem;
            popupTimer = 3f; // Fica na tela por 3 segundos
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
            coinsText.text = "Comida: " + GameController.CoinCount;
        }
        if (zoneCountText != null)
        {
            zoneCountText.text = "Zonas: " + GameController.zonesCompleted + "/4";
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