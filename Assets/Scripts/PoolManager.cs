using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instancia;


    [System.Serializable]
    public class ConfiguracionPool
    {
        public string etiqueta; 
        public GameObject prefab;
        public int cantidadInicial;
    }

    [Header("Configuración de Piscinas")]
    public List<ConfiguracionPool> listaDePiscinas;

    // El "Diccionario" que guarda las colas de objetos listos para usarse
    private Dictionary<string, Queue<GameObject>> diccionarioPiscinas;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        ConstruirPiscinas();
    }

    private void ConstruirPiscinas()
    {
        diccionarioPiscinas = new Dictionary<string, Queue<GameObject>>();

        foreach (ConfiguracionPool pool in listaDePiscinas)
        {
            Queue<GameObject> colaDeObjetos = new Queue<GameObject>();

            for (int i = 0; i < pool.cantidadInicial; i++)
            {
              
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false); // Nacen apagados
                colaDeObjetos.Enqueue(obj);
            }

            diccionarioPiscinas.Add(pool.etiqueta, colaDeObjetos);
        }
    }

    
    public GameObject SolicitarObjeto(string etiqueta, Vector3 posicion, Quaternion rotacion)
    {
        if (!diccionarioPiscinas.ContainsKey(etiqueta))
        {
            Debug.LogWarning($"[PoolManager] No existe una piscina con la etiqueta: {etiqueta}");
            return null;
        }

        // Sistema de seguridad: Si nos quedamos sin objetos, creamos uno nuevo dinámicamente
        if (diccionarioPiscinas[etiqueta].Count == 0)
        {
            ConfiguracionPool poolOriginal = listaDePiscinas.Find(p => p.etiqueta == etiqueta);
            GameObject nuevoObj = Instantiate(poolOriginal.prefab, transform);
            nuevoObj.SetActive(false);
            diccionarioPiscinas[etiqueta].Enqueue(nuevoObj);
            Debug.Log($"[PoolManager] La piscina {etiqueta} se quedó corta. Creando objeto extra.");
        }

      
        GameObject objetoASacar = diccionarioPiscinas[etiqueta].Dequeue();

        objetoASacar.transform.position = posicion;
        objetoASacar.transform.rotation = rotacion;
        objetoASacar.SetActive(true); 

        return objetoASacar;
    }

    
    public void DevolverObjeto(string etiqueta, GameObject objeto)
    {
        objeto.SetActive(false);
        diccionarioPiscinas[etiqueta].Enqueue(objeto);
    }
}