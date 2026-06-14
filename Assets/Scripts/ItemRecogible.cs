using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    [Header("Configuración del Item")]
    public GameObject armaPrefabParaMano;
    public int indiceSlot = 0;

    [Header("Memoria de Balas (Solo Armas)")]
    public bool tieneMemoria = false;
    public int cargadorGuardado = 0;
    public int reservaGuardada = 0;

    public void SerRecogido(InventarioJugador inventario, Transform pivoteArma)
    {
        // 1. Verificamos si hay un arma vieja para tirar
        if (inventario.ranuras[indiceSlot] != null)
        {
            ControladorArma armaVieja = inventario.ranuras[indiceSlot];

            if (armaVieja.prefabSuelo != null)
            {
                // Instanciamos el Prefab del suelo en los pies del jugador
                GameObject armaTiradaObj = Instantiate(armaVieja.prefabSuelo, inventario.transform.position, Quaternion.identity);

                if (armaVieja is ControladorArmaFuego armaFuegoVieja)
                {
                    // Buscamos en el objeto y en sus hijos (por si el script está en el Collider)
                    ItemRecogible scriptSuelo = armaTiradaObj.GetComponentInChildren<ItemRecogible>();

                    if (scriptSuelo != null)
                    {
                        scriptSuelo.tieneMemoria = true;
                        scriptSuelo.cargadorGuardado = armaFuegoVieja.municionActualCargador;
                        scriptSuelo.reservaGuardada = armaFuegoVieja.municionActualReserva;
                        Debug.Log($"<color=orange>DROP:</color> Arma tirada. Memoria inyectada al suelo -> Cargador: {scriptSuelo.cargadorGuardado}");
                    }
                    else
                    {
                        Debug.LogError("<color=red>ERROR:</color> El PrefabSuelo no tiene el script ItemRecogible.");
                    }
                }
            }

            Destroy(armaVieja.gameObject);
        }

        // 2. Instanciar la nueva arma en la mano
        GameObject nuevaArmaObj = Instantiate(armaPrefabParaMano, pivoteArma);
        nuevaArmaObj.transform.localPosition = new Vector3(0.15f, 0, 0);
        nuevaArmaObj.transform.localRotation = Quaternion.identity;

        ControladorArma nuevoControlador = nuevaArmaObj.GetComponent<ControladorArma>();
        inventario.ranuras[indiceSlot] = nuevoControlador;

        // 3. PASAR LA MEMORIA DE FORMA DIRECTA
        if (nuevoControlador is ControladorArmaFuego armaFuegoNueva)
        {
            if (tieneMemoria)
            {
                Debug.Log($"<color=green>RECOGER:</color> Arma recogida CON memoria. Inyectando Cargador: {cargadorGuardado}");
                armaFuegoNueva.CargarMemoria(cargadorGuardado, reservaGuardada);
            }
            else
            {
                Debug.Log("<color=yellow>RECOGER:</color> Arma nueva recogida SIN memoria (Balas al máximo).");
                armaFuegoNueva.LlenarMunicionPorDefecto();
            }
        }

        // 4. Equipar el arma (Esto avisa a la UI que debe actualizarse)
        inventario.CambiarSlot(indiceSlot, true);

        // 5. Destruir este objeto del suelo
        Destroy(gameObject);
    }
}