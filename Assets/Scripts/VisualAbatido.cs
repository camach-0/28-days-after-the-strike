using UnityEngine;

[RequireComponent(typeof(SistemaSalud))]
public class VisualAbatido : MonoBehaviour
{
    [Header("Gráficos de Caído")]
    [Tooltip("Arrastra aquí el SpriteRenderer de tu personaje (el que tiene el cuadrado)")]
    public SpriteRenderer spritePersonaje;

    [Tooltip("Arrastra aquí tu imagen chistosa importada como Sprite")]
    public Sprite spriteAbatido;

    [Header("Icono de Ayuda (Opcional)")]
    public GameObject iconoRescate;

    private SistemaSalud miSalud;
    private bool estadoAnterior = false;

    // Memoria para no arruinar el color original (Cholo, Colla, etc.)
    private Sprite spriteOriginal;
    private Color colorOriginal;

    private void Awake()
    {
        miSalud = GetComponent<SistemaSalud>();
    }

    private void Start()
    {
        // 1. Apenas nace el jugador, guardamos su cuadrado y su color único
        if (spritePersonaje != null)
        {
            spriteOriginal = spritePersonaje.sprite;
            colorOriginal = spritePersonaje.color;
        }
    }

    private void Update()
    {
        if (miSalud.estaIncapacitado != estadoAnterior)
        {
            estadoAnterior = miSalud.estaIncapacitado;

            if (miSalud.estaIncapacitado)
            {
                // 2. Cae al suelo: Ponemos el meme y lo pintamos de blanco para que se vea bien
                if (spritePersonaje != null && spriteAbatido != null)
                {
                    spritePersonaje.sprite = spriteAbatido;
                    spritePersonaje.color = Color.white;
                }
                if (iconoRescate != null) iconoRescate.SetActive(true);
            }
            else
            {
                // 3. Lo levantan: Restauramos su cuadrado y su color exacto
                if (spritePersonaje != null)
                {
                    spritePersonaje.sprite = spriteOriginal;
                    spritePersonaje.color = colorOriginal;
                }
                if (iconoRescate != null) iconoRescate.SetActive(false);
            }
        }
    }
}