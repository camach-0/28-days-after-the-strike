using UnityEngine;
using System; // Necesario para los Eventos

[RequireComponent(typeof(JugadorController))]
public class InventarioJugador : MonoBehaviour
{
    [Header("Ranuras estilo L4D2")]
    public ControladorArma[] ranuras = new ControladorArma[5];

    [Header("Configuración de Muerte")]
    [Tooltip("Prefab de la pistola básica (de mano) que recibirá al revivir")]
    public GameObject prefabPistolaBase;

    // PATRÓN OBSERVER: Evento que avisa a quien quiera escuchar que cambiamos de arma
    public event Action<int> OnArmaCambiada;

    private int indiceSlotActual = 1;
    private JugadorController jugador;
    private SistemaSalud miSalud;

    private void Awake()
    {
        jugador = GetComponent<JugadorController>();
        miSalud = GetComponent<SistemaSalud>();
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

        OnArmaCambiada?.Invoke(indiceSlotActual);
    }

    public void CambiarSlot(int nuevoIndice, bool forzar = false)
    {
        // --- CANDADO 1: BOTONES NUMÉRICOS ---
        // Si estamos incapacitados y NO es un cambio forzado por el sistema, bloqueamos la acción.
        if (miSalud != null && miSalud.estaIncapacitado && !forzar) return;

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

        OnArmaCambiada?.Invoke(indiceSlotActual);
    }

    // ====================================================================
    // --- MÉTODOS PÚBLICOS PARA SER INVOCADOS POR EL CEREBRO (Fase 2) ---
    // ====================================================================

    public void CiclarArma(int direccion)
    {
        // --- CANDADO 2: RUEDA DEL RATÓN ---
        if (miSalud != null && miSalud.estaIncapacitado) return;

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

    public void EjecutarCambioRapido()
    {
        // --- CANDADO 3: BOTÓN DE CAMBIO RÁPIDO (Q) ---
        if (miSalud != null && miSalud.estaIncapacitado) return;

        if (indiceSlotActual == 0 && ranuras[1] != null) CambiarSlot(1);
        else if (ranuras[0] != null) CambiarSlot(0);
    }

    // ====================================================================
    // --- SISTEMA DE RESURRECCIÓN ---
    // ====================================================================

    public void ResetearInventarioPorMuerte()
    {
        // 1. Soltamos los objetos con dispersión
        for (int i = 0; i < ranuras.Length; i++)
        {
            if (ranuras[i] != null)
            {
                if (ranuras[i].prefabSuelo != null)
                {
                    // ¡NUEVO! Creamos un pequeño desplazamiento aleatorio
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.0f;
                    Vector3 posicionCaida = transform.position + new Vector3(offset.x, offset.y, 0f);

                    Instantiate(ranuras[i].prefabSuelo, posicionCaida, Quaternion.identity);
                }

                Destroy(ranuras[i].gameObject);
                ranuras[i] = null;
            }
        }

        // 2. Le creamos su pistola en el lugar CORRECTO (El Pivote)
        if (prefabPistolaBase != null)
        {
            // ¡CORRECCIÓN! Buscamos el pivote para pegarle el arma ahí
            Transform pivote = jugador.GetComponent<JugadorMovimiento>().pivoteArma;

            GameObject nuevaPistola = Instantiate(prefabPistolaBase, pivote);
            ControladorArma scriptPistola = nuevaPistola.GetComponent<ControladorArma>();

            if (scriptPistola != null)
            {
                ranuras[1] = scriptPistola;
                CambiarSlot(1, true);
            }
        }
    }
}