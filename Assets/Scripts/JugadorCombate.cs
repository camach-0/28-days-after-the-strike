using UnityEngine;

public class JugadorCombate : MonoBehaviour
{
    [Header("Arma Actual")]
    public ControladorArma armaEquipada;

    private bool estaDisparando = false;
    private bool disparoAnterior = false;

    public void ProcesarInputDisparo(bool presionado, Vector2 direccionMirando)
    {
        bool esPrimerToque = presionado && !disparoAnterior;
        estaDisparando = presionado;

        if (esPrimerToque && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                if (!armaFuego.datosFuego.esAutomatica || armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            else
            {
                // Aquí entran el Machete, Botiquín y el primer toque de la Motosierra
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }

        disparoAnterior = presionado;
    }

    public void ProcesarDisparoContinuo(Vector2 direccionMirando)
    {
        if (estaDisparando && armaEquipada != null)
        {
            // 1. Armas Automáticas (Uzi, M16)
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                if (armaFuego.datosFuego.esAutomatica && !armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            // ========================================================
            // ¡EL PARCHE AQUÍ! 
            // 2. Si es una Motosierra, le pasamos la señal continua
            // ========================================================
            else if (armaEquipada is ControladorMotosierra)
            {
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }
    }

    public void ProcesarInputEmpujon(Vector2 direccionMirando)
    {
        if (armaEquipada != null)
        {
            armaEquipada.IntentarEmpujon(direccionMirando);
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