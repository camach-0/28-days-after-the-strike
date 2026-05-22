using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Bala : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [Tooltip("Debe ser el mismo nombre que pusiste en el PoolManager")]
    public string etiquetaPool = "BalaBase";

    [Header("Atributos Físicos")]
    public float velocidad = 20f;
    public float tiempoDeVida = 3f;

    [HideInInspector] public int dano;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // ========================================================
    // NUEVO CICLO DE VIDA (Mecánica clave para objetos en Pool)
    // ========================================================
    private void OnEnable()
    {
        // Al encenderse, activamos una cuenta regresiva para "morir" si no choca con nada
        StartCoroutine(DesactivarTrasTiempo());
    }

    private void OnDisable()
    {
        // Al apagarse, limpiamos su inercia para que no nazca moviéndose en su próxima vida
        StopAllCoroutines();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    IEnumerator DesactivarTrasTiempo()
    {
        yield return new WaitForSeconds(tiempoDeVida);

        // Si pasaron 3 segundos en el aire, la devolvemos al Pool
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    public void ConfigurarDireccion(Vector2 direccion)
    {
        if (rb != null)
        {
            rb.linearVelocity = direccion.normalized * velocidad;
        }
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        if (colision.CompareTag("Player") || colision.CompareTag("Bala")) return;

        IReceptorDano objetivo = colision.GetComponent<IReceptorDano>();
        if (objetivo != null)
        {
            objetivo.RecibirDano(dano, rb.linearVelocity.normalized, 0f);
        }

        // ¡ADIÓS DESTROY! La bala simplemente se devuelve a la piscina
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}