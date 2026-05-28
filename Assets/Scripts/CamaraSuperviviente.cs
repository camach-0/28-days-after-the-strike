using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CamaraSuperviviente : MonoBehaviour
{
    [Header("Seguimiento")]
    [Tooltip("Arrastra aquí al jugador que esta cámara debe seguir")]
    public Transform jugadorASeguir;

    [Tooltip("La velocidad con la que la cámara sigue al jugador. 10 = rápido, 2 = suave")]
    public float velocidadSuavizado = 10f;

    [Header("Límites del Mapa (Confiner)")]
    [Tooltip("Arrastra aquí el objeto vacío que tiene el BoxCollider2D de los límites del mapa")]
    public BoxCollider2D limitesMapa;

    // Variables internas para el temblor
    private float duracionTemblor = 0f;
    private float magnitudTemblor = 0f;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (jugadorASeguir == null) return;

        // 1. Dónde debería estar la cámara (siguiendo al jugador)
        Vector3 posicionDestino = new Vector3(jugadorASeguir.position.x, jugadorASeguir.position.y, transform.position.z);

        // 2. Sistema de Límites (Para no mostrar los bordes grises)
        if (limitesMapa != null)
        {
            // Calculamos cuánto mide la pantalla para no salirnos
            float altoPantalla = cam.orthographicSize;
            float anchoPantalla = cam.orthographicSize * cam.aspect;

            float minX = limitesMapa.bounds.min.x + anchoPantalla;
            float maxX = limitesMapa.bounds.max.x - anchoPantalla;
            float minY = limitesMapa.bounds.min.y + altoPantalla;
            float maxY = limitesMapa.bounds.max.y - altoPantalla;

            // Encerramos la cámara matemáticamente
            posicionDestino.x = Mathf.Clamp(posicionDestino.x, minX, maxX);
            posicionDestino.y = Mathf.Clamp(posicionDestino.y, minY, maxY);
        }

        // 3. Sistema de Temblor (Shake)
        if (duracionTemblor > 0)
        {
            posicionDestino += (Vector3)Random.insideUnitCircle * magnitudTemblor;
            duracionTemblor -= Time.deltaTime;
        }

        // 4. Movemos la cámara suavemente hacia el destino final
        transform.position = Vector3.Lerp(transform.position, posicionDestino, velocidadSuavizado * Time.deltaTime);
    }

    // Llama a esta función desde la M60 o cuando un zombi te golpee
    public void HacerTemblar(float magnitud = 0.2f, float duracion = 0.15f)
    {
        magnitudTemblor = magnitud;
        duracionTemblor = duracion;
    }
}