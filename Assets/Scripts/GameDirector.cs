using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameDirector : MonoBehaviour
{
    [Header("Configuración de la Horda")]
    public GameObject zombiPrefab;
    public float distanciaDeAparicion = 20f; // Distancia desde el centro del equipo

    [Header("Límites y Memoria")]
    public int limiteZombisEnMapa = 60; // Subimos el límite para soportar hordas masivas

    [Header("Tamaño de la Horda")]
    public int minZombisPorHorda = 20;
    public int maxZombisPorHorda = 35;
    public int minGrupos = 2; // Desde cuántos frentes atacan (ej. frente y espalda)
    public int maxGrupos = 4; // Rodearlos por 4 lados

    [Header("Sistema de Estrés (Tiempos de Paz)")]
    public float tiempoPaz_SaludAlta = 20f;
    public float tiempoPaz_SaludMedia = 35f;
    public float tiempoPaz_SaludBaja = 50f;

    private void Start()
    {
        StartCoroutine(GenerarHordasDinamicas());
    }

    private IEnumerator GenerarHordasDinamicas()
    {
        yield return new WaitForSeconds(10f); // Tiempo inicial

        while (true)
        {
            int zombisActuales = GameObject.FindGameObjectsWithTag("Enemy").Length;
            Vector2 centroEquipo = ObtenerCentroDelEquipo();

            // Si hay jugadores vivos y la memoria lo permite...
            if (centroEquipo != Vector2.zero && zombiPrefab != null && zombisActuales < limiteZombisEnMapa)
            {
                int tamanoHorda = Random.Range(minZombisPorHorda, maxZombisPorHorda);
                int cantidadGrupos = Random.Range(minGrupos, maxGrupos + 1);
                int zombisPorGrupo = tamanoHorda / cantidadGrupos;

                Debug.Log($"¡HORDA DETECTADA! {tamanoHorda} zombis atacando desde {cantidadGrupos} direcciones.");

                for (int i = 0; i < cantidadGrupos; i++)
                {
                    // Elegimos una dirección al azar para este grupo (ej. Norte, Sur, Este...)
                    Vector2 direccionAtaque = Random.insideUnitCircle.normalized;
                    Vector2 puntoGeneracion = centroEquipo + (direccionAtaque * distanciaDeAparicion);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(puntoGeneracion, out hit, 10f, NavMesh.AllAreas))
                    {
                        // Generamos el sub-grupo en este punto
                        for (int j = 0; j < zombisPorGrupo; j++)
                        {
                            // Les damos un poco de separación para que no nazcan uno encima del otro
                            Vector3 posicionDesfasada = hit.position + (Vector3)(Random.insideUnitCircle * 2f);

                            GameObject nuevoZombi = Instantiate(zombiPrefab, posicionDesfasada, Quaternion.identity);
                            ZombiController cerebro = nuevoZombi.GetComponent<ZombiController>();

                            if (cerebro != null)
                            {
                                cerebro.esDeHorda = true;
                            }
                        }
                    }
                }
            }

            // Calculamos el tiempo de paz para la siguiente oleada basándonos en la vida
            float tiempoDeCalma = CalcularTiempoDePaz();
            yield return new WaitForSeconds(tiempoDeCalma);
        }
    }

    // --- NUEVO: Calcula el punto medio entre todos los jugadores vivos ---
    private Vector2 ObtenerCentroDelEquipo()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        Vector2 sumaPosiciones = Vector2.zero;
        int vivos = 0;

        foreach (GameObject jug in jugadores)
        {
            Entidad vida = jug.GetComponent<Entidad>();
            if (vida != null && !vida.estaMuerto)
            {
                sumaPosiciones += (Vector2)jug.transform.position;
                vivos++;
            }
        }

        if (vivos == 0) return Vector2.zero;
        return sumaPosiciones / vivos; // Retorna el centro geométrico del equipo
    }

    private float CalcularTiempoDePaz()
    {
        GameObject[] todosLosJugadores = GameObject.FindGameObjectsWithTag("Player");
        float vidaActualTotal = 0f, vidaMaximaTotal = 0f;
        int jugadoresVivos = 0;

        foreach (GameObject jugador in todosLosJugadores)
        {
            Entidad vidaJugador = jugador.GetComponent<Entidad>();
            if (vidaJugador != null && !vidaJugador.estaMuerto)
            {
                vidaActualTotal += vidaJugador.vidaActual;
                vidaMaximaTotal += vidaJugador.vidaMaxima;
                jugadoresVivos++;
            }
        }

        if (jugadoresVivos == 0 || vidaMaximaTotal == 0) return 10f;

        float porcentaje = vidaActualTotal / vidaMaximaTotal;
        if (porcentaje >= 0.7f) return tiempoPaz_SaludAlta;
        if (porcentaje >= 0.3f) return tiempoPaz_SaludMedia;
        return tiempoPaz_SaludBaja;
    }
}