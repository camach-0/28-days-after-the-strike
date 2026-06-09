using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class GestorAcciones : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelAccion;
    public TextMeshProUGUI textoAccion;
    public Image barraRelleno;

    private Coroutine rutinaActual;
    private Action accionPendiente;

    private void Awake()
    {
        if (panelAccion != null) panelAccion.SetActive(false);
    }

    // Le agregamos "= null" para que los aliados puedan usar la barra solo de forma visual (sin ejecutar código doble)
    public void IniciarAccion(float tiempo, string mensaje, Action accionAlTerminar = null)
    {
        CancelarAccion();

        if (panelAccion != null) panelAccion.SetActive(true);
        if (textoAccion != null) textoAccion.text = mensaje;
        if (barraRelleno != null) barraRelleno.fillAmount = 0f;

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
        if (panelAccion != null) panelAccion.SetActive(false);
        accionPendiente = null;
    }

    private IEnumerator RutinaProgreso(float tiempoTotal)
    {
        float tiempoPasado = 0f;

        while (tiempoPasado < tiempoTotal)
        {
            tiempoPasado += Time.deltaTime;
            if (barraRelleno != null) barraRelleno.fillAmount = tiempoPasado / tiempoTotal;
            yield return null;
        }

        if (panelAccion != null) panelAccion.SetActive(false);

        if (accionPendiente != null)
        {
            // Limpiamos antes de ejecutar
            Action accionFinal = accionPendiente;
            accionPendiente = null;
            accionFinal.Invoke();
        }
    }
}