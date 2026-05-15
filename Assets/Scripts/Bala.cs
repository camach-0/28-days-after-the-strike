using UnityEngine;

// Esto obliga a Unity a asegurarse de que el objeto tenga un Rigidbody2D
[RequireComponent(typeof(Rigidbody2D))]
public class Bala : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 20f;
    public float tiempoDeVida = 3f;

    [HideInInspector]
    public int dano;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // CONFIGURACIÓN DE SEGURIDAD ANTIBUGS DESDE EL CÓDIGO
        // Forzamos que sea dinámico, sin gravedad y con detección continua de choques
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    public void ConfigurarDireccion(Vector2 direccion)
    {
        if (rb != null)
        {
            Vector2 velocidadFinal = direccion.normalized * velocidad;
            rb.linearVelocity = velocidadFinal;

            // EL CHIVATO: Esto aparecerá en la consola de Unity
            Debug.Log($"Bala creada | Dir: {direccion.normalized} | Velocidad en Inspector: {velocidad} | Fuerza Final: {velocidadFinal}");
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        // 1. Ignoramos al jugador
        if (colision.CompareTag("Player")) return;

        // 2. NUEVO: Ignoramos otras balas para que la escopeta funcione
        if (colision.CompareTag("Bala")) return;

        IReceptorDano objetivo = colision.GetComponent<IReceptorDano>();

        if (objetivo != null)
        {
            objetivo.RecibirDano(dano, rb.linearVelocity.normalized, 0f);
        }

        Destroy(gameObject);
    }
}