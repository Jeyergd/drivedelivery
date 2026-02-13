using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target; // Arraste o carro para cá

    [Header("Configurações")]
    public float smoothSpeed = 0.125f; // Quão "suave" é o movimento (0 a 1)
    public Vector3 offset; // A distância fixa entre a câmera e o carro

    void Start()
    {
        // Se você posicionar a câmera manualmente no Editor, 
        // isso calcula o offset automático no início.
        if (target != null && offset == Vector3.zero)
        {
            offset = transform.position - target.position;
        }
    }

    // Usamos LateUpdate para câmeras para garantir que o carro 
    // já se moveu antes de a câmera tentar segui-lo.
    void LateUpdate()
    {
        if (target == null) return;

        // Posição desejada baseada no deslocamento
        Vector3 desiredPosition = target.position + offset;

        // Interpolação suave (Lerp) entre a posição atual e a desejada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplica a nova posição
        transform.position = smoothedPosition;
    }
}