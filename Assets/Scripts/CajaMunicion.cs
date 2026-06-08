using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CajaMunicion : MonoBehaviour
{
    [Header("Feedback Técnico")]
    public AudioClip sonidoRecarga;

    // Variables internas para rastrear al jugador sin depender de las físicas
    private bool jugadorEnZona = false;
    private InventarioJugador inventarioDetectado;
    private JugadorInput inputDetectado;

    // 1. Cuando el jugador TOCA la zona
    private void OnTriggerEnter2D(Collider2D colision)
    {
        if (colision.CompareTag("Player"))
        {
            jugadorEnZona = true;
            inventarioDetectado = colision.GetComponent<InventarioJugador>();
            inputDetectado = colision.GetComponent<JugadorInput>();
            Debug.Log("<color=yellow>MUNICIÓN:</color> Jugador entró en la zona de recarga.");
        }
    }

    // 2. Cuando el jugador SALE de la zona
    private void OnTriggerExit2D(Collider2D colision)
    {
        if (colision.CompareTag("Player"))
        {
            jugadorEnZona = false;
            inventarioDetectado = null;
            inputDetectado = null;
            Debug.Log("<color=yellow>MUNICIÓN:</color> Jugador salió de la zona.");
        }
    }

    // 3. El Input corre a la velocidad de la luz en el Update
    private void Update()
    {
        if (jugadorEnZona && inputDetectado != null && inputDetectado.IntentoInteractuar)
        {
            Debug.Log("<color=green>MUNICIÓN:</color> Botón apretado. Intentando recargar...");
            ProcesarRecarga(inventarioDetectado);

            // Consumimos el input para no recargar 50 veces en un segundo
            inputDetectado.IntentoInteractuar = false;
        }
    }

    // 4. El Algoritmo Matemático
    private void ProcesarRecarga(InventarioJugador inventario)
    {
        bool recargaExitosa = false;

        foreach (ControladorArma arma in inventario.ranuras)
        {
            if (arma is ControladorArmaFuego armaFuego)
            {
                if (armaFuego.datosFuego != null && !armaFuego.datosFuego.reservaInfinita)
                {
                    if (armaFuego.municionActualReserva < armaFuego.datosFuego.municionMaxima)
                    {
                        armaFuego.municionActualReserva = armaFuego.datosFuego.municionMaxima;
                        recargaExitosa = true;
                        Debug.Log($"<color=cyan>SISTEMA MUNICIÓN:</color> Arma recargada al máximo ({armaFuego.datosFuego.municionMaxima}).");
                    }
                    else
                    {
                        Debug.Log("<color=grey>SISTEMA MUNICIÓN:</color> El arma ya tiene la reserva al tope.");
                    }
                }
            }
        }

        if (recargaExitosa)
        {
            if (sonidoRecarga != null)
            {
                GameObject objSonido = PoolManager.Instancia.SolicitarObjeto("EfectoSonido", transform.position, Quaternion.identity);
                if (objSonido != null) objSonido.GetComponent<AudioReciclable>().Reproducir(sonidoRecarga);
            }
        }
    }
}