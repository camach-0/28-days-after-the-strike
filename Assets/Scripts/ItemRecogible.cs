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
        if (inventario.ranuras[indiceSlot] != null)
        {
            Destroy(inventario.ranuras[indiceSlot].gameObject);
        }

        // 2. Creamos (Instanciamos) el nuevo arma directamente como hijo del PivoteArma
        GameObject nuevaArmaObj = Instantiate(armaPrefabParaMano, pivoteArma);

        // --- EL PARCHE: Forzamos a que se pegue al centro del jugador ---
        // Usa Vector3.zero para que esté en el centro exacto (invisible), 
        // o usa new Vector3(0.6f, 0, 0) si quieres que se vea un poco hacia adelante.
        nuevaArmaObj.transform.localPosition = new Vector3(0.6f, 0, 0);
        nuevaArmaObj.transform.localRotation = Quaternion.identity; // Evita que nazca torcido

        // 3. Obtenemos su controlador y se lo damos al inventario
        ControladorArma nuevoControlador = nuevaArmaObj.GetComponent<ControladorArma>();
        inventario.ranuras[indiceSlot] = nuevoControlador;

        // 4. Forzamos al jugador a equiparse esta nueva arma (le pasamos 'true')
        inventario.CambiarSlot(indiceSlot, true);

        // 5. Destruimos el objeto brillante del suelo porque ya lo recogimos
        Destroy(gameObject);
    }
}