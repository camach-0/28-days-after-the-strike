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
    // ¡Adiós tiempoDeVida fijo! Ahora es un cálculo dinámico según el arma.

    [HideInInspector] public int dano;
    [HideInInspector] public float fuerzaEmpuje;
    [HideInInspector] public int penetracionRestante;

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
        // ¡VACÍO! Ya no iniciamos el contador de muerte aquí.
        // Esperaremos a que el arma nos pase el alcance primero.
    }

    private void OnDisable()
    {
        // Al apagarse, limpiamos su inercia para que no nazca moviéndose en su próxima vida
        StopAllCoroutines();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // ========================================================
    // PUENTE PARA EL BOT (¡ESTO SOLUCIONA EL ÚLTIMO ERROR!)
    // ========================================================
    public void ConfigurarDireccion(Vector2 direccion)
    {
        // Cuando el bot dispara, no nos manda estadísticas. 
        // Así que le ponemos valores por defecto: 10 daño, 5 empuje, 1 penetración, 15 alcance.
        ConfigurarBala(direccion, 10, 5f, 1, 15f);
    }

    // ========================================================
    // Función unificada que recibe toda la balística del jugador
    // ========================================================
    public void ConfigurarBala(Vector2 direccion, int danoArma, float empuje, int penetracion, float alcanceMaximo)
    {
        if (rb != null)
        {
            rb.linearVelocity = direccion.normalized * velocidad;
        }

        // Asignamos las estadísticas puras que nos manda el arma
        dano = danoArma;
        fuerzaEmpuje = empuje;
        penetracionRestante = penetracion;

        // Calculamos matemáticamente cuánto tiempo vivirá basada en su alcance y velocidad
        float tiempoDeVida = alcanceMaximo / velocidad;

        StopAllCoroutines(); // Limpiamos cualquier contador fantasma de su vida pasada
        StartCoroutine(DesactivarTrasTiempo(tiempoDeVida));
    }

    IEnumerator DesactivarTrasTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        // Ignoramos al jugador, otras balas y objetos tirados en el piso
        if (colision.CompareTag("Player") || colision.CompareTag("Bala") || colision.CompareTag("Recogible")) return;

        IReceptorDano objetivo = colision.GetComponent<IReceptorDano>();

        if (objetivo != null)
        {
            // Le pasamos el daño, la dirección y la nueva fuerza de empuje para el Knockback
            objetivo.RecibirDano(dano, rb.linearVelocity.normalized, fuerzaEmpuje);

            // ¡MECÁNICA DE PENETRACIÓN! Restamos a un zombi atravesado
            penetracionRestante--;
        }
        else
        {
            // Si choca contra una pared u obstáculo rígido, pierde toda la penetración de golpe
            penetracionRestante = 0;
        }

        // Solo la devolvemos a la piscina si ya no le queda fuerza de penetración
        if (penetracionRestante <= 0)
        {
            PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
        }
    }
}