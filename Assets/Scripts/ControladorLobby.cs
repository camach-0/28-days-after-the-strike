using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;

public class ControladorLobby : MonoBehaviour
{
    public int miIDJugador; 
    public int indicePersonajeActual = 0;
    public bool estaListo = false;

    // Variables para evitar que la cruceta se mueva demasiado rápido
    private bool puedeMoverse = true;
    private float tiempoEsperaMovimiento = 0.2f;
    private float temporizadorMovimiento = 0f;

    private void Start()
    {

        miIDJugador = LobbyManager.Instancia.RegistrarNuevoJugador(this);
        indicePersonajeActual = miIDJugador;

        PlayerInput miInput = GetComponent<PlayerInput>();
        if (miInput != null)
        {
            DatosGlobales.dispositivosPorJugador[miIDJugador] = miInput.devices.ToArray();
            DatosGlobales.esquemasControlPorJugador[miIDJugador] = miInput.currentControlScheme;
        }

        LobbyManager.Instancia.ActualizarVisuales();
    }

    private void Update()
    {
        if (!puedeMoverse)
        {
            temporizadorMovimiento += Time.deltaTime;
            if (temporizadorMovimiento >= tiempoEsperaMovimiento)
            {
                puedeMoverse = true;
                temporizadorMovimiento = 0f;
            }
        }
    }

    public void OnNavigate(InputValue valor)
    {
   
        if (estaListo || !puedeMoverse) return;

        Vector2 direccion = valor.Get<Vector2>();

        if (direccion.x > 0.5f)
        {
            MoverCursor(1); 
            puedeMoverse = false;
        }
        else if (direccion.x < -0.5f)
        {
            MoverCursor(-1);
            puedeMoverse = false;
        }
    }
    public void OnSubmit()
    {
        if (!estaListo)
        {
            if (!LobbyManager.Instancia.EstaPersonajeLibre(indicePersonajeActual))
            {
                return;
            }

            estaListo = true;
            DatosGlobales.GuardarPersonaje(miIDJugador, indicePersonajeActual);
            LobbyManager.Instancia.ActualizarVisuales();
            LobbyManager.Instancia.ComprobarTodosListos();
        }
    }

    public void OnCancel()
    {
        if (estaListo)
        {
            estaListo = false;
            LobbyManager.Instancia.ActualizarVisuales();
            LobbyManager.Instancia.ComprobarTodosListos();
        }
        else
        {
            if (miIDJugador == 0)
            {
                LobbyManager.Instancia.VolverAlMenu();
            }
        }
    }


    private void MoverCursor(int direccion)
    {
        int intentos = 0;
        do
        {
            indicePersonajeActual += direccion;

            if (indicePersonajeActual >= LobbyManager.Instancia.slotsVisuales.Length)
                indicePersonajeActual = 0;
            else if (indicePersonajeActual < 0)
                indicePersonajeActual = LobbyManager.Instancia.slotsVisuales.Length - 1;

            intentos++;
        }
        while (!LobbyManager.Instancia.EstaPersonajeLibre(indicePersonajeActual) && intentos < 4);

        LobbyManager.Instancia.ActualizarVisuales();
    }
}