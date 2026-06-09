using UnityEngine;

public class ControladorConsumible : ControladorArma
{
    [Header("Configuración del Consumible")]
    public float cantidadCuracion = 50f;
    public float tiempoUso = 4f;

    [Header("Sistema de Botín (Drop)")]
    public string etiquetaSuelo = "Pickup_Botiquin";

    private SistemaSalud saludJugador;
    private bool seEstaUsando = false;
    private Vector2 posicionInicial;

    private void Start()
    {
        saludJugador = GetComponentInParent<SistemaSalud>();
    }

    private void Update()
    {
        // REGLA: Cancelar si el jugador se mueve mientras carga
        if (seEstaUsando)
        {
            if (Vector2.Distance(transform.position, posicionInicial) > 0.1f)
            {
                GestorAcciones.Instancia.CancelarAccion();
                seEstaUsando = false;
                Debug.Log("<color=orange>Curación cancelada por movimiento.</color>");
            }
        }
    }

    // CLIC IZQUIERDO: Curarse a uno mismo
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando || saludJugador == null) return;
        if (saludJugador.vidaActual >= saludJugador.vidaMaxima) return;

        seEstaUsando = true;
        posicionInicial = transform.position;
        GestorAcciones.Instancia.IniciarAccion(tiempoUso, "CURÁNDOSE...", () => AplicarCuracion(saludJugador));
    }

    // CLIC DERECHO: Curar a un compañero cercano
    public override void IntentarEmpujon(Vector2 direccion)
    {
        if (seEstaUsando || saludJugador == null) return;

        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        SistemaSalud aliadoDestino = null;

        foreach (var col in cercanos)
        {
            if (col.gameObject != saludJugador.gameObject)
            {
                SistemaSalud s = col.GetComponent<SistemaSalud>();
                if (s != null && s.esSuperviviente && !s.estaMuertoDefinitivo && !s.estaIncapacitado && s.vidaActual < s.vidaMaxima)
                {
                    aliadoDestino = s;
                    break;
                }
            }
        }

        if (aliadoDestino != null)
        {
            seEstaUsando = true;
            posicionInicial = transform.position;
            GestorAcciones.Instancia.IniciarAccion(tiempoUso, "CURANDO A COMPAÑERO...", () => AplicarCuracion(aliadoDestino));
        }
    }

    private void AplicarCuracion(SistemaSalud objetivo)
    {
        objetivo.Curar(cantidadCuracion);
        Debug.Log($"<color=green>Botiquín aplicado a {objetivo.gameObject.name}</color>");

        ConsumirYDestruir();
    }

    private void ConsumirYDestruir()
    {
        InventarioJugador miInv = GetComponentInParent<InventarioJugador>();
        if (miInv != null)
        {
            for (int i = 0; i < miInv.ranuras.Length; i++)
            {
                if (miInv.ranuras[i] == this) { miInv.ranuras[i] = null; break; }
            }
            if (miInv.ranuras[0] != null) miInv.CambiarSlot(0);
            else miInv.CambiarSlot(1);
        }
        Destroy(gameObject);
    }
}