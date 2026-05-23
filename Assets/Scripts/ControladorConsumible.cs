using UnityEngine;

public class ControladorConsumible : ControladorArma
{
    [Header("Configuración del Consumible")]
    public float cantidadCuracion = 50f;

    private SistemaSalud saludJugador;
    private bool seEstaUsando = false;

    private void Start()
    {
        // ¡ACTUALIZADO! Ahora buscamos el SistemaSalud en lugar de Entidad
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

        // 1. Curamos usando el nuevo método limpio (La barra UI se actualiza sola)
        saludJugador.Curar(cantidadCuracion);
        Debug.Log($"¡Te has curado! Vida actual: {saludJugador.vidaActual}");

        // 2. Comunicación con el inventario
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
    public override void IntentarEmpujon(Vector2 direccion) { }
}