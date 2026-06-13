using UnityEngine;
using UnityEngine.Rendering.Universal; // Necesario para controlar la Luz 2D de URP

public class EfectoDestelloArma : MonoBehaviour
{
    [Header("Configuración de Animación")]
    [Tooltip("Arrastra aquí los 7 sprites de tu Muzzle Flash en orden")]
    public Sprite[] framesAnimacion;
    [Tooltip("A cuántos FPS debe correr la animación (Ej: 30 o 60)")]
    public float framesPorSegundo = 60f;

    [Header("Referencias (Componentes en este objeto)")]
    public SpriteRenderer spriteRenderer;
    public Light2D luzDestello;

    // Variables internas ultra-optimizadas (Sin Garbage Collection)
    private int frameActual = 0;
    private float tiempoPorFrame;
    private float temporizador = 0f;
    private bool estaReproduciendo = false;

    private void Awake()
    {
        // Calculamos matemáticamente cuánto debe durar cada frame
        tiempoPorFrame = 1f / framesPorSegundo;

        // Nos aseguramos de que empiece apagado
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (luzDestello != null) luzDestello.enabled = false;
    }

    // Función que llamará tu arma exactamente al disparar
    public void ReproducirDestello()
    {
        if (framesAnimacion == null || framesAnimacion.Length == 0) return;

        // Reiniciamos los contadores
        frameActual = 0;
        temporizador = 0f;
        estaReproduciendo = true;

        // 1. Encendemos los componentes
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = framesAnimacion[0];
            spriteRenderer.enabled = true;

            // TRUCO PRO: Rotación aleatoria en Z para que no se vea repetitivo
            float anguloAleatorio = Random.Range(0f, 1f);
            spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, anguloAleatorio);
        }

        // 2. Encendemos la luz en el primer frame
        if (luzDestello != null) luzDestello.enabled = true;
    }

    private void Update()
    {
        // Si no está reproduciendo, salimos inmediatamente (Costo de CPU casi cero)
        if (!estaReproduciendo) return;

        temporizador += Time.deltaTime;

        // ¿Es hora de cambiar de frame?
        if (temporizador >= tiempoPorFrame)
        {
            temporizador -= tiempoPorFrame; // Mantenemos la precisión del tiempo
            frameActual++;

            // ¿Llegamos al final de los 7 frames?
            if (frameActual >= framesAnimacion.Length)
            {
                // Apagamos todo
                estaReproduciendo = false;
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                if (luzDestello != null) luzDestello.enabled = false;
            }
            else
            {
                // Pasamos al siguiente frame de la animación
                if (spriteRenderer != null) spriteRenderer.sprite = framesAnimacion[frameActual];
            }
        }
    }
}