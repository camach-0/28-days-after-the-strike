using UnityEngine;

// No le ponemos "abstract" a la clase todavía para mantenerlo simple en las pruebas.
// Esta clase será el padre de JugadorController y de EnemigoBase.
public class Entidad : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float velocidadMovimiento = 5f;

    [Header("Estados")]
    public bool estaMuerto = false;

    // Start se ejecuta al inicio del juego. 
    // Lo hacemos "virtual" para que los hijos (como el jugador) puedan añadirle cosas si quieren.
    public virtual void Start()
    {
        // Al aparecer, la entidad tiene la vida llena
        vidaActual = vidaMaxima;
    }

    // Método universal para recibir daño.
    // También es "virtual" por si el Tank recibe daño de forma distinta (ej. armadura).
    public virtual void RecibirDano(float cantidadDano)
    {
        if (estaMuerto) return; // Si ya está muerto, ignorar

        vidaActual -= cantidadDano;
        Debug.Log(gameObject.name + " recibió " + cantidadDano + " de daño. Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    // Método universal para morir
    public virtual void Morir()
    {
        estaMuerto = true;
        vidaActual = 0;
        Debug.Log(gameObject.name + " ha muerto.");

        // Más adelante, aquí avisaremos al GameDirector o reproduciremos una animación.
        // Por ahora, simplemente destruimos el cuadrado de la escena.
        Destroy(gameObject);
    }
}