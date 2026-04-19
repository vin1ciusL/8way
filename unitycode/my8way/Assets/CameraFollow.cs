using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void Start()
    {
        // Tenta usar PlayerReference singleton primeiro
        if (player == null)
        {
            if (PlayerReference.instance != null)
            {
                player = PlayerReference.instance;
            }
            else
            {
                Debug.LogError("CameraFollow: PlayerReference não encontrada! Tente adicionar PlayerReference ao Player GameObject.");
            }
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
