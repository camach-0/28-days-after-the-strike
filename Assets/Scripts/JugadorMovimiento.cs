using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JugadorMovimiento : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public Transform pivoteArma; // Lo mudamos aquí, ya que pertenece a la rotación

    private Rigidbody2D rb;
    private Vector2 direccionMovimiento;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    // El Cerebro llama a esta función para inyectar los datos de movimiento
    public void Mover(Vector2 inputMovimiento, float velocidad)
    {
        direccionMovimiento = inputMovimiento * velocidad;
    }

    // El Cerebro llama a esta función para indicarle hacia dónde rotar el brazo
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
        // El Músculo solo aplica la física. ¡Nada de lógica extra aquí!
        rb.MovePosition(rb.position + direccionMovimiento * Time.fixedDeltaTime);
    }
}