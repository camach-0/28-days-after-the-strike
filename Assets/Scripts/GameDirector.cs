using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GameDirector : MonoBehaviour
{
    [Header("Configuración de la Horda")]
    public GameObject zombiPrefab;
    public Transform jugador; // El Director necesita saber dónde estás

    [Header("Generación Dinámica")]
    [Tooltip("A qué distancia del jugador nacen (pon un número mayor al de la cámara)")]
    public float distanciaDeAparicion = 15f;

    [Header("Ritmo de Juego")]
    public float tiempoPrimeraOleada = 5f;
    public float tiempoEntreOleadas = 15f;
    public int zombisPorOleada = 4;

    private void Start()
    {
        // Si no asignaste al jugador en el Inspector, lo busca automáticamente
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player").transform;
        }

        StartCoroutine(GenerarHordasDinamicas());
    }

    private IEnumerator GenerarHordasDinamicas()
    {
        yield return new WaitForSeconds(tiempoPrimeraOleada);

        while (true)
        {
            if (jugador != null && zombiPrefab != null)
            {
                Debug.Log("¡Horda invocada! Nacen fuera de la pantalla y corren hacia ti.");

                for (int i = 0; i < zombisPorOleada; i++)
                {
                    // 1. Calculamos un punto en un círculo invisible alrededor del jugador
                    Vector2 direccionAleatoria = Random.insideUnitCircle.normalized;
                    Vector2 puntoTeorico = (Vector2)jugador.position + (direccionAleatoria * distanciaDeAparicion);

                    // 2. Le preguntamos al NavMesh si ese punto exacto es pisable (no es una pared)
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(puntoTeorico, out hit, 5f, NavMesh.AllAreas))
                    {
                        // 3. Si es pisable, lo creamos ahí
                        GameObject nuevoZombi = Instantiate(zombiPrefab, hit.position, Quaternion.identity);

                        ZombiController cerebro = nuevoZombi.GetComponent<ZombiController>();
                        if (cerebro != null)
                        {
                            cerebro.esDeHorda = true; // Arranca corriendo
                        }
                    }
                }
            }

            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }
}