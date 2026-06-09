using System.Collections.Generic;
using UnityEngine;

public class GestorUIEquipo : MonoBehaviour
{
    [Header("Configuración UI")]
    [Tooltip("El Prefab de la tarjeta que crearemos por cada aliado")]
    public GameObject prefabTarjetaAliado;

    [Tooltip("El Panel vacío con un Vertical Layout Group donde aparecerán las tarjetas")]
    public Transform contenedorTarjetas;

    private SistemaSalud miSalud;

    private void Start()
    {
        // Buscamos nuestra propia salud para NO crear una tarjeta para nosotros mismos
        miSalud = GetComponentInParent<SistemaSalud>();

        // Le damos medio segundo al GameManager para que instancie a todos los bots antes de leer la lista
        Invoke(nameof(GenerarTarjetasEquipo), 0.5f);
    }

    private void GenerarTarjetasEquipo()
    {
        if (GameManager.Instancia == null || prefabTarjetaAliado == null || contenedorTarjetas == null)
        {
            Debug.LogWarning("Faltan referencias en GestorUIEquipo o no hay GameManager.");
            return;
        }

        // Recorremos a todos los supervivientes en el mapa
        foreach (SistemaSalud aliado in GameManager.Instancia.supervivientesActivos)
        {
            // Ignoramos nuestra propia salud (yo ya veo mi barra gigante abajo)
            if (aliado == miSalud) continue;

            // Creamos una tarjeta y la metemos dentro del contenedor
            GameObject nuevaTarjeta = Instantiate(prefabTarjetaAliado, contenedorTarjetas);
            ElementoUIEquipo scriptTarjeta = nuevaTarjeta.GetComponent<ElementoUIEquipo>();

            if (scriptTarjeta != null)
            {
                // Limpiamos un poco el nombre para que no diga "(Bot)" o "(Clone)" tan feo en pantalla
                string nombreLimpio = aliado.gameObject.name.Replace("(Clone)", "").Replace("(Bot)", "").Trim();

                scriptTarjeta.Inicializar(aliado, nombreLimpio);
            }
        }
    }
}