using UnityEngine;

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
    }

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    public void ConfigurarDireccion(Vector2 direccion)
    {
        if (rb != null)
        {
            // Usamos velocity, es más estándar y menos propenso a bugs
            rb.linearVelocity = direccion.normalized * velocidad;
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        // 1. Evitar chocar con el propio jugador que dispara
        if (colision.CompareTag("Player")) return;

        IReceptorDano objetivo = colision.GetComponent<IReceptorDano>();

        if (objetivo != null)
        {
            objetivo.RecibirDano(dano, rb.linearVelocity.normalized, 0f);
        }

        Destroy(gameObject);
    }
}