using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Configuración del Item")]
    [Tooltip("El prefab del arma que se pondrá en la mano del jugador")]
    public GameObject armaPrefabParaMano;

    [Tooltip("0 = Principal, 1 = Secundaria, 2 = Granada, 3 = Botiquín, 4 = Pastillas")]
    public int indiceSlot = 0;

    // Esta función la llamará el jugador cuando apriete el botón de interactuar
    public void SerRecogido(InventarioJugador inventario, Transform pivoteArma)
    {
        // 1. Si el jugador ya tiene un arma en ese slot, la destruimos para hacer espacio
        // (En L4D el arma vieja cae al piso, pero para simplificar ahora, la destruiremos)
        if (inventario.ranuras[indiceSlot] != null)
        {
            Destroy(inventario.ranuras[indiceSlot].gameObject);
        }

        // 2. Creamos (Instanciamos) el nuevo arma directamente como hijo del PivoteArma
        GameObject nuevaArmaObj = Instantiate(armaPrefabParaMano, pivoteArma);

        // 3. Obtenemos su controlador y se lo damos al inventario
        ControladorArma nuevoControlador = nuevaArmaObj.GetComponent<ControladorArma>();
        inventario.ranuras[indiceSlot] = nuevoControlador;

        // 4. Forzamos al jugador a equiparse esta nueva arma (le pasamos 'true')
        inventario.CambiarSlot(indiceSlot, true);

        // 5. Destruimos el objeto brillante del suelo porque ya lo recogimos
        Destroy(gameObject);
    }
}