using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Configuración del Pool")]
    public string etiquetaPool = "ItemPistola";

    [Header("Configuración del Item")]
    public GameObject armaPrefabParaMano;
    public int indiceSlot = 0;

    public void SerRecogido(InventarioJugador inventario, Transform pivoteArma)
    {
        // 1. Verificamos si el jugador ya tiene un arma en esta ranura
        if (inventario.ranuras[indiceSlot] != null)
        {
            ControladorArma armaVieja = inventario.ranuras[indiceSlot];

            // 2. ¡EL DROP! Pedimos a la piscina que escupa la versión de SUELO de nuestra arma vieja
            string etiquetaSueloVieja = armaVieja.EtiquetaPoolSuelo;
            if (!string.IsNullOrEmpty(etiquetaSueloVieja))
            {
                // La instanciamos exactamente donde están los pies del jugador
                PoolManager.Instancia.SolicitarObjeto(etiquetaSueloVieja, inventario.transform.position, Quaternion.identity);
                Debug.Log($"Arma arrojada al piso: {etiquetaSueloVieja}");
            }

            // 3. Destruimos el arma visual vieja de la mano
            Destroy(armaVieja.gameObject);
        }

        // 4. Instanciamos el nuevo arma en la mano
        GameObject nuevaArmaObj = Instantiate(armaPrefabParaMano, pivoteArma);
        nuevaArmaObj.transform.localPosition = new Vector3(0.6f, 0, 0);
        nuevaArmaObj.transform.localRotation = Quaternion.identity;

        ControladorArma nuevoControlador = nuevaArmaObj.GetComponent<ControladorArma>();
        inventario.ranuras[indiceSlot] = nuevoControlador;

        // 5. Equipamos la nueva arma
        inventario.CambiarSlot(indiceSlot, true);

        // 6. Devolvemos ESTE ítem (el que acabamos de recoger) a la piscina
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }
}