using UnityEngine;

public class JugadorCombate : MonoBehaviour
{
    [Header("Arma Actual")]
    public ControladorArma armaEquipada;

    private bool estaDisparando = false;
    private bool disparoAnterior = false; // <-- ¡NUEVA VARIABLE DE MEMORIA!

    // Llamado constantemente por el Cerebro
    public void ProcesarInputDisparo(bool presionado, Vector2 direccionMirando)
    {
        // Solo consideramos que es un "clic" real si AHORA está presionado, pero el fotograma pasado NO lo estaba
        bool esPrimerToque = presionado && !disparoAnterior;
        estaDisparando = presionado; // Guardamos el estado para que el disparo continuo lo lea

        if (esPrimerToque && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                // Disparo Semiautomático o Ráfaga (Solo ocurre una vez por clic)
                if (!armaFuego.datosFuego.esAutomatica || armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            else
            {
                // Armas Melee o Consumibles (Una vez por clic)
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }

        // ¡CLAVE! Actualizamos nuestra memoria para el siguiente fotograma
        disparoAnterior = presionado;
    }

    // Llamado constantemente por el Cerebro para armas automáticas (Uzi, M16)
    public void ProcesarDisparoContinuo(Vector2 direccionMirando)
    {
        if (estaDisparando && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                // Si es automática, permitimos que dispare sin soltar el botón
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