using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [Tooltip("Debe ser el mismo nombre que pusiste en la Etiqueta del PoolManager")]
    public string etiquetaPool = "ItemPistola"; // <-- ¡NUEVO! Conectado a la piscina

    [Header("Configuración del Item")]
    [Tooltip("El prefab del arma que se pondrá en la mano del jugador")]
    public GameObject armaPrefabParaMano;

    [Tooltip("0 = Principal, 1 = Secundaria, 2 = Granada, 3 = Botiquín, 4 = Pastillas")]
    public int indiceSlot = 0;

    // Esta función la llamará el Cerebro cuando apriete el botón de interactuar
    public void SerRecogido(InventarioJugador inventario, Transform pivoteArma)
    {
        // 1. Borramos el arma vieja de la mano (Dejamos el Destroy temporalmente 
        // hasta que implementemos una mecánica para "tirar el arma al piso")
        if (inventario.ranuras[indiceSlot] != null)
        {
            Destroy(inventario.ranuras[indiceSlot].gameObject);
        }

        // 2. Instanciamos el nuevo arma directamente como hijo del PivoteArma
        GameObject nuevaArmaObj = Instantiate(armaPrefabParaMano, pivoteArma);

        // Forzamos a que se pegue al centro del jugador
        nuevaArmaObj.transform.localPosition = new Vector3(0.6f, 0, 0);
        nuevaArmaObj.transform.localRotation = Quaternion.identity; // Evita que nazca torcido

        // 3. Obtenemos su controlador y se lo damos al inventario
        ControladorArma nuevoControlador = nuevaArmaObj.GetComponent<ControladorArma>();
        inventario.ranuras[indiceSlot] = nuevoControlador;

        // 4. Forzamos al jugador a equiparse esta nueva arma (le pasamos 'true')
        inventario.CambiarSlot(indiceSlot, true);

        // ====================================================================
        // --- 5. ¡ADIÓS DESTROY! Devolvemos el ítem del suelo a la piscina ---
        // ====================================================================
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}