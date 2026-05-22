using UnityEngine;

public class JugadorCombate : MonoBehaviour
{
    [Header("Arma Actual")]
    public ControladorArma armaEquipada;

    private bool estaDisparando = false;

    // Llamado cuando el jugador presiona o suelta el botón de disparo
    public void ProcesarInputDisparo(bool presionado, Vector2 direccionMirando)
    {
        estaDisparando = presionado;

        if (estaDisparando && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                // Disparo Semiautomático o Ráfaga
                if (!armaFuego.datosFuego.esAutomatica || armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            else
            {
                // Armas Melee o Consumibles
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }
    }

    // Llamado constantemente por el Cerebro para armas como la Uzi o M16
    public void ProcesarDisparoContinuo(Vector2 direccionMirando)
    {
        if (estaDisparando && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                if (armaFuego.datosFuego.esAutomatica && !armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
        }
    }

    public void IntentarRecarga()
    {
        if (armaEquipada != null && armaEquipada is ControladorArmaFuego armaFuego)
        {
            armaFuego.IniciarRecarga();
        }
    }
}