using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JugadorMovimiento : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public Transform pivoteArma;

    private Rigidbody2D rb;
    private Vector2 direccionMovimiento;

    // --- NUEVO: Modificador de velocidad para la Adrenalina ---
    private float multiplicadorActivo = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Mover(Vector2 inputMovimiento, float velocidad)
    {
        // ¡LA MAGIA AQUÍ! Multiplicamos la velocidad que envía el Cerebro por nuestra adrenalina
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

    // =======================================================
    // --- NUEVO: SISTEMA DE INYECCIÓN DE ADRENALINA ---
    // =======================================================
    public void InyectarAdrenalina(float multiplicador, float tiempo)
    {
        // Si ya teníamos adrenalina, detenemos la anterior para que no se bugueen los tiempos
        StopAllCoroutines();
        StartCoroutine(RutinaAdrenalina(multiplicador, tiempo));
    }

    private IEnumerator RutinaAdrenalina(float multiplicador, float tiempo)
    {
        multiplicadorActivo = multiplicador; // Aceleramos
        Debug.Log("<color=cyan>JugadorMovimiento: ¡Adrenalina AL MÁXIMO!</color>");

        yield return new WaitForSeconds(tiempo); // Esperamos 10 segundos

        multiplicadorActivo = 1f; // Volvemos a la normalidad
        Debug.Log("<color=grey>JugadorMovimiento: El efecto de adrenalina ha terminado.</color>");
    }
}