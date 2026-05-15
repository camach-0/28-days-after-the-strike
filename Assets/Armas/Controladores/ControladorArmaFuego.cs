using UnityEngine;
using System.Collections;

public class ControladorArmaFuego : ControladorArma
{
    [Header("Configuración del Arma")]
    public DatosArmaFuego datosFuego;
    public GameObject balaPrefab;

    [Header("Estado Actual")]
    public int municionActualCargador;
    public int municionActualReserva;
    public bool estaRecargando = false;

    // NUEVO: Bloquea el arma mientras dispara una ráfaga
    private bool estaDisparandoRafaga = false;

    void Start()
    {
        if (datosFuego != null)
        {
            municionActualCargador = datosFuego.tamanoCargador;
            municionActualReserva = datosFuego.municionMaxima;
        }
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        // Si no hay datos, está recargando, o está a mitad de una ráfaga, no hacemos nada
        if (datosFuego == null || estaRecargando || estaDisparandoRafaga) return;

        // Comprobamos la cadencia normal
        if (Time.time >= tiempoProximoAtaque && municionActualCargador > 0)
        {
            if (datosFuego.esRafaga)
            {
                // DISPARO DE RÁFAGA (SCAR)
                StartCoroutine(CorrutinaRafaga(direccionApuntado));
            }
            else
            {
                // DISPARO NORMAL (Auto / Semiauto / Escopeta)
                tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
                GastarBalaYDisparar(direccionApuntado);
            }
        }
    }

    // Separo esta lógica para no repetir código
    private void GastarBalaYDisparar(Vector2 direccionApuntado)
    {
        municionActualCargador--;
        EjecutarDisparo(direccionApuntado);

        if (municionActualCargador <= 0)
        {
            IniciarRecarga();
        }
    }

    // LA MAGIA DE LA RÁFAGA
    IEnumerator CorrutinaRafaga(Vector2 direccionApuntado)
    {
        estaDisparandoRafaga = true;

        for (int i = 0; i < datosFuego.balasPorRafaga; i++)
        {
            if (municionActualCargador <= 0) break; // Corta la ráfaga si te quedas sin balas

            GastarBalaYDisparar(direccionApuntado);

            // Pausa minúscula entre las balas de la ráfaga
            if (municionActualCargador > 0 && i < datosFuego.balasPorRafaga - 1)
            {
                yield return new WaitForSeconds(datosFuego.tiempoEntreBalasRafaga);
            }
        }

        // Aplicamos el cooldown general DESPUÉS de que termina la ráfaga
        tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
        estaDisparandoRafaga = false;
    }

    void EjecutarDisparo(Vector2 direccionApuntado)
    {
        if (datosFuego.sonidoAtaque != null)
        {
            AudioSource.PlayClipAtPoint(datosFuego.sonidoAtaque, transform.position);
        }

        for (int i = 0; i < datosFuego.perdigonesPorDisparo; i++)
        {
            float anguloBase = Mathf.Atan2(direccionApuntado.y, direccionApuntado.x) * Mathf.Rad2Deg;
            float anguloDispersion = Random.Range(-datosFuego.dispersionBalas, datosFuego.dispersionBalas);
            Quaternion rotacionFinalBala = Quaternion.Euler(0, 0, anguloBase + anguloDispersion);

            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, rotacionFinalBala);
            Vector2 direccionFinal = rotacionFinalBala * Vector2.right;

            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccionFinal);

            // Asumo que danoBase viene de la clase base DatosArma
            nuevaBala.GetComponent<Bala>().dano = (int)datosFuego.danoBase;
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
        Debug.Log("Recargando...");

        yield return new WaitForSeconds(datosFuego.tiempoRecarga);

        int balasFaltantes = datosFuego.tamanoCargador - municionActualCargador;
        int balasATomar = Mathf.Min(balasFaltantes, municionActualReserva);

        municionActualCargador += balasATomar;
        municionActualReserva -= balasATomar;

        estaRecargando = false;
        Debug.Log("¡Arma recargada!");
    }
}