using UnityEngine;

public class JugadorVisual : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public SpriteRenderer spriteCuerpo; // Asigna aquí el objeto "Cuerpo" (hijo)
    public Animator animator;           // Asigna aquí el propio objeto "Visuales"
    public Transform pivoteArma;        // Asigna aquí el objeto "PivoteArma" (hijo)

    [Header("Ajustes")]
    [Tooltip("Distancia del hombro. Si hay tartamudeo al mirar arriba, ponlo en 0")]
    public float offsetXHombro = 0.1f;

    private JugadorInput inputJugador;
    private readonly int hashVelocidad = Animator.StringToHash("Velocidad");

    private bool mirandoDerecha = true;

    private void Awake()
    {
        inputJugador = GetComponentInParent<JugadorInput>();
    }

    private void Update()
    {
        if (inputJugador == null) return;

        ManejarVolteo();
        ManejarAnimaciones();
    }

    private void ManejarVolteo()
    {
        Vector2 direccion = inputJugador.DireccionMirando;

        // 1. ZONA MUERTA
        if (direccion.x > 0.15f) mirandoDerecha = true;
        else if (direccion.x < -0.15f) mirandoDerecha = false;

        // 2. VOLTEAR EL CUERPO
        if (spriteCuerpo != null)
        {
            spriteCuerpo.flipX = !mirandoDerecha;
        }

        // 3. MOVER EL PIVOTE AL HOMBRO
        pivoteArma.localPosition = new Vector3(mirandoDerecha ? offsetXHombro : -offsetXHombro, pivoteArma.localPosition.y, pivoteArma.localPosition.z);

        // 4. ¡LA SOLUCIÓN DE LA LINTERNA!
        // Mantenemos el pivote siempre normal para que la luz no se rompa
        pivoteArma.localScale = Vector3.one;

        // Buscamos específicamente el arma instalada y LA VOLTEAMOS SOLO A ELLA
        ControladorArma arma = pivoteArma.GetComponentInChildren<ControladorArma>();
        if (arma != null)
        {
            // El arma, al voltearse, también voltea su Muzzle Flash correctamente
            arma.transform.localScale = new Vector3(1, mirandoDerecha ? 1 : -1, 1);
        }
    }

    private void ManejarAnimaciones()
    {
        if (animator == null) return;
        float velocidad = inputJugador.InputMovimiento.magnitude;
        animator.SetFloat(hashVelocidad, velocidad);
    }
}