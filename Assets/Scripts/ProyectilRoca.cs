using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class ProyectilRoca : MonoBehaviour
{
    public float velocidad = 15f;
    public float danoImpacto = 40f;
    public float fuerzaKnockback = 15f;
    public float tiempoDeVida = 4f;
    public string etiquetaPool = "RocaTank"; // Para reciclarlo

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Vuelo en línea recta estilo Top-Down
    }

    public void Lanzar(Vector2 direccion)
    {
        rb.linearVelocity = direccion * velocidad;
        Invoke(nameof(Desaparecer), tiempoDeVida);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SistemaSalud salud = collision.GetComponent<SistemaSalud>();
            if (salud != null)
            {
                Vector2 direccionGolpe = rb.linearVelocity.normalized;
                salud.RecibirDano(danoImpacto, direccionGolpe, fuerzaKnockback);
            }
            Desaparecer();
        }
        // SOLUCIÓN: Ahora verifica si choca con la capa (Layer) Obstaculos en lugar del Tag
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstaculos"))
        {
            Desaparecer();
        }
    }

    private void Desaparecer()
    {
        CancelInvoke();
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}