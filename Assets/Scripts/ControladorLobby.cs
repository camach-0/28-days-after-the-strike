using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorLobby : MonoBehaviour
{
    public int miIDJugador; // 0 (P1), 1 (P2), etc.
    public int indicePersonajeActual = 0; // En qué panel está parado (0 al 3)
    public bool estaListo = false;

    private bool ejeEnUso = false; // Filtro para que el mando no "patine" súper rápido

    private void Start()
    {
        // Pedimos un ID al Manager al aparecer en pantalla
        miIDJugador = LobbyManager.Instancia.RegistrarNuevoJugador(this);
        indicePersonajeActual = miIDJugador; // Cada quien empieza en su propio panel
        LobbyManager.Instancia.ActualizarVisuales();
    }

    // Usamos OnMover (tus flechas o joystick)
    public void OnMover(InputValue valor)
    {
        if (estaListo) return;

        Vector2 input = valor.Get<Vector2>();

        // EL CHISMOSO: Esto imprimirá en la consola las flechas que aprietes
        Debug.Log("Moviendo cursor. Señal recibida: " + input);

        if (input.x > 0.5f && !ejeEnUso)
        {
            MoverCursor(1);
            ejeEnUso = true;
        }
        else if (input.x < -0.5f && !ejeEnUso)
        {
            MoverCursor(-1);
            ejeEnUso = true;
        }
        else if (Mathf.Abs(input.x) < 0.2f)
        {
            ejeEnUso = false;
        }
    }

    public void OnDisparar(InputValue valor)
    {
        // EL CHISMOSO: Imprimirá si detecta tu botón de selección
        Debug.Log("¡Botón de selección presionado!");

        if (valor.isPressed && !estaListo)
        {
            estaListo = true;
            DatosGlobales.personajesSeleccionados[miIDJugador] = indicePersonajeActual;
            LobbyManager.Instancia.ComprobarTodosListos();
            LobbyManager.Instancia.ActualizarVisuales();
        }
    }

    private void MoverCursor(int direccion)
    {
        indicePersonajeActual += direccion;

        // Efecto "Pac-Man" (si pasas del último, vuelves al primero)
        if (indicePersonajeActual > 3) indicePersonajeActual = 0;
        if (indicePersonajeActual < 0) indicePersonajeActual = 3;

        LobbyManager.Instancia.ActualizarVisuales();
    }
}