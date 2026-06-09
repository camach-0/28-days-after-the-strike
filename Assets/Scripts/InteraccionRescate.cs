using UnityEngine;

public class InteraccionRescate : MonoBehaviour
{
    [Header("Configuración de Rescate")]
    public float radioInteraccion = 2f;
    public float tiempoParaLevantar = 3f;
    public float vidaAlLevantar = 30f;

    [Tooltip("Capa de los jugadores para que el radar los encuentre")]
    public LayerMask capaJugadores;

    private SistemaSalud miSalud;
    private JugadorInput miInput;
    private SistemaSalud objetivoCaido;
    private bool estaRescatando = false;

    // --- NUEVO: Variable para vigilar si recibe daño zombi ---
    private float ultimaVidaAliado;

    private void Start()
    {
        miSalud = GetComponent<SistemaSalud>();
        miInput = GetComponent<JugadorInput>();
    }

    private void Update()
    {
        // 1. Si YO estoy incapacitado o muerto, cancelamos cualquier intento de rescate
        if (miSalud != null && (miSalud.estaIncapacitado || miSalud.estaMuertoDefinitivo))
        {
            CancelarRescate();
            return;
        }

        // 2. Leemos si ESTE jugador específico está manteniendo el botón
        bool botonPresionado = miInput != null && miInput.ManteniendoInteractuar;

        // 3. Si presiono y MANTENGO el botón
        if (botonPresionado)
        {
            if (!estaRescatando)
            {
                BuscarCompaneroCaido();
            }
            // Si ya estamos rescatando a alguien...
            else if (objetivoCaido != null)
            {
                // Si se curó mágicamente o murió del todo
                if (!objetivoCaido.estaIncapacitado)
                {
                    CancelarRescate();
                }
                else
                {
                    // --- REGLA DE INTERRUPCIÓN POR DAÑO ---
                    // Comparamos su vida actual con la del frame anterior
                    float danoRecibido = ultimaVidaAliado - objetivoCaido.vidaActualIncapacitado;

                    // Si baja de golpe más de 3 puntos (el sangrado quita menos de eso en un instante)
                    if (danoRecibido > 3f)
                    {
                        CancelarRescate();
                        Debug.Log("<color=red>¡Zombi atacó al caído! Rescate interrumpido.</color>");
                    }
                    else
                    {
                        // Si es solo el desangrado normal, guardamos su vida para vigilarla en el próximo frame
                        ultimaVidaAliado = objetivoCaido.vidaActualIncapacitado;
                    }
                }
            }
        }
        // 4. Si suelto el botón, cancelamos la barra
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

                // ¡AQUÍ GUARDAMOS SU VIDA INICIAL ANTES DE EMPEZAR A LEVANTARLO!
                ultimaVidaAliado = objetivoCaido.vidaActualIncapacitado;

                if (GestorAcciones.Instancia != null)
                {
                    GestorAcciones.Instancia.IniciarAccion(tiempoParaLevantar, "LEVANTANDO A UN COMPAÑERO...", CompletarRescate);
                }
                break; // Solo levantamos a uno a la vez
            }
        }
    }

    // Esta función la llamará automáticamente el Gestor cuando la barra se llene
    private void CompletarRescate()
    {
        if (objetivoCaido != null)
        {
            objetivoCaido.LevantarRescatado(vidaAlLevantar);
            Debug.Log("<color=green>¡Compañero levantado con éxito!</color>");
        }

        // Limpiamos las variables
        estaRescatando = false;
        objetivoCaido = null;
    }

    private void CancelarRescate()
    {
        if (estaRescatando)
        {
            // Le avisamos a la UI que apague la barra
            if (GestorAcciones.Instancia != null)
            {
                GestorAcciones.Instancia.CancelarAccion();
            }
        }

        estaRescatando = false;
        objetivoCaido = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}