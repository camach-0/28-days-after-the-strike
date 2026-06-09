using UnityEngine;

public class ControladorDesfibrilador : ControladorArma
{
    [Header("Configuración del Desfibrilador")]
    public float radioDeAlcance = 1.5f;
    public float tiempoUso = 3f;

    private bool seEstaUsando = false;
    private Vector2 posicionInicial;

    private GestorAcciones miGestor;
    private JugadorInput miInput;

    private void Start()
    {
        SistemaSalud raizJugador = GetComponentInParent<SistemaSalud>();
        miInput = GetComponentInParent<JugadorInput>();

        if (raizJugador != null)
        {
            miGestor = raizJugador.GetComponentInChildren<GestorAcciones>();
        }
    }

    private void Update()
    {
        if (seEstaUsando)
        {
            // REGLA 1: Cancelar por moverse
            if (Vector2.Distance(transform.root.position, posicionInicial) > 0.1f)
            {
                if (miGestor != null) miGestor.CancelarAccion();
                seEstaUsando = false;
                Debug.Log("<color=orange>Desfibrilador cancelado por movimiento.</color>");
            }
            // REGLA 2: Cancelar si suelto el clic izquierdo
            else if (miInput != null && !miInput.EstaDisparando)
            {
                if (miGestor != null) miGestor.CancelarAccion();
                seEstaUsando = false;
                Debug.Log("<color=orange>Desfibrilador cancelado por soltar el botón.</color>");
            }
        }
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando) return;
        UsarDesfibrilador();
    }

    public override void IntentarEmpujon(Vector2 direccion) { }

    private void UsarDesfibrilador()
    {
        Collider2D[] objetosCercanos = Physics2D.OverlapCircleAll(transform.position, radioDeAlcance);
        Cadaver cadaverEncontrado = null;

        foreach (Collider2D col in objetosCercanos)
        {
            Cadaver c = col.GetComponent<Cadaver>();
            if (c != null)
            {
                cadaverEncontrado = c;
                break;
            }
        }

        if (cadaverEncontrado != null)
        {
            seEstaUsando = true;
            posicionInicial = transform.root.position;

            if (miGestor != null)
            {
                miGestor.IniciarAccion(tiempoUso, "REVIVIENDO...", () => CompletarDesfibrilador(cadaverEncontrado));
            }
        }
        else
        {
            Debug.Log("No hay ningún cadáver cerca para revivir.");
        }
    }

    private void CompletarDesfibrilador(Cadaver cadaverEncontrado)
    {
        Debug.Log("¡CLEAR! Electrocutando cuerpo...");
        cadaverEncontrado.AplicarDesfibrilador();

        InventarioJugador miInventario = GetComponentInParent<InventarioJugador>();
        if (miInventario != null)
        {
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }

            if (miInventario.ranuras[0] != null) miInventario.CambiarSlot(0);
            else miInventario.CambiarSlot(1);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radioDeAlcance);
    }
}