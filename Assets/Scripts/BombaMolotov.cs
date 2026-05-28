using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BombaMolotov : MonoBehaviour
{
    [Header("Configuración del Pool")]
    public string etiquetaPool = "BombaMolotov";
    public string etiquetaFuego = "FuegoMolotov";

    [Header("Comportamiento de Vuelo")]
    [Tooltip("Tiempo en segundos antes de que caiga al suelo (Rango Máximo)")]
    public float tiempoMaximoVuelo = 1.0f;

    private Rigidbody2D rb;
    private bool yaExploto = false;
    private float tiempoDeLanzamiento;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        yaExploto = false;
        tiempoDeLanzamiento = Time.time;

        // 1. Iniciamos el cronómetro de vuelo simulando que va por el aire
        StartCoroutine(RutinaDeVuelo());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        // Limpiamos la inercia (compatible con Unity 6)
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator RutinaDeVuelo()
    {
        // Esperamos el tiempo necesario para llegar al rango máximo
        yield return new WaitForSeconds(tiempoMaximoVuelo);

        // Si nadie la detuvo en el aire, explota donde haya caído
        if (!yaExploto)
        {
            Estallar();
        }
    }

    // 2. Chocar contra objetos sólidos (Paredes, cajas, obstáculos)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignorar si choca con otro objeto tirado en el suelo
        if (collision.gameObject.CompareTag("Recogible")) return;

        Estallar();
    }

    // 3. Chocar contra Hitboxes o Áreas (Zombis, Jugadores Aliados)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bala") || collision.CompareTag("Recogible")) return;

        if (collision.CompareTag("Enemy"))
        {
            Estallar();
        }
        else if (collision.CompareTag("Player"))
        {
            // ¡Seguridad! Evitamos que le explote en la mano al que la acaba de lanzar
            if (Time.time - tiempoDeLanzamiento > 0.1f)
            {
                Estallar();
            }
        }
    }

    private void Estallar()
    {
        // Seguro para no instanciar 2 charcos si choca y se acaba el tiempo en el mismo frame
        if (yaExploto) return;
        yaExploto = true;

        // Instanciamos el charco de fuego exactamente donde está la botella
        PoolManager.Instancia.SolicitarObjeto(etiquetaFuego, transform.position, Quaternion.identity);

        // Desaparecemos la botella y la devolvemos al inventario invisible
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}