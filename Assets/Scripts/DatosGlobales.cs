using UnityEngine;

// Al no heredar de MonoBehaviour, esta clase es puramente de datos.
// Al ser "static", su memoria sobrevive a los cambios de escena.
public static class DatosGlobales
{
    public static int cantidadJugadoresHumanos = 0;

    // Guardará la elección de cada jugador (0=Cholo, 1=Colla, 2=Camba, 3=Chola)
    // El -1 significa que nadie ha elegido ese espacio aún.
    public static int[] personajesSeleccionados = new int[4] { -1, -1, -1, -1 };
    // Agrega esto dentro de la clase DatosGlobales junto a tus otras variables estáticas
    public static UnityEngine.InputSystem.InputDevice[][] dispositivosPorJugador = new UnityEngine.InputSystem.InputDevice[4][];
    public static string[] esquemasControlPorJugador = new string[4];
}