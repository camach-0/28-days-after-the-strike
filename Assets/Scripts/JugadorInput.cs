using UnityEngine;
using UnityEngine.InputSystem;

public class JugadorInput : MonoBehaviour
{
    [HideInInspector] public Camera camaraPrincipal; // El Cerebro se la asignará
    [HideInInspector] public Transform pivoteArma;   // El Cerebro se lo asignará

    // El Cerebro leerá estas variables públicas
    public Vector2 InputMovimiento { get; private set; }
    public Vector2 DireccionMirando { get; private set; } = Vector2.right;
    public bool EstaDisparando { get; private set; }

    // Variables de "un solo toque" (El cerebro las lee y luego las apaga)
    public bool IntentoRecargar { get; set; }
    public bool IntentoLinterna { get; set; }

    private bool usandoRaton = true;
    private Vector2 posicionRatonPantalla;

    public void OnMover(InputValue valor)
    {
        Vector2 inputBruto = valor.Get<Vector2>();
        InputMovimiento = (inputBruto.magnitude > 0.15f) ? inputBruto : Vector2.zero;
    }

    public void OnApuntar(InputValue valor)
    {
        Vector2 inputApunte = valor.Get<Vector2>();

        if (inputApunte.sqrMagnitude > 2f) // Es el ratón
        {
            usandoRaton = true;
            posicionRatonPantalla = inputApunte;
        }
        else if (inputApunte.sqrMagnitude > 0.01f) // Es el mando
        {
            usandoRaton = false;
            DireccionMirando = inputApunte.normalized;
        }
    }

    public void OnDisparar(InputValue valor)
    {
        EstaDisparando = valor.isPressed;
    }

    public void OnRecargar(InputValue valor)
    {
        if (valor.isPressed) IntentoRecargar = true;
    }

    public void OnLinterna(InputValue valor)
    {
        if (valor.isPressed) IntentoLinterna = true;
    }

    // Traduce la posición de la pantalla a coordenadas del mundo real
    public void ProcesarApuntadoRaton()
    {
        if (usandoRaton && camaraPrincipal != null && pivoteArma != null)
        {
            float distanciaZ = Mathf.Abs(camaraPrincipal.transform.position.z - transform.position.z);
            Vector3 screenPoint = new Vector3(posicionRatonPantalla.x, posicionRatonPantalla.y, distanciaZ);
            Vector3 mouseWorldPosition = camaraPrincipal.ScreenToWorldPoint(screenPoint);

            Vector2 direccionHaciaRaton = new Vector2(
                mouseWorldPosition.x - pivoteArma.position.x,
                mouseWorldPosition.y - pivoteArma.position.y
            );

            if (direccionHaciaRaton.sqrMagnitude > 0.01f)
            {
                DireccionMirando = direccionHaciaRaton.normalized;
            }
        }
    }
}