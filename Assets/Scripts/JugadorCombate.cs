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
            // 1. Armas de Fuego estándar
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                if (!armaFuego.datosFuego.esAutomatica || armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            // 2. M60 (Le permitimos el tiro inicial rápido)
            else if (armaEquipada is ControladorM60)
            {
                armaEquipada.IntentarAtaque(direccionMirando);
            }
            // 3. Resto de armas (Machete, Motosierra, etc.)
            else
            {
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }

        disparoAnterior = presionado;
    }

    public void ProcesarDisparoContinuo(Vector2 direccionMirando)
    {
        if (estaDisparando && armaEquipada != null)
        {
            // 1. Armas Automáticas estándar (Uzi, M16)
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                if (armaFuego.datosFuego.esAutomatica && !armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            // 2. Motosierra (Daño continuo por área)
            else if (armaEquipada is ControladorMotosierra)
            {
                armaEquipada.IntentarAtaque(direccionMirando);
            }
            // 3. ¡NUEVO! Ametralladora Pesada M60 (Fuego automático unificado)
            else if (armaEquipada is ControladorM60)
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