using UnityEngine;
using System.Collections;

public class ControladorArmaFuego : ControladorArma
{
    [Header("Configuración del Arma")]
    public DatosArmaFuego datosFuego;

    [Tooltip("Debe ser el mismo nombre que pusiste en la Etiqueta del PoolManager")]
    public string etiquetaBala = "BalaBase";

    [Header("Estado Actual")]
    public int municionActualCargador;
    public int municionActualReserva;
    public bool estaRecargando = false;

    private bool estaDisparandoRafaga = false;

    void Start()
    {
        if (datosFuego != null)
        {
            municionActualCargador = datosFuego.tamanoCargador;
            municionActualReserva = datosFuego.municionMaxima;
        }
    }

    private void OnDisable()
    {
        estaRecargando = false;
        estaDisparandoRafaga = false;
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (datosFuego == null || estaRecargando || estaDisparandoRafaga) return;

        if (Time.time >= tiempoProximoAtaque && municionActualCargador > 0)
        {
            if (datosFuego.esRafaga)
            {
                StartCoroutine(CorrutinaRafaga(direccionApuntado));
            }
            else
            {
                tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
                GastarBalaYDisparar(direccionApuntado);
            }
        }
    }

    private void GastarBalaYDisparar(Vector2 direccionApuntado)
    {
        municionActualCargador--;
        EjecutarDisparo(direccionApuntado);

        if (municionActualCargador <= 0) IniciarRecarga();
    }

    IEnumerator CorrutinaRafaga(Vector2 direccionApuntado)
    {
        estaDisparandoRafaga = true;

        for (int i = 0; i < datosFuego.balasPorRafaga; i++)
        {
            if (municionActualCargador <= 0) break;

            GastarBalaYDisparar(direccionApuntado);

            if (municionActualCargador > 0 && i < datosFuego.balasPorRafaga - 1)
            {
                yield return new WaitForSeconds(datosFuego.tiempoEntreBalasRafaga);
            }
        }

        tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
        estaDisparandoRafaga = false;
    }

    void EjecutarDisparo(Vector2 direccionApuntado)
    {
        if (datosFuego.sonidoAtaque != null) AudioSource.PlayClipAtPoint(datosFuego.sonidoAtaque, transform.position);

        for (int i = 0; i < datosFuego.perdigonesPorDisparo; i++)
        {
            float anguloBase = Mathf.Atan2(direccionApuntado.y, direccionApuntado.x) * Mathf.Rad2Deg;
            float anguloDispersion = Random.Range(-datosFuego.dispersionBalas, datosFuego.dispersionBalas);
            Quaternion rotacionFinalBala = Quaternion.Euler(0, 0, anguloBase + anguloDispersion);

            // ==========================================
            // ¡LA MAGIA DE LA PISCINA! Adiós Instantiate.
            // ==========================================
            GameObject nuevaBala = PoolManager.Instancia.SolicitarObjeto(etiquetaBala, puntoDisparo.position, rotacionFinalBala);

            if (nuevaBala != null)
            {
                Vector2 direccionFinal = rotacionFinalBala * Vector2.right;
                Bala scriptBala = nuevaBala.GetComponent<Bala>();

                scriptBala.ConfigurarDireccion(direccionFinal);
                scriptBala.dano = (int)datosFuego.danoBase;
            }
        }
    }

    public void IniciarRecarga()
    {
        if (!estaRecargando && municionActualCargador < datosFuego.tamanoCargador && municionActualReserva > 0)
        {
            StartCoroutine(CorrutinaRecarga());
        }
    }

    IEnumerator CorrutinaRecarga()
    {
        estaRecargando = true;
        yield return new WaitForSeconds(datosFuego.tiempoRecarga);

        int balasFaltantes = datosFuego.tamanoCargador - municionActualCargador;
        int balasATomar = Mathf.Min(balasFaltantes, municionActualReserva);

        municionActualCargador += balasATomar;
        municionActualReserva -= balasATomar;

        estaRecargando = false;
    }
}