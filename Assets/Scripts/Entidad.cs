using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Entidad : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float velocidadMovimiento = 5f;
    public Image barraDeVidaUI;
    public bool estaMuerto = false;

    [Header("Feedback Visual y Equilibrio (I-Frames)")]
    public float tiempoInvulnerabilidad = 0.4f;
    private float tiempoUltimoGolpe = -100f;

    [SerializeField] private float duracionFlashRojo = 0.15f;
    private SpriteRenderer sr;
    private Color colorOriginal;

    private bool inicializado = false; // ¡NUEVO! Control para bots

    // NUEVO: Método que asegura que la vida se cargue siempre
    private void InicializarSeguro()
    {
        if (inicializado) return; // Si ya se configuró, no hace nada

        vidaActual = vidaMaxima;
        estaMuerto = false;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) colorOriginal = sr.color;

        inicializado = true;
    }

    public virtual void Start()
    {
        InicializarSeguro(); // Los humanos se inicializan aquí
    }

    public virtual void RecibirDano(float cantidad)
    {
        InicializarSeguro(); // ¡TRUCO! Los bots se inicializan justo antes de recibir el golpe

        if (estaMuerto) return;

        if (Time.time < tiempoUltimoGolpe + tiempoInvulnerabilidad)
        {
            return;
        }

        tiempoUltimoGolpe = Time.time;
        vidaActual -= cantidad;

        if (barraDeVidaUI != null)
        {
            barraDeVidaUI.fillAmount = vidaActual / vidaMaxima;
        }

        if (sr != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(FlashRojoDano());
        }

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Morir();
        }
    }

    private IEnumerator FlashRojoDano()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(duracionFlashRojo);

        if (!estaMuerto && sr != null)
        {
            sr.color = colorOriginal;
        }
    }

    public virtual void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;
    }
}