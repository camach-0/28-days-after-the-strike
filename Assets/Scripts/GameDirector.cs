using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameDirector : MonoBehaviour
{
    [Header("Configuración de la Horda")]
    public string etiquetaZombi = "ZombiBase";
    public float distanciaDeAparicion = 20f;

    [Header("Límites y Memoria")]
    public int limiteZombisEnMapa = 60;

    [Header("Tamaño de la Horda")]
    public int minZombisPorHorda = 20;
    public int maxZombisPorHorda = 35;
    public int minGrupos = 2;
    public int maxGrupos = 4;

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
        yield return new WaitForSeconds(10f);

        while (true)
        {
            // NOTA: Asegúrate de que tus Zombis tengan la etiqueta (Tag) "Enemy" en Unity.
            int zombisActuales = ZombiController.zombisActivosEnMapa;
            Vector2 centroEquipo = ObtenerCentroDelEquipo();

            // ¡CORREGIDO! Ya no verificamos el viejo zombiPrefab, el PoolManager se encarga
            if (centroEquipo != Vector2.zero && zombisActuales < limiteZombisEnMapa)
            {
                int tamanoHorda = Random.Range(minZombisPorHorda, maxZombisPorHorda);
                int cantidadGrupos = Random.Range(minGrupos, maxGrupos + 1);
                int zombisPorGrupo = tamanoHorda / cantidadGrupos;

                Debug.Log($"¡HORDA DETECTADA! {tamanoHorda} zombis atacando desde {cantidadGrupos} direcciones.");

                for (int i = 0; i < cantidadGrupos; i++)
                {
                    Vector2 direccionAtaque = Random.insideUnitCircle.normalized;
                    Vector2 puntoGeneracion = centroEquipo + (direccionAtaque * distanciaDeAparicion);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(puntoGeneracion, out hit, 10f, NavMesh.AllAreas))
                    {
                        for (int j = 0; j < zombisPorGrupo; j++)
                        {
                            Vector3 posicionDesfasada = hit.position + (Vector3)(Random.insideUnitCircle * 2f);

                            // =======================================================
                            // ¡LA MAGIA DE LA PISCINA! Pedimos zombis ya creados
                            // =======================================================
                            GameObject nuevoZombi = PoolManager.Instancia.SolicitarObjeto(etiquetaZombi, posicionDesfasada, Quaternion.identity);

                            if (nuevoZombi != null) // Validamos que la piscina nos haya dado uno
                            {
                                ZombiController cerebro = nuevoZombi.GetComponent<ZombiController>();

                                if (cerebro != null)
                                {
                                    cerebro.esDeHorda = true;
                                }
                            }
                        }
                    }
                }
            }

            float tiempoDeCalma = CalcularTiempoDePaz();
            yield return new WaitForSeconds(tiempoDeCalma);
        }
    }

    // --- NUEVO: Calcula el punto medio consultando la lista ultra-rápida del GameManager ---
    private Vector2 ObtenerCentroDelEquipo()
    {
        if (GameManager.Instancia == null || GameManager.Instancia.supervivientesActivos.Count == 0) return Vector2.zero;

        Vector2 sumaPosiciones = Vector2.zero;
        int vivos = 0;

        foreach (SistemaSalud vida in GameManager.Instancia.supervivientesActivos)
        {
            if (vida != null && vida.vidaActual > 0)
            {
                sumaPosiciones += (Vector2)vida.transform.position;
                vivos++;
            }
        }

        if (vivos == 0) return Vector2.zero;
        return sumaPosiciones / vivos;
    }

    private float CalcularTiempoDePaz()
    {
        if (GameManager.Instancia == null || GameManager.Instancia.supervivientesActivos.Count == 0) return 10f;

        float vidaActualTotal = 0f, vidaMaximaTotal = 0f;
        int jugadoresVivos = 0;

        foreach (SistemaSalud vidaJugador in GameManager.Instancia.supervivientesActivos)
        {
            if (vidaJugador != null && vidaJugador.vidaActual > 0)
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