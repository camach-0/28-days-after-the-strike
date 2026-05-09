using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bala : MonoBehaviour
{
    [Header("Estadísticas")]
    public float velocidad = 15f;
    public float tiempoVida = 2f; // Se destruye tras 2 segundos si no choca con nada
    public float dano = 25f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // La bala no debe caerse
    }

    private void Start()
    {
        // Seguro de limpieza: destruye la bala para que no se llene la memoria de tu PC
        Destroy(gameObject, tiempoVida);
    }

    // El jugador llamará a esta función justo al disparar
    public void ConfigurarDireccion(Vector2 direccionMirando)
    {
        // Le damos un empujón físico constante en la dirección del disparo
        GetComponent<Rigidbody2D>().linearVelocity = direccionMirando * velocidad;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Evitamos chocar con el jugador que nos disparó o con paredes invisibles
        if (collision.CompareTag("Player")) return;

        // 2. Buscamos si el objeto con el que chocamos tiene el script "Entidad" (o sea, si está vivo)
        Entidad objetivo = collision.GetComponent<Entidad>();

        // 3. Si tiene el script, ¡le hacemos daño!
        if (objetivo != null)
        {
            objetivo.RecibirDano(dano); // Usamos la variable 'dano' que ya tenías arriba (ej. 25f)
        }

        // 4. Sin importar si chocó con un enemigo o con una pared de tu mapa, la bala se destruye
        Destroy(gameObject);
    }
}