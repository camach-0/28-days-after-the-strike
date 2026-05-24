using UnityEngine;

public class JugadorCombate : MonoBehaviour
{
    [Header("Arma Actual")]
    public ControladorArma armaEquipada;

    private bool estaDisparando = false;
    private bool disparoAnterior = false; // El filtro para el clic

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
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }

        disparoAnterior = presionado;
    }

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

    // --- ¡NUEVO! Conexión del Cerebro con el Culatazo ---
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