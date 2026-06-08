using UnityEngine;

public class ControladorAdrenalina : ControladorArma
{
    [Header("Efecto de Adrenalina")]
    public float multiplicadorVelocidad = 1.5f; // Te hace 50% más rápido (1.5x)
    public float tiempoEfecto = 10f; // Dura 10 segundos
    public float saludTemporal = 25f; // ¡NUEVO! HP temporal que te da

    private bool seEstaUsando = false;

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando) return;
        Inyectar();
    }

    public override void IntentarEmpujon(Vector2 direccion) { }

    private void Inyectar()
    {
        seEstaUsando = true;

        // 1. Buscamos el movimiento y le mandamos el efecto de velocidad
        JugadorMovimiento movimiento = GetComponentInParent<JugadorMovimiento>();
        if (movimiento != null)
        {
            movimiento.InyectarAdrenalina(multiplicadorVelocidad, tiempoEfecto);
        }

        // 2. ¡NUEVO! Buscamos el sistema de salud y le damos los 25 HP temporales
        SistemaSalud saludJugador = GetComponentInParent<SistemaSalud>();
        if (saludJugador != null)
        {
            saludJugador.AñadirVidaTemporal(saludTemporal);
        }

        // 3. Sacamos la jeringa del inventario
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

        // 4. Destruimos la jeringa de la mano
        Destroy(gameObject);
    }
}