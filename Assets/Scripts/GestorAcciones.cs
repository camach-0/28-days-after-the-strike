using UnityEngine;
using UnityEngine.UI;
using TMPro; // Usamos TextMeshPro para que se vea en alta calidad
using System;
using System.Collections;

public class GestorAcciones : MonoBehaviour
{
    // Patrón Singleton para llamarlo desde cualquier script sin buscarlo
    public static GestorAcciones Instancia;

    [Header("Referencias UI")]
    public GameObject panelAccion;
    public TextMeshProUGUI textoAccion;
    public Image barraRelleno;

    private Coroutine rutinaActual;
    private Action accionPendiente; // Aquí guardamos la curación/resurrección para ejecutarla al final

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        panelAccion.SetActive(false); // Iniciamos con la barra invisible
    }

    // ESTA ES LA FUNCIÓN MÁGICA: Recibe el tiempo, el texto dinámico y lo que hará al final
    public void IniciarAccion(float tiempo, string mensaje, Action accionAlTerminar)
    {
        CancelarAccion(); // Por si había otra acción a medias

        panelAccion.SetActive(true);
        textoAccion.text = mensaje; // ¡Aquí cambiamos el texto dinámicamente!
        barraRelleno.fillAmount = 0f;
        accionPendiente = accionAlTerminar;

        rutinaActual = StartCoroutine(RutinaProgreso(tiempo));
    }

    public void CancelarAccion()
    {
        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            rutinaActual = null;
        }
        panelAccion.SetActive(false);
        accionPendiente = null;
    }

    private IEnumerator RutinaProgreso(float tiempoTotal)
    {
        float tiempoPasado = 0f;

        while (tiempoPasado < tiempoTotal)
        {
            tiempoPasado += Time.deltaTime;
            barraRelleno.fillAmount = tiempoPasado / tiempoTotal;
            yield return null; // Esperamos al siguiente frame
        }

        // ¡Si llegamos aquí, la barra se llenó completa sin ser cancelada!
        panelAccion.SetActive(false);

        if (accionPendiente != null)
        {
            accionPendiente.Invoke(); // Ejecutamos la curación/resurrección
        }
    }
}