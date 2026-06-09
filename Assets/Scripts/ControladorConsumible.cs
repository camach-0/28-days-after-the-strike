using UnityEngine;

public class ControladorConsumible : ControladorArma
{
    [Header("Configuración del Consumible")]
    public float cantidadCuracion = 50f;
    public float tiempoUso = 4f;

    [Header("Sistema de Botín (Drop)")]
    public string etiquetaSuelo = "Pickup_Botiquin";

    private SistemaSalud saludJugador;
    private JugadorInput miInput;
    private bool seEstaUsando = false;
    private Vector2 posicionInicial;
    private float tiempoInicioAccion;

    // ¡NUEVO! Variable directa para saber a quién curamos
    private bool curandoAliado = false;

    private GestorAcciones miGestor;
    private GestorAcciones gestorAliado;

    private void Start()
    {
        saludJugador = GetComponentInParent<SistemaSalud>();
        miInput = GetComponentInParent<JugadorInput>();

        if (saludJugador != null)
        {
            miGestor = saludJugador.GetComponentInChildren<GestorAcciones>();
        }
    }

    private void Update()
    {
        if (seEstaUsando)
        {
            if (Time.time - tiempoInicioAccion < 0.2f) return;

            if (Vector2.Distance(transform.root.position, posicionInicial) > 0.1f)
            {
                CancelarUso("Curación cancelada por movimiento.");
            }
            // ¡NUEVA REGLA! Ya no dependemos de si el aliado tiene UI o no
            else if (!curandoAliado && miInput != null && !miInput.EstaDisparando)
            {
                CancelarUso("Curación cancelada por soltar el botón izquierdo.");
            }
            else if (curandoAliado && miInput != null && !miInput.EstaEmpujando)
            {
                CancelarUso("Curación de aliado cancelada por soltar el botón derecho.");
            }
        }
    }

    private void CancelarUso(string mensaje)
    {
        if (miGestor != null) miGestor.CancelarAccion();
        if (gestorAliado != null) gestorAliado.CancelarAccion();
        seEstaUsando = false;
        gestorAliado = null;
        Debug.Log($"<color=orange>{mensaje}</color>");
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando || saludJugador == null) return;
        if (saludJugador.vidaActual >= saludJugador.vidaMaxima) return;

        seEstaUsando = true;
        curandoAliado = false; // Me curo a mí mismo
        posicionInicial = transform.root.position;
        tiempoInicioAccion = Time.time;
        gestorAliado = null;

        if (miGestor != null)
            miGestor.IniciarAccion(tiempoUso, "CURÁNDOSE...", () => AplicarCuracion(saludJugador));
    }

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
            curandoAliado = true; // Curamos a otro
            posicionInicial = transform.root.position;
            tiempoInicioAccion = Time.time;

            gestorAliado = aliadoDestino.GetComponentInChildren<GestorAcciones>();

            if (miGestor != null)
                miGestor.IniciarAccion(tiempoUso, "CURANDO A COMPAÑERO...", () => AplicarCuracion(aliadoDestino));

            if (gestorAliado != null)
                gestorAliado.IniciarAccion(tiempoUso, "SIENDO CURADO...");
        }
    }

    private void AplicarCuracion(SistemaSalud objetivo)
    {
        objetivo.Curar(cantidadCuracion);
        gestorAliado = null;
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