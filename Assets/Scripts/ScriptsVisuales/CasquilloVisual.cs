using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CasquilloVisual : MonoBehaviour
{
    [Header("Físicas de Expulsión")]
    public float fuerzaMinima = 3f;
    public float fuerzaMaxima = 5f;
    public float torqueAleatorio = 500f; // Qué tan rápido gira en el aire

    [Header("Ciclo de Vida")]
    [Tooltip("Cuánto tiempo se queda en el suelo antes de desaparecer")]
    public float tiempoVida = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Esta función la llamará el arma al disparar
    public void Expulsar(Vector2 direccionArma)
    {
        // 1. Calculamos hacia dónde escupir el casquillo (Perpendicular al arma)
        // Si disparas hacia la derecha, el casquillo sale volando hacia arriba (lado derecho del arma en 2D)
        Vector2 direccionExpulsion = new Vector2(-direccionArma.y, direccionArma.x);

        // Le añadimos un poco de caos aleatorio para que no salgan todos idénticos
        direccionExpulsion += new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));

        // 2. Reseteamos velocidades pasadas (por si lo estamos reciclando)
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 3. ¡Boom! Le aplicamos la fuerza física y el giro
        float fuerzaPum = Random.Range(fuerzaMinima, fuerzaMaxima);
        rb.AddForce(direccionExpulsion.normalized * fuerzaPum, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-torqueAleatorio, torqueAleatorio));

        // 4. Iniciamos el contador de muerte
        StopAllCoroutines();
        StartCoroutine(RutinaApagar());
    }

    private IEnumerator RutinaApagar()
    {
        // Espera a que termine su vida útil
        yield return new WaitForSeconds(tiempoVida);

        // Simplemente se apaga. ¡El PoolManager ya lo tiene en su lista para reusarlo!
        gameObject.SetActive(false);
    }
}