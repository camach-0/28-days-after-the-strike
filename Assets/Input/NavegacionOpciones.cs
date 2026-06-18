using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections; // ¡NUEVO! Vital para poder "esperar un frame"

public class NavegacionOpciones : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;

    [Header("Puntos de Navegación")]
    public GameObject primerElementoOpciones;
    public GameObject botonOpcionesPrincipal;

    public void AbrirPanel()
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);

        // Llamamos a la corrutina para esperar 1 frame
        StartCoroutine(SeleccionarConRetraso(primerElementoOpciones));
    }

    public void CerrarPanel()
    {
        panelOpciones.SetActive(false);
        panelPrincipal.SetActive(true);

        // Llamamos a la corrutina para esperar 1 frame
        StartCoroutine(SeleccionarConRetraso(botonOpcionesPrincipal));
    }

    // Esta es la "magia" que soluciona el bug del mando perdido
    private IEnumerator SeleccionarConRetraso(GameObject botonObjetivo)
    {
        // Le decimos a Unity: Espera al siguiente frame antes de continuar
        yield return null;

        // Ahora que el panel ya está 100% despierto, seleccionamos el botón
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonObjetivo);
    }
}