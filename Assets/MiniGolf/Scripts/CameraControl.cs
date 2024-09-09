using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static CameraControl instance;  // Singleton Instance

    [SerializeField] private float rotationSpeed = 5f;  // Velocidade de rotação da câmera
    [SerializeField] private Vector3 offset = new Vector3(0, 5f, -7f);  // Offset da posição da câmera em relação à bola

    private Transform ballTransform;  // Referência ao Transform da bola
    private BallControl ballControl;  // Referência ao BallControl para acessar startPos, endPos e ballIsStatic

    private void Awake()
    {
        // Implementação do padrão Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Método para configurar o alvo que a câmera vai seguir
    /// </summary>
    /// <param name="target">O objeto da bola que a câmera deve seguir</param>
    public void SetTarget(GameObject target)
    {
        ballTransform = target.transform;  // Pega o Transform do GameObject
        ballControl = target.GetComponent<BallControl>();  // Pega o script BallControl
    }

    private void LateUpdate()
    {
        // Verifica se a bola está parada (ballIsStatic == true)
        if (ballTransform != null && ballControl != null && ballControl.ballIsStatic && ballControl.cameraIsMoving)
        {
            // Calcula a direção baseada em startPos e endPos
            Vector3 direction = ballControl.startPos - ballControl.endPos;

            // Se há uma direção válida
            if (direction != Vector3.zero)
            {
                // Calcular a posição da câmera em relação à bola
                Vector3 desiredPosition = ballTransform.position + offset;
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * rotationSpeed);

                // Calcular o ângulo de rotação baseado na direção da mira
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                // Suavizar a rotação para não ser abrupta
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}
