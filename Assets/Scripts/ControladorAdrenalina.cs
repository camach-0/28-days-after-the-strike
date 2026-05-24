using UnityEngine;

public class ControladorAdrenalina : ControladorArma
{
    [Header("Efecto de Adrenalina")]
    public float multiplicadorVelocidad = 1.5f; // Te hace 50% más rápido (1.5x)
    public float tiempoEfecto = 10f; // Dura 10 segundos

    private bool seEstaUsando = false;

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (seEstaUsando) return;
        Inyectar();
    }

    private void Inyectar()
    {
        seEstaUsando = true;

        // 1. Buscamos el movimiento y le mandamos el efecto de velocidad
        JugadorMovimiento movimiento = GetComponentInParent<JugadorMovimiento>();
        if (movimiento != null)
        {
            movimiento.InyectarAdrenalina(multiplicadorVelocidad, tiempoEfecto);
        }

        // 2. Sacamos la jeringa del inventario de Jorge
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

            // Cambiamos automáticamente al arma primaria o secundaria
            if (miInventario.ranuras[0] != null) miInventario.CambiarSlot(0);
            else miInventario.CambiarSlot(1);
        }

        // 3. Destruimos la jeringa de la mano inmediatamente
        Destroy(gameObject);
    }
}