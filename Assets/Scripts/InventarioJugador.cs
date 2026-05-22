using UnityEngine;
using System; // Necesario para los Eventos

[RequireComponent(typeof(JugadorController))]
public class InventarioJugador : MonoBehaviour
{
    [Header("Ranuras estilo L4D2")]
    public ControladorArma[] ranuras = new ControladorArma[5];

    // PATRÓN OBSERVER: Evento que avisa a quien quiera escuchar que cambiamos de arma
    public event Action<int> OnArmaCambiada;

    private int indiceSlotActual = 1;
    private JugadorController jugador;

    private void Awake()
    {
        jugador = GetComponent<JugadorController>();
    }

    private void Start()
    {
        for (int i = 0; i < ranuras.Length; i++)
        {
            if (ranuras[i] != null)
            {
                ranuras[i].gameObject.SetActive(i == indiceSlotActual);
            }
        }

        if (ranuras[indiceSlotActual] != null)
        {
            jugador.armaEquipada = ranuras[indiceSlotActual];
        }

        // Lanzamos el evento al iniciar para que la UI se ilumine correctamente
        OnArmaCambiada?.Invoke(indiceSlotActual);
    }

    public void CambiarSlot(int nuevoIndice, bool forzar = false)
    {
        if (nuevoIndice < 0 || nuevoIndice > 4) return;
        if (!forzar && nuevoIndice == indiceSlotActual) return;
        if (ranuras[nuevoIndice] == null) return;

        ActualizarArmaActiva(nuevoIndice);
    }

    private void ActualizarArmaActiva(int nuevoIndice)
    {
        if (ranuras[indiceSlotActual] != null) ranuras[indiceSlotActual].gameObject.SetActive(false);

        indiceSlotActual = nuevoIndice;

        if (ranuras[indiceSlotActual] != null)
        {
            ranuras[indiceSlotActual].gameObject.SetActive(true);
            jugador.armaEquipada = ranuras[indiceSlotActual];
        }

        // ¡Gritamos al vacío el nuevo número de arma!
        OnArmaCambiada?.Invoke(indiceSlotActual);
    }

    // ====================================================================
    // --- MÉTODOS PÚBLICOS PARA SER INVOCADOS POR EL CEREBRO (Fase 2) ---
    // ====================================================================

    // El Controlador llamará a esto pasándole directamente la dirección (-1 o 1)
    public void CiclarArma(int direccion)
    {
        int nuevoIndice = indiceSlotActual;
        int intentos = 0;
        do
        {
            nuevoIndice += direccion;
            if (nuevoIndice > 4) nuevoIndice = 0;
            if (nuevoIndice < 0) nuevoIndice = 4;
            intentos++;

            if (ranuras[nuevoIndice] != null)
            {
                CambiarSlot(nuevoIndice);
                break;
            }
        } while (intentos < 5);
    }

    // El Controlador llamará a esto cuando se presione el botón de cambio rápido
    public void EjecutarCambioRapido()
    {
        if (indiceSlotActual == 0 && ranuras[1] != null) CambiarSlot(1);
        else if (ranuras[0] != null) CambiarSlot(0);
    }
}