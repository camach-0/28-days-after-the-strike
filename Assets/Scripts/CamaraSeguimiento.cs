using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform objetivo;
    public float suavizado = 5f;

    private void LateUpdate()
    {
        // 1. Si la cámara no tiene objetivo, lo busca automáticamente
        if (objetivo == null)
        {
            // Busca en la escena cualquier objeto que tenga la etiqueta "Player"
            GameObject jugadorGenerado = GameObject.FindGameObjectWithTag("Player");

            if (jugadorGenerado != null)
            {
                objetivo = jugadorGenerado.transform; // ¡Lo encontró! Ahora es su objetivo
            }
            else
            {
                return; // Si nadie ha presionado Start, la cámara simplemente no se mueve
            }
        }

        // 2. Si ya tiene objetivo, lo sigue suavemente (el código que ya tenías)
        Vector3 posicionDeseada = new Vector3(objetivo.position.x, objetivo.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
    }
}