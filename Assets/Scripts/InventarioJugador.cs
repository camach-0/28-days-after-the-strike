using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(JugadorController))]
public class InventarioJugador : MonoBehaviour
{
    [Header("Ranuras estilo L4D2")]
    [Tooltip("0: Principal, 1: Secundaria, 2: Arrojadiza, 3: Botiquín, 4: Pastillas")]
    public ControladorArma[] ranuras = new ControladorArma[5];

    // En L4D siempre empiezas con la pistola en la mano (Slot 1)
    private int indiceSlotActual = 1;

    private JugadorController jugador;

    private void Awake()
    {
        jugador = GetComponent<JugadorController>();
    }

    private void Start()
    {
        // Al iniciar el juego, apagamos todas las armas excepto la que tenemos en la mano
        for (int i = 0; i < ranuras.Length; i++)
        {
            if (ranuras[i] != null)
            {
                ranuras[i].gameObject.SetActive(i == indiceSlotActual);
            }
        }

        // Si tenemos un arma en el slot inicial, se la equipamos al jugador
        if (ranuras[indiceSlotActual] != null)
        {
            jugador.armaEquipada = ranuras[indiceSlotActual];
        }
    }

    // Método para cambiar a un slot específico (0 al 4)
    public void CambiarSlot(int nuevoIndice)
    {
        // Filtros de seguridad
        if (nuevoIndice < 0 || nuevoIndice > 4) return; // Fuera de rango
        if (nuevoIndice == indiceSlotActual) return;    // Ya tenemos esta arma en la mano
        if (ranuras[nuevoIndice] == null) return;       // El slot está vacío, no hacemos nada

        ActualizarArmaActiva(nuevoIndice);
    }

    private void ActualizarArmaActiva(int nuevoIndice)
    {
        // 1. Apagar el arma vieja
        if (ranuras[indiceSlotActual] != null)
        {
            ranuras[indiceSlotActual].gameObject.SetActive(false);
        }

        // 2. Actualizar el índice al nuevo
        indiceSlotActual = nuevoIndice;

        // 3. Encender la nueva arma
        if (ranuras[indiceSlotActual] != null)
        {
            ranuras[indiceSlotActual].gameObject.SetActive(true);

            // 4. ¡CRUCIAL! Le pasamos la nueva arma al controlador del jugador
            jugador.armaEquipada = ranuras[indiceSlotActual];
        }
    }

    // --- ENTRADAS DE TECLADO (Botones 1, 2, 3, 4, 5) ---
    public void OnSlot1(InputValue valor) { if (valor.isPressed) CambiarSlot(0); } // Principal (M16/Escopeta)
    public void OnSlot2(InputValue valor) { if (valor.isPressed) CambiarSlot(1); } // Secundaria (Pistola/Melee)
    public void OnSlot3(InputValue valor) { if (valor.isPressed) CambiarSlot(2); } // Arrojadiza (Molotov)
    public void OnSlot4(InputValue valor) { if (valor.isPressed) CambiarSlot(3); } // Curación Mayor (Botiquín)
    public void OnSlot5(InputValue valor) { if (valor.isPressed) CambiarSlot(4); } // Curación Menor (Pastillas)

    // --- RUEDA DEL RATÓN (SCROLL) ---
    public void OnRuedaRaton(InputValue valor)
    {
        // Unity lee la rueda del ratón como un número: positivo (arriba) o negativo (abajo)
        float scroll = valor.Get<float>();

        if (scroll > 0) CiclarArma(-1); // Rueda hacia arriba -> Arma anterior
        else if (scroll < 0) CiclarArma(1);  // Rueda hacia abajo -> Arma siguiente
    }

    private void CiclarArma(int direccion)
    {
        int nuevoIndice = indiceSlotActual;
        int intentos = 0; // Para evitar que el juego se congele si solo tienes 1 arma

        do
        {
            nuevoIndice += direccion;

            // Si pasamos del slot 4, volvemos al 0. Si bajamos del 0, vamos al 4.
            if (nuevoIndice > 4) nuevoIndice = 0;
            if (nuevoIndice < 0) nuevoIndice = 4;

            intentos++;

            // Si el slot que tocaba TIENE un arma, la equipamos y rompemos el bucle
            if (ranuras[nuevoIndice] != null)
            {
                CambiarSlot(nuevoIndice);
                break;
            }
        } while (intentos < 5); // Máximo 5 intentos porque hay 5 slots
    }

    // --- MANDO PS4 / XBOX (CAMBIO RÁPIDO) ---
    public void OnCambioRapido(InputValue valor)
    {
        // Esto imita el botón Triángulo (Y en Xbox) del Left 4 Dead
        if (valor.isPressed)
        {
            // Si tenemos la principal, cambia a secundaria. Si tenemos cualquier otra, cambia a principal.
            if (indiceSlotActual == 0 && ranuras[1] != null)
                CambiarSlot(1);
            else if (ranuras[0] != null)
                CambiarSlot(0);
        }
    }
}