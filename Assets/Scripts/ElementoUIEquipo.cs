using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementoUIEquipo : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoNombre;
    public Image barraSaludFondo; // Opcional, para cambiar el fondo si muere
    public Image barraSaludRelleno;

    [Header("Colores Estilo L4D")]
    public Color colorSano = Color.green;
    public Color colorHerido = new Color(1f, 0.5f, 0f); // Naranja
    public Color colorIncapacitado = Color.red;

    private SistemaSalud saludObjetivo;

    // El cerebro llama a esta función para "conectar" al bot con esta tarjeta
    public void Inicializar(SistemaSalud objetivo, string nombre)
    {
        saludObjetivo = objetivo;
        if (textoNombre != null) textoNombre.text = nombre;
    }

    private void Update()
    {
        if (saludObjetivo != null && barraSaludRelleno != null)
        {
            // Actualizamos la cantidad de relleno
            barraSaludRelleno.fillAmount = saludObjetivo.vidaActual / saludObjetivo.vidaMaxima;

            // Cambiamos el color dependiendo del estado
            if (saludObjetivo.estaMuertoDefinitivo)
            {
                barraSaludRelleno.fillAmount = 0;
                textoNombre.color = Color.gray;
            }
            else if (saludObjetivo.estaIncapacitado)
            {
                barraSaludRelleno.color = colorIncapacitado;
            }
            else if (barraSaludRelleno.fillAmount > 0.4f)
            {
                barraSaludRelleno.color = colorSano;
            }
            else
            {
                barraSaludRelleno.color = colorHerido;
            }
        }
    }
}