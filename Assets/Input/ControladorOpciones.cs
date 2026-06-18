using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Vital para controlar el mando en sub-menús

public class ControladorOpciones : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;

    [Header("Navegación del Mando")]
    public GameObject primerBotonOpciones; // Ej: El Slider de Volumen General
    public GameObject botonOpcionesPrincipal; // El botón "Opciones" del menú principal

    public void AbrirOpciones()
    {
        // 1. Apagamos el menú principal y encendemos opciones
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);

        // 2. Le decimos al EventSystem que mueva el foco del mando al Slider
        // Limpiamos la selección actual primero para evitar bugs de Unity
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonOpciones);
    }

    public void CerrarOpciones()
    {
        // 1. Apagamos opciones y volvemos al menú principal
        panelOpciones.SetActive(false);
        panelPrincipal.SetActive(true);

        // 2. Le devolvemos el foco del mando al botón "Opciones"
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonOpcionesPrincipal);
    }
}