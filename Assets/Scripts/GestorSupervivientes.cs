using UnityEngine;
using UnityEngine.InputSystem; // Necesario para apagar/encender controles

public class GestorSupervivientes : MonoBehaviour
{
    [Header("El Equipo (Orden: P1, P2, P3, P4)")]
    public GameObject[] personajesEnEscena;

    private void Start()
    {
        AsignarControles();
    }

    private void AsignarControles()
    {
        // Leemos cuántos humanos dijeron "LISTO" en la Escena 2
        int humanos = DatosGlobales.cantidadJugadoresHumanos;
        Debug.Log("Iniciando nivel con " + humanos + " jugadores humanos.");

        for (int i = 0; i < personajesEnEscena.Length; i++)
        {
            GameObject personaje = personajesEnEscena[i];
            PlayerInput inputHumano = personaje.GetComponent<PlayerInput>();
            JugadorController scriptHumano = personaje.GetComponent<JugadorController>();

            if (i < humanos)
            {
                // ESTE ES UN HUMANO
                // Mantenemos su control encendido
                if (inputHumano != null) inputHumano.enabled = true;
                if (scriptHumano != null) scriptHumano.enabled = true;

                personaje.name += " (Humano)";
            }
            else
            {
                // ESTE ES UN BOT
                // Le apagamos el control humano para que no se mueva con tu teclado
                if (inputHumano != null) inputHumano.enabled = false;
                if (scriptHumano != null) scriptHumano.enabled = false;

                personaje.name += " (Bot)";

                // TODO: Aquí encenderemos el script AliadoBotController más adelante
            }
        }
    }
}