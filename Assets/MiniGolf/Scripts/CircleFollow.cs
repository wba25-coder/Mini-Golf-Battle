using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleFollow : MonoBehaviour
{
    public RectTransform circleRectTransform;  // A referência para o círculo no Canvas
    public Camera mainCamera;  // A câmera principal
    private Transform ballTransform;  // A referência para a bola

    void Start()
    {
        // Encontra a bola pela tag "Ball"
        GameObject ball = GameObject.FindWithTag("Ball");
        if (ball != null)
        {
            ballTransform = ball.transform;
        }
        else
        {
            Debug.LogError("Nenhuma bola foi encontrada na cena com a tag 'Ball'");
        }
        
        // Inicia a rotação do círculo
        StartCoroutine(RotateCircle());
    }

    void Update()
    {
        // Converte a posição da bola no mundo 3D para a posição da tela
        Vector3 screenPos = mainCamera.WorldToScreenPoint(ballTransform.position);
        
        // Atualiza a posição da imagem no Canvas
        circleRectTransform.position = screenPos;
    }

    IEnumerator RotateCircle()
    {
        while (true)
        {
            // Gira o círculo no sentido horário lentamente
            circleRectTransform.Rotate(0, 0, -20 * Time.deltaTime);
            yield return null;  // Espera até o próximo frame
        }
    }
}
