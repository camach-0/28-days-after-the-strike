using UnityEngine;
using System.Collections;

// AHORA HEREDA DE ControladorArma (como en tu diagrama)
public class ControladorArmaFuego : ControladorArma
{
    [Header("Configuración del Arma")]
    public DatosArmaFuego datosFuego;
    public GameObject balaPrefab;

    [Header("Estado Actual")]
    public int municionActualCargador;
    public int municionActualReserva;
    public bool estaRecargando = false;

    void Start()
    {
        if (datosFuego != null)
        {
            municionActualCargador = datosFuego.tamanoCargador;
            municionActualReserva = datosFuego.municionMaxima;
        }
    }

    // Sobrescribimos el método del padre
    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (datosFuego == null || estaRecargando) return;

        // AQUÍ SE APLICA LA CADENCIA
        if (Time.time >= tiempoProximoAtaque && municionActualCargador > 0)
        {
            tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
            municionActualCargador--;

            EjecutarDisparo(direccionApuntado);

            if (municionActualCargador <= 0)
            {
                IniciarRecarga();
            }
        }
    }

    void EjecutarDisparo(Vector2 direccionApuntado)
    {
        if (datosFuego.sonidoAtaque != null)
        {
            AudioSource.PlayClipAtPoint(datosFuego.sonidoAtaque, transform.position);
        }

        // Bucle para escopetas (si es pistola, perdigonesPorDisparo será 1)
        for (int i = 0; i < datosFuego.perdigonesPorDisparo; i++)
        {
            // Calculamos el ángulo hacia donde mira el jugador en grados
            float anguloBase = Mathf.Atan2(direccionApuntado.y, direccionApuntado.x) * Mathf.Rad2Deg;

            // Le sumamos la dispersión aleatoria
            float anguloDispersion = Random.Range(-datosFuego.dispersionBalas, datosFuego.dispersionBalas);

            // Creamos la rotación final
            Quaternion rotacionFinalBala = Quaternion.Euler(0, 0, anguloBase + anguloDispersion);

            // Instanciamos
            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, rotacionFinalBala);

            // Extraemos el vector de dirección pura a partir de la rotación
            Vector2 direccionFinal = rotacionFinalBala * Vector2.right;

            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccionFinal);
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

        // Simulamos el tiempo de recarga
        yield return new WaitForSeconds(datosFuego.tiempoRecarga);

        int balasFaltantes = datosFuego.tamanoCargador - municionActualCargador;
        int balasATomar = Mathf.Min(balasFaltantes, municionActualReserva);

        municionActualCargador += balasATomar;
        municionActualReserva -= balasATomar;

        estaRecargando = false;
        Debug.Log("¡Arma recargada!");
    }
}