using UnityEngine;
using System;

public class Zone : MonoBehaviour
{
    public float captureTimeRequired = 30f;
    private float currentCaptureTime = 0f;
    public bool isCompleted = false;

    // Evento para avisar a UI sobre o progresso
    public static event Action<float, float> OnCaptureProgress; 

    void OnTriggerStay2D(Collider2D collision)
    {
        if (isCompleted || GameController.gameOver) return;

        if (collision.CompareTag("Player"))
        {
            GameController.isCapturingZone = true;
            currentCaptureTime += Time.deltaTime;

            // Avisa a UI quanto tempo falta
            OnCaptureProgress?.Invoke(currentCaptureTime, captureTimeRequired);

            if (currentCaptureTime >= captureTimeRequired)
            {
                CompleteZone();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCompleted)
        {
            GameController.isCapturingZone = false;
            OnCaptureProgress?.Invoke(0, captureTimeRequired); // Esconde o timer
        }
    }

    void CompleteZone()
    {
        isCompleted = true;
        GameController.isCapturingZone = false;
        GetComponent<SpriteRenderer>().color = Color.green;
        GameController.CompleteZone();
        OnCaptureProgress?.Invoke(captureTimeRequired, captureTimeRequired); 
    }
}