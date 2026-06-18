using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JugadorMovimiento : MonoBehaviour
{
    [Header("Particulas de movimiento")]
    public ParticleSystem particulasMovimiento;
    private ParticleSystem.EmissionModule emisionParticulas;

    [Header("Referencias Visuales")]
    public Transform pivoteArma;

    [Header("Efectos de Sonido (Pasos)")]
    public AudioClip sonidoPasos;
    [Tooltip("Tiempo en segundos entre cada sonido de paso")]
    public float tiempoEntrePasos = 0.35f;
    private float tiempoProximoPaso = 0f;

    private Rigidbody2D rb;
    private Vector2 direccionMovimiento;

    private float multiplicadorActivo = 1f;

    // NUEVO: Variable pública de solo lectura para saber si es inmune a golpes
    public bool tieneAdrenalinaActiva { get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        if (particulasMovimiento != null)
        {
            emisionParticulas = particulasMovimiento.emission;
        }
    }

    private void Update()
    {
        bool estaMoviendo = direccionMovimiento.sqrMagnitude > 0.0001f;

        if (particulasMovimiento != null)
        {
            float rate = estaMoviendo ? 35f : 0f;
            emisionParticulas.rateOverTime = new ParticleSystem.MinMaxCurve(rate);
        }

        // --- LÓGICA DE SONIDO DE PASOS ---
        if (estaMoviendo && Time.time >= tiempoProximoPaso && sonidoPasos != null)
        {
            // Reproducimos el paso con volumen al 30% (0.3f) para que no aturda
            AudioSource.PlayClipAtPoint(sonidoPasos, transform.position, 0.3f);

            // Calculamos cuándo debe sonar el siguiente. 
            // Si el jugador corre más rápido por adrenalina, los pasos suenan más rápido.
            float intervaloReal = tiempoEntrePasos / multiplicadorActivo;
            tiempoProximoPaso = Time.time + intervaloReal;
        }
    }

    public void Mover(Vector2 inputMovimiento, float velocidad)
    {
        direccionMovimiento = inputMovimiento * (velocidad * multiplicadorActivo);
    }

    public void Apuntar(Vector2 direccionMirando)
    {
        if (pivoteArma != null)
        {
            float angulo = Mathf.Atan2(direccionMirando.y, direccionMirando.x) * Mathf.Rad2Deg;
            pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direccionMovimiento * Time.fixedDeltaTime);
    }

    public void InyectarAdrenalina(float multiplicador, float tiempo)
    {
        StopAllCoroutines();
        StartCoroutine(RutinaAdrenalina(multiplicador, tiempo));
    }

    private IEnumerator RutinaAdrenalina(float multiplicador, float tiempo)
    {
        multiplicadorActivo = multiplicador;
        tieneAdrenalinaActiva = true; // Activa la inmunidad a ralentizaciones
        Debug.Log("<color=cyan>JugadorMovimiento: ¡Adrenalina AL MÁXIMO!</color>");

        yield return new WaitForSeconds(tiempo);

        multiplicadorActivo = 1f;
        tieneAdrenalinaActiva = false; // Pierde la inmunidad
        Debug.Log("<color=grey>JugadorMovimiento: El efecto de adrenalina ha terminado.</color>");
    }
}