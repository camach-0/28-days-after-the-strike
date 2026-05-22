using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider2D))]
public class ZonaInfeccion : MonoBehaviour
{
    [Range(0, 100)]
    public float probabilidadEstatico = 30f; // Por defecto, el 30% se quedará quieto

    [Header("Configuración de la Zona")]
    public string etiquetaZombi = "ZombiBase"; // <-- ¡NUEVO! Conectado a la piscina
    public int cantidadZombis = 5;

    // Aquí guardaremos la lista de los zombis que creamos para poder borrarlos después
    private List<GameObject> zombisVivos = new List<GameObject>();
    private bool zonaActiva = false;
    private BoxCollider2D area;
    private bool zonaAgotada = false; // Este es nuestro candado

    private void Start()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true; // Vital: Esto hace que la caja sea "invisible" y no bloquee el paso
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el jugador entra, la zona está apagada, y NUNCA se ha usado antes...
        if (collision.CompareTag("Player") && !zonaActiva && !zonaAgotada)
        {
            ActivarZona();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Si el jugador se va de la zona, la apagamos para limpiar la memoria
        if (collision.CompareTag("Player") && zonaActiva)
        {
            DesactivarZona();
        }
    }

    private void ActivarZona()
    {
        zonaActiva = true;
        zonaAgotada = true; // ¡CERRAMOS EL CANDADO PARA SIEMPRE!

        Debug.Log("Jugador entró a la zona: Generando " + cantidadZombis + " zombis. Zona agotada.");

        for (int i = 0; i < cantidadZombis; i++)
        {
            Vector2 puntoAleatorio = new Vector2(
                Random.Range(area.bounds.min.x, area.bounds.max.x),
                Random.Range(area.bounds.min.y, area.bounds.max.y)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(puntoAleatorio, out hit, 2f, NavMesh.AllAreas))
            {
                // =======================================================
                // ¡LA MAGIA DE LA PISCINA! Pedimos zombis ya creados
                // =======================================================
                GameObject nuevoZombi = PoolManager.Instancia.SolicitarObjeto(etiquetaZombi, hit.position, Quaternion.identity);

                if (nuevoZombi != null) // Validamos que la piscina nos haya dado uno
                {
                    ZombiController cerebro = nuevoZombi.GetComponent<ZombiController>();

                    if (cerebro != null)
                    {
                        cerebro.esDeHorda = false;

                        // EL TRUCO DE VARIEDAD:
                        float suerte = Random.Range(0f, 100f);
                        if (suerte <= probabilidadEstatico)
                        {
                            cerebro.esEstatico = true;
                        }
                        else
                        {
                            cerebro.esEstatico = false;
                        }
                    }
                    zombisVivos.Add(nuevoZombi);
                }
            }
        }
    }

    private void DesactivarZona()
    {
        zonaActiva = false;
        Debug.Log("Jugador salió de la zona: Limpiando zombis NO alertados.");

        // IMPORTANTE: Leemos la lista de atrás hacia adelante (en reversa).
        for (int i = zombisVivos.Count - 1; i >= 0; i--)
        {
            GameObject zombi = zombisVivos[i];

            // Validamos que el zombi exista y siga ENCENDIDO (por si el jugador lo mató dentro de la zona)
            if (zombi != null && zombi.activeInHierarchy)
            {
                ZombiController cerebro = zombi.GetComponent<ZombiController>();

                // Si el zombi NO tiene un objetivo en la mira (es decir, deambula)
                if (cerebro != null && cerebro.objetivoJugador == null)
                {
                    // ¡NUEVO! Lo devolvemos a la piscina para que duerma
                    PoolManager.Instancia.DevolverObjeto(etiquetaZombi, zombi);
                    zombisVivos.RemoveAt(i); // Lo quitamos de la lista
                }
                else
                {
                    // Si el zombi SÍ tiene un objetivo (te está persiguiendo), lo liberamos de la zona.
                    zombisVivos.RemoveAt(i);
                }
            }
            else
            {
                // Si el zombi es nulo o ya estaba apagado (muerto), solo limpiamos la lista
                zombisVivos.RemoveAt(i);
            }
        }
    }
}