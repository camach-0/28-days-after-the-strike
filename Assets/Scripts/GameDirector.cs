using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameDirector : MonoBehaviour
{
    [Header("Configuración del Tank")]
    public string etiquetaTank = "Zombi_Tank";
    public bool tankGenerado = false;
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

    [Header("Efectos de Sonido")]
    [Tooltip("El sonido aterrador cuando llega una horda")]
    public AudioClip sonidoHorda;

    public static GameDirector Instancia;
    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(GenerarHordasDinamicas());
    }

    private IEnumerator GenerarHordasDinamicas()
    {
        yield return new WaitForSeconds(10f);

        while (true)
        {
            int zombisActuales = ZombiController.zombisActivosEnMapa;
            Vector2 centroEquipo = ObtenerCentroDelEquipo();

            if (centroEquipo != Vector2.zero && zombisActuales < limiteZombisEnMapa)
            {
                int tamanoHorda = Random.Range(minZombisPorHorda, maxZombisPorHorda);
                int cantidadGrupos = Random.Range(minGrupos, maxGrupos + 1);
                int zombisPorGrupo = tamanoHorda / cantidadGrupos;

                Debug.Log($"¡HORDA DETECTADA! {tamanoHorda} zombis atacando desde {cantidadGrupos} direcciones.");

                // Reproducimos el sonido global en la cámara para asegurarnos de que todos lo escuchen
                if (sonidoHorda != null && Camera.main != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoHorda, Camera.main.transform.position, 1f);
                }

                for (int i = 0; i < cantidadGrupos; i++)
                {
                    Vector2 direccionAtaque = Random.insideUnitCircle.normalized;
                    Vector2 puntoGeneracionBase = centroEquipo + (direccionAtaque * distanciaDeAparicion);

                    for (int j = 0; j < zombisPorGrupo; j++)
                    {
                        Vector3 posicionDeseada = (Vector3)puntoGeneracionBase + (Vector3)(Random.insideUnitCircle * 3f);

                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(posicionDeseada, out hit, 4f, NavMesh.AllAreas))
                        {
                            GameObject nuevoZombi = PoolManager.Instancia.SolicitarObjeto(etiquetaZombi, hit.position, Quaternion.identity);

                            if (nuevoZombi != null)
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

    public void DesatarHordaPorVomito(Transform victima)
    {
        Debug.Log($"<color=red>¡EL DIRECTOR SE ENFADA! ¡Enviando horda masiva hacia {victima.name}!</color>");

        if (sonidoHorda != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(sonidoHorda, Camera.main.transform.position, 1f);
        }

        StopAllCoroutines();
        StartCoroutine(RutinaHordaDirigida(victima));
        StartCoroutine(GenerarHordasDinamicas());
    }

    private IEnumerator RutinaHordaDirigida(Transform victima)
    {
        if (victima == null) yield break;

        int tamanoHorda = Random.Range(maxZombisPorHorda, maxZombisPorHorda + 15);
        int cantidadGrupos = 4;
        int zombisPorGrupo = tamanoHorda / cantidadGrupos;

        for (int i = 0; i < cantidadGrupos; i++)
        {
            Vector2 direccionAtaque = Random.insideUnitCircle.normalized;
            Vector2 puntoGeneracionBase = (Vector2)victima.position + (direccionAtaque * distanciaDeAparicion);

            for (int j = 0; j < zombisPorGrupo; j++)
            {
                Vector3 posicionDeseada = (Vector3)puntoGeneracionBase + (Vector3)(Random.insideUnitCircle * 3f);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(posicionDeseada, out hit, 4f, NavMesh.AllAreas))
                {
                    GameObject nuevoZombi = PoolManager.Instancia.SolicitarObjeto(etiquetaZombi, hit.position, Quaternion.identity);

                    if (nuevoZombi != null)
                    {
                        ZombiController cerebro = nuevoZombi.GetComponent<ZombiController>();
                        if (cerebro != null)
                        {
                            cerebro.esDeHorda = true;
                            cerebro.objetivoJugador = victima;
                        }
                    }
                }
            }
        }
        yield return null;
    }
    public void GenerarTank(Vector2 posicionAparicion)
    {
        if (!tankGenerado)
        {
            tankGenerado = true;
            Debug.Log("<color=red>¡EL DIRECTOR HA SOLTADO AL TANK!</color>");
            PoolManager.Instancia.SolicitarObjeto(etiquetaTank, posicionAparicion, Quaternion.identity);
        }
    }
}