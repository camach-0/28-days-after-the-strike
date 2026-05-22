using UnityEngine;

// Heredamos de ControladorArma para que el Inventario de Jorge lo acepte sin quejarse
public class ControladorConsumible : ControladorArma
{
    [Header("Configuración del Consumible")]
    [Tooltip("¿Cuánta vida recupera?")]
    public float cantidadCuracion = 50f;

    private Entidad jugador;
    private bool seEstaUsando = false;

    private void Start()
    {
        // Buscamos el componente Entidad (que controla la vida) en el jugador que sostiene esto
        jugador = GetComponentInParent<Entidad>();
    }

    // Jorge programó que al apretar R2/Clic se ejecute esto. ¡Nosotros lo hackeamos para curar!
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando || jugador == null) return;

        // Regla de Left 4 Dead: No puedes curarte si ya estás al 100%
        if (jugador.vidaActual >= jugador.vidaMaxima)
        {
            Debug.Log("Vida al máximo, no se necesita curación.");
            return;
        }

        AplicarCuracion();
    }

    private void AplicarCuracion()
    {
        seEstaUsando = true;

        // 1. Curamos al jugador
        jugador.vidaActual += cantidadCuracion;
        if (jugador.vidaActual > jugador.vidaMaxima)
        {
            jugador.vidaActual = jugador.vidaMaxima;
        }

        // 2. Actualizamos la barra de vida
        if (jugador.barraDeVidaUI != null)
        {
            jugador.barraDeVidaUI.fillAmount = jugador.vidaActual / jugador.vidaMaxima;
        }

        Debug.Log($"¡Te has curado! Vida actual: {jugador.vidaActual}");

        // --- ¡LO NUEVO! Comunicación con el inventario ---
        InventarioJugador miInventario = GetComponentInParent<InventarioJugador>();
        if (miInventario != null)
        {
            // Primero, buscamos en qué ranura estábamos (3 o 4) y la dejamos vacía
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }

            // Luego, forzamos al jugador a sacar el Arma Principal (0). 
            // Si no tiene principal, sacará la Secundaria (1).
            if (miInventario.ranuras[0] != null)
            {
                miInventario.CambiarSlot(0);
            }
            else
            {
                miInventario.CambiarSlot(1);
            }
        }

        // 3. Destruimos el objeto curativo
        Destroy(gameObject);
    }
}