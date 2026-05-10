using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Esto permite que otros scripts encuentren al UIManager fácilmente
    public static UIManager Instancia;
    public GameObject panelGameOver;

    [Header("Barras de Vida por Jugador")]
    [Tooltip("Arrastra aquí el RellenoVerde del P1, P2, P3 y P4 en orden.")]
    public Image[] barrasDeVida;

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
    public void MostrarPantallaGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            // Opcional: Puedes usar una animación o un CanvasGroup para que sea un desvanecimiento suave
        }
    }
}