using UnityEngine;

public static class DatosGlobales
{
    public static int cantidadJugadoresHumanos = 0;

    // Aquí guardaremos qué foto (0, 1, 2, 3) eligió cada jugador (P1, P2, P3, P4)
    public static int[] personajesSeleccionados = new int[4] { -1, -1, -1, -1 };

    public static UnityEngine.InputSystem.InputDevice[][] dispositivosPorJugador = new UnityEngine.InputSystem.InputDevice[4][];
    public static string[] esquemasControlPorJugador = new string[4];

    public static string[] nombresPersonajes = { "CHOLO", "COLLA", "CAMBA", "CHOLA" };
    public static Color[] coloresPersonajes = {
        Color.red,
        new Color(0.5f, 0f, 1f), // Morado
        Color.cyan,
        Color.magenta
    };

    // ¡La función que faltaba para guardar la selección!
    public static void GuardarPersonaje(int idJugador, int indicePersonaje)
    {
        if (idJugador >= 0 && idJugador < 4)
        {
            personajesSeleccionados[idJugador] = indicePersonaje;
        }
    }
}