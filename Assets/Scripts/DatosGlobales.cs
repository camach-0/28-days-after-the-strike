using UnityEngine;

public static class DatosGlobales
{
    public static int cantidadJugadoresHumanos = 0;
    public static int[] personajesSeleccionados = new int[4] { -1, -1, -1, -1 };

    public static UnityEngine.InputSystem.InputDevice[][] dispositivosPorJugador = new UnityEngine.InputSystem.InputDevice[4][];
    public static string[] esquemasControlPorJugador = new string[4];

    // ¡NUEVO! El Diccionario de Identidad
    public static string[] nombresPersonajes = { "CHOLO", "COLLA", "CAMBA", "CHOLA" };
    public static Color[] coloresPersonajes = {
        Color.red,
        new Color(0.5f, 0f, 1f), // Morado (Colla)
        Color.cyan,
        Color.magenta
    };
}