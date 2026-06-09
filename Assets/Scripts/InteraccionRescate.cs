using UnityEngine;

public class InteraccionRescate : MonoBehaviour
{
    [Header("Configuración de Rescate")]
    public float radioInteraccion = 2f;
    public float tiempoParaLevantar = 3f;
    public float vidaAlLevantar = 30f;
    public LayerMask capaJugadores;

    private SistemaSalud miSalud;
    private JugadorInput miInput;
    private SistemaSalud objetivoCaido;
    private bool estaRescatando = false;
    private float ultimaVidaAliado;

    private Vector2 posicionInicial;

    private GestorAcciones miGestor;
    private GestorAcciones gestorAliado;

    private void Start()
    {
        miSalud = GetComponent<SistemaSalud>();
        miInput = GetComponent<JugadorInput>();
        miGestor = GetComponentInChildren<GestorAcciones>();
    }

    private void Update()
    {
        if (miSalud != null && (miSalud.estaIncapacitado || miSalud.estaMuertoDefinitivo))
        {
            CancelarRescate();
            return;
        }

        bool botonPresionado = miInput != null && miInput.ManteniendoInteractuar;

        if (botonPresionado)
        {
            if (!estaRescatando)
            {
                BuscarCompaneroCaido();
            }
            else if (objetivoCaido != null)
            {
                if (Vector2.Distance(transform.root.position, posicionInicial) > 0.1f)
                {
                    CancelarRescate();
                    Debug.Log("<color=orange>Rescate cancelado por movimiento.</color>");
                    return;
                }

                if (!objetivoCaido.estaIncapacitado) CancelarRescate();
                else
                {
                    float danoRecibido = ultimaVidaAliado - objetivoCaido.vidaActualIncapacitado;
                    if (danoRecibido > 3f) CancelarRescate();
                    else ultimaVidaAliado = objetivoCaido.vidaActualIncapacitado;
                }
            }
        }
        else CancelarRescate();
    }

    private void BuscarCompaneroCaido()
    {
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radioInteraccion, capaJugadores);
        foreach (Collider2D col in cercanos)
        {
            if (col.gameObject == this.gameObject) continue;
            SistemaSalud saludOtro = col.GetComponent<SistemaSalud>();

            if (saludOtro != null && saludOtro.estaIncapacitado)
            {
                objetivoCaido = saludOtro;
                estaRescatando = true;
                ultimaVidaAliado = objetivoCaido.vidaActualIncapacitado;

                posicionInicial = transform.root.position;

                gestorAliado = objetivoCaido.GetComponentInChildren<GestorAcciones>();

                if (miGestor != null) miGestor.IniciarAccion(tiempoParaLevantar, "LEVANTANDO A UN COMPAÑERO...", CompletarRescate);
                if (gestorAliado != null) gestorAliado.IniciarAccion(tiempoParaLevantar, "SIENDO LEVANTADO...");
                break;
            }
        }
    }

    private void CompletarRescate()
    {
        if (objetivoCaido != null)
        {
            objetivoCaido.LevantarRescatado(vidaAlLevantar);

            // ---> LA CORRECCIÓN CLAVE ESTÁ AQUÍ <---
            // Buscamos el Input nativo de Unity, es la forma más segura de saber si es Humano o Bot.
            UnityEngine.InputSystem.PlayerInput inputHumano = objetivoCaido.GetComponent<UnityEngine.InputSystem.PlayerInput>();

            // Si NO tiene el control de Unity encendido, entonces es 100% un Bot
            if (inputHumano == null || !inputHumano.enabled)
            {
                AliadoBotController bot = objetivoCaido.GetComponent<AliadoBotController>();
                if (bot != null) bot.enabled = true;

                UnityEngine.AI.NavMeshAgent agente = objetivoCaido.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agente != null) agente.enabled = true;
            }

            Debug.Log("<color=green>¡Compañero levantado con éxito!</color>");
        }

        estaRescatando = false;
        objetivoCaido = null;
        gestorAliado = null;
    }

    private void CancelarRescate()
    {
        if (estaRescatando)
        {
            if (miGestor != null) miGestor.CancelarAccion();
            if (gestorAliado != null) gestorAliado.CancelarAccion();
        }
        estaRescatando = false;
        objetivoCaido = null;
        gestorAliado = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}