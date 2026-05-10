using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider2D))]
public class ZonaInfeccion : MonoBehaviour
{
    [Range(0, 100)]
    public float probabilidadEstatico = 30f; // Por defecto, el 30% se quedará quieto
    [Header("Configuración de la Zona")]
    public GameObject zombiPrefab;
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
                GameObject nuevoZombi = Instantiate(zombiPrefab, hit.position, Quaternion.identity);
                ZombiController cerebro = nuevoZombi.GetComponent<ZombiController>();

                if (cerebro != null)
                {
                    cerebro.esDeHorda = false;

                    // EL TRUCO DE VARIEDAD:
                    // Si el número aleatorio es menor a nuestra probabilidad, se queda quieto.
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

    private void DesactivarZona()
    {
        zonaActiva = false;
        Debug.Log("Jugador salió de la zona: Limpiando zombis NO alertados.");

        // IMPORTANTE: Leemos la lista de atrás hacia adelante (en reversa).
        // En programación, si vas a borrar cosas de una lista, siempre se hace en reversa 
        // para que los números de orden no se recorran y causen errores.
        for (int i = zombisVivos.Count - 1; i >= 0; i--)
        {
            GameObject zombi = zombisVivos[i];

            if (zombi != null) // Si el zombi sigue vivo (no le has disparado)
            {
                ZombiController cerebro = zombi.GetComponent<ZombiController>();

                // Si tiene cerebro y NO te está persiguiendo...
                if (cerebro != null && cerebro.estadoActual != ZombiController.EstadoZombi.Persiguiendo)
                {
                    Destroy(zombi); // Lo borramos para liberar memoria
                    zombisVivos.RemoveAt(i); // Lo quitamos de la lista
                }
                else
                {
                    // Si el zombi SÍ te está persiguiendo, lo quitamos de la lista de esta zona
                    // para "liberarlo". Ahora es independiente y no será borrado.
                    zombisVivos.RemoveAt(i);
                }
            }
            else
            {
                // Si el zombi ya estaba muerto (nulo), solo limpiamos la lista
                zombisVivos.RemoveAt(i);
            }
        }
    }
}