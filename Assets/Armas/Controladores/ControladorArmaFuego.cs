using UnityEngine;
using System.Collections;

public class ControladorArmaFuego : MonoBehaviour
{
    [Header("Configuración del Arma")]
    public DatosArmaFuego datosFuego;
    public Transform puntoDisparo;
    public GameObject balaPrefab; // AÑADIDO: Para conectar nuestra Bala actual

    [Header("Estado Actual")]
    public int municionActualCargador;
    public int municionActualReserva;
    public bool estaRecargando = false;
    private float tiempoProximoAtaque = 0f;

    void Start()
    {
        if (datosFuego != null)
        {
            municionActualCargador = datosFuego.tamanoCargador;
            municionActualReserva = datosFuego.municionMaxima;
        }
    }

    // ELIMINAMOS EL UPDATE CON INPUTS. 
    // Ahora el arma es "tonta" y espera a que el JugadorController le dé la orden.

    public void ApretarGatillo()
    {
        if (datosFuego == null || estaRecargando) return;

        if (Time.time >= tiempoProximoAtaque && municionActualCargador > 0)
        {
            tiempoProximoAtaque = Time.time + datosFuego.cadenciaAtaque;
            municionActualCargador--;

            EjecutarDisparo();

            if (municionActualCargador <= 0)
            {
                IniciarRecarga();
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

    void EjecutarDisparo()
    {
        // Reproducir sonido si lo hay
        if (datosFuego.sonidoAtaque != null)
        {
            AudioSource.PlayClipAtPoint(datosFuego.sonidoAtaque, transform.position);
        }

        // Lógica de disparo (Soporta múltiples perdigones como la Escopeta de tu compañero)
        for (int i = 0; i < datosFuego.perdigonesPorDisparo; i++)
        {
            // Calculamos la dispersión de la bala que tu compañero configuró
            float anguloDispersion = Random.Range(-datosFuego.dispersionBalas, datosFuego.dispersionBalas);
            Quaternion rotacionBala = puntoDisparo.rotation * Quaternion.Euler(0, 0, anguloDispersion);

            // Creamos nuestra bala
            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, rotacionBala);

            // Calculamos la dirección con el ángulo de dispersión
            Vector2 direccion = rotacionBala * Vector2.right; // Asumiendo que el puntoDisparo mira a la derecha localmente

            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccion);

            // Le pasamos el daño base del ScriptableObject a nuestra bala
            nuevaBala.GetComponent<Bala>().dano = (int)datosFuego.danoBase;
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