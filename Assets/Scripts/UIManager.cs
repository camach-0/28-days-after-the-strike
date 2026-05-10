using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Mantenemos el Singleton por si en el futuro queremos llamarlo para 
    // mostrar menús de pausa, puntajes o mensajes en pantalla.
    public static UIManager Instancia;

    private void Awake()
    {
        // Configuramos el Singleton
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Aquí puedes agregar funciones futuras, como:
    // public void MostrarMenuPausa() { ... }
    // public void ActualizarPuntaje(int puntos) { ... }
}