using UnityEngine;
using UnityEngine.InputSystem; // <-- ¡NUEVO! Necesario para el nuevo sistema de controles

public class InteraccionRescate : MonoBehaviour
{
    [Header("Configuración de Rescate")]
    [Tooltip("Arrastra aquí la acción 'Interactuar' de tu Input Actions")]
    public InputActionReference accionInteractuar; // <-- Cambiamos string por InputActionReference

    public float radioInteraccion = 2f;
    public float tiempoParaLevantar = 3f;
    public float vidaAlLevantar = 30f;

    [Tooltip("Capa de los jugadores para que el radar los encuentre")]
    public LayerMask capaJugadores;

    private SistemaSalud miSalud;
    private SistemaSalud objetivoCaido;
    private float temporizadorRescate = 0f;
    private bool estaRescatando = false;

    private void Start()
    {
        miSalud = GetComponent<SistemaSalud>();
    }

    private void Update()
    {
        // 1. Si YO estoy incapacitado o muerto, no puedo ser el héroe
        if (miSalud != null && (miSalud.estaIncapacitado || miSalud.vidaActual <= 0))
        {
            CancelarRescate();
            return;
        }

        // 2. Leemos si el botón está siendo presionado en este momento
        bool botonPresionado = accionInteractuar != null && accionInteractuar.action.IsPressed();

        // 3. Si presiono y MANTENGO el botón
        if (botonPresionado)
        {
            if (!estaRescatando)
            {
                BuscarCompaneroCaido();
            }

            if (estaRescatando && objetivoCaido != null)
            {
                if (!objetivoCaido.estaIncapacitado)
                {
                    CancelarRescate();
                    return;
                }

                temporizadorRescate += Time.deltaTime;
                Debug.Log($"<color=cyan>Rescatando... {temporizadorRescate:F1}s / {tiempoParaLevantar}s</color>");

                if (temporizadorRescate >= tiempoParaLevantar)
                {
                    CompletarRescate();
                }
            }
        }
        // 4. Si suelto el botón o no lo estoy tocando
        else
        {
            CancelarRescate();
        }
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
                temporizadorRescate = 0f;
                Debug.Log("<color=yellow>¡Comenzando a levantar al compañero!</color>");
                break;
            }
        }
    }

    private void CompletarRescate()
    {
        if (objetivoCaido != null)
        {
            objetivoCaido.LevantarRescatado(vidaAlLevantar);
            Debug.Log("<color=green>¡Compañero levantado con éxito!</color>");
        }
        CancelarRescate();
    }

    private void CancelarRescate()
    {
        if (estaRescatando)
        {
            Debug.Log("<color=red>Rescate cancelado o interrumpido.</color>");
        }
        estaRescatando = false;
        temporizadorRescate = 0f;
        objetivoCaido = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}