using UnityEngine;

public class ControladorConsumible : ControladorArma
{
    [Header("Configuración del Consumible")]
    public float cantidadCuracion = 50f;

    [Header("Sistema de Botín (Drop)")]
    [Tooltip("El nombre exacto en el PoolManager para la versión de SUELO de este botiquín. Ej: Pickup_Botiquin")]
    public string etiquetaSuelo = "Pickup_Botiquin";

    private SistemaSalud saludJugador;
    private bool seEstaUsando = false;

    // =================================================================
    // ¡NUEVO! Cumplimos con el contrato del molde ControladorArma
    // =================================================================
    //public override string EtiquetaPoolSuelo => etiquetaSuelo;

    private void Start()
    {
        // Buscamos el SistemaSalud en el jugador padre
        saludJugador = GetComponentInParent<SistemaSalud>();
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando || saludJugador == null) return;

        if (saludJugador.vidaActual >= saludJugador.vidaMaxima)
        {
            Debug.Log("Vida al máximo, no se necesita curación.");
            return;
        }

        AplicarCuracion();
    }

    private void AplicarCuracion()
    {
        seEstaUsando = true;

        // 1. Curamos usando el método limpio
        saludJugador.Curar(cantidadCuracion);
        Debug.Log($"¡Te has curado! Vida actual: {saludJugador.vidaActual}");

        // 2. Comunicación con el inventario para vaciar la ranura
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

            // Cambiamos automáticamente al arma principal o secundaria tras curarnos
            if (miInventario.ranuras[0] != null) miInventario.CambiarSlot(0);
            else miInventario.CambiarSlot(1);
        }

        // Destruimos el objeto de la mano porque ya nos lo gastamos
        Destroy(gameObject);
    }

    // Cumplimos con la obligación de tener un botón de empuje (aunque no haga nada)
    public override void IntentarEmpujon(Vector2 direccion) { }
}