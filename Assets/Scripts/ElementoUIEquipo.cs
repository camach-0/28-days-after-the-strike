using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementoUIEquipo : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoNombre;
    public Image barraSaludRelleno;
    public Image fotoPerfil; // <--- ¡NUEVO! Para el cuadro de color

    [Header("Colores Estilo L4D")]
    public Color colorSano = Color.green;
    public Color colorHerido = new Color(1f, 0.5f, 0f);
    public Color colorIncapacitado = Color.red;

    private SistemaSalud saludObjetivo;

    // ¡ACTUALIZADO! Ahora recibe el color
    // Recibimos un Sprite (caraJugador) en lugar de un Color
    public void Inicializar(SistemaSalud objetivo, string nombre, Sprite caraJugador)
    {
        saludObjetivo = objetivo;
        if (textoNombre != null) textoNombre.text = nombre;

        if (fotoPerfil != null)
        {
            fotoPerfil.sprite = caraJugador; // Asignamos la cara
            fotoPerfil.color = Color.white;  // Limpiamos tintes para que se vea normal
        }
    }

    private void Update()
    {
        if (saludObjetivo != null && barraSaludRelleno != null)
        {
            barraSaludRelleno.fillAmount = saludObjetivo.vidaActual / saludObjetivo.vidaMaxima;

            if (saludObjetivo.estaMuertoDefinitivo)
            {
                barraSaludRelleno.fillAmount = 0;
                textoNombre.color = Color.gray;
                if (fotoPerfil != null) fotoPerfil.color = Color.gray;
            }
            else if (saludObjetivo.estaIncapacitado) barraSaludRelleno.color = colorIncapacitado;
            else if (barraSaludRelleno.fillAmount > 0.4f) barraSaludRelleno.color = colorSano;
            else barraSaludRelleno.color = colorHerido;
        }
    }
}