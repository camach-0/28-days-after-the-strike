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
        // Evitamos que la bala choque con el propio jugador que la disparó
        if (!collision.gameObject.CompareTag("Player"))
        {
            // Más adelante, aquí detectaremos si chocó con un "Entidad" para restarle vida
            Debug.Log("La bala chocó con: " + collision.name);
            Destroy(gameObject); // La bala se destruye al impactar
        }
    }
}