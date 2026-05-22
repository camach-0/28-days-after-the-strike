using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class JugadorController : Entidad
{
    private Vector2 direccionMovimiento;
    private Vector2 direccionMirando = Vector2.right;
    private Rigidbody2D rb;
    private Vector2 posicionRatonPantalla;

    [Header("¡OBLIGATORIO: Configuración de Cámara!")]
    public Camera camaraPrincipal;
    private bool usandoRaton = true;

    [Header("Linterna")]
    public GameObject objetoLinterna;

    [Header("Referencias Visuales")]
    public Transform pivoteArma;

    // AHORA USAMOS LA CLASE PADRE (Sirve para cualquier arma)
    [Header("Conexión con el Arma")]
    public ControladorArma armaEquipada;

    // NUEVO: Para saber si mantiene apretado el gatillo
    private bool estaDisparando = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
    }

    public override void Start()
    {
        base.Start();
        if (GameManager.Instancia != null) GameManager.Instancia.RegistrarJugador(this);
        if (barraDeVidaUI != null) barraDeVidaUI.fillAmount = 1f;
    }

    public void OnMover(InputValue valor)
    {
        Vector2 inputBruto = valor.Get<Vector2>();
        direccionMovimiento = (inputBruto.magnitude > 0.15f) ? inputBruto : Vector2.zero;
    }

    public void OnApuntar(InputValue valor)
    {
        if (estaMuerto) return;
        Vector2 inputApunte = valor.Get<Vector2>();

        // Si el valor es altísimo, son coordenadas de pantalla (Es el Ratón)
        if (inputApunte.sqrMagnitude > 2f)
        {
            usandoRaton = true;
            posicionRatonPantalla = inputApunte; // Guardamos su posición
        }
        // Si el valor es pequeñito, es la palanca del Mando (-1 a 1)
        else if (inputApunte.sqrMagnitude > 0.01f)
        {
            usandoRaton = false;
            direccionMirando = inputApunte.normalized;
        }
    }

    // NUEVO SISTEMA DE DISPARO CONTINUO
    public void OnDisparar(InputValue valor)
    {
        if (estaMuerto) return;

        // isPressed es true cuando el dedo aprieta, false cuando suelta
        estaDisparando = valor.isPressed;

        // LÓGICA SEMIAUTOMÁTICA O RÁFAGA (Al hacer un solo clic)
        if (valor.isPressed && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                // Si el arma NO es automática (Escopeta, Pistola, SCAR), dispara aquí
                if (!armaFuego.datosFuego.esAutomatica || armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
            else
            {
                // Armas Melee o Arrojadizas atacan con clic por defecto
                armaEquipada.IntentarAtaque(direccionMirando);
            }
        }
    }

    public void OnRecargar(InputValue valor)
    {
        if (estaMuerto) return;
        // Solo recarga si el arma es de fuego
        if (valor.isPressed && armaEquipada != null && armaEquipada is ControladorArmaFuego)
        {
            ((ControladorArmaFuego)armaEquipada).IniciarRecarga();
        }
    }

    private void Update()
    {
        if (estaMuerto) return;

        // YA NO USAMOS Mouse.current. ¡Usamos los datos aislados de OnApuntar!
        if (usandoRaton && camaraPrincipal != null)
        {
            float distanciaZ = Mathf.Abs(camaraPrincipal.transform.position.z - transform.position.z);
            Vector3 screenPoint = new Vector3(posicionRatonPantalla.x, posicionRatonPantalla.y, distanciaZ);
            Vector3 mouseWorldPosition = camaraPrincipal.ScreenToWorldPoint(screenPoint);

            if (pivoteArma != null)
            {
                Vector2 direccionHaciaRaton = new Vector2(
                    mouseWorldPosition.x - pivoteArma.position.x,
                    mouseWorldPosition.y - pivoteArma.position.y
                );

                if (direccionHaciaRaton.sqrMagnitude > 0.01f)
                {
                    direccionMirando = direccionHaciaRaton.normalized;
                }
            }
        }

        if (pivoteArma != null)
        {
            float angulo = Mathf.Atan2(direccionMirando.y, direccionMirando.x) * Mathf.Rad2Deg;
            pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);
        }

        // AQUI EJECUTAMOS EL DISPARO CONTINUO
        if (estaDisparando && armaEquipada != null)
        {
            if (armaEquipada is ControladorArmaFuego armaFuego)
            {
                // Si el arma ES automática y NO es ráfaga (Uzi, M16)
                if (armaFuego.datosFuego.esAutomatica && !armaFuego.datosFuego.esRafaga)
                {
                    armaEquipada.IntentarAtaque(direccionMirando);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return;
        rb.MovePosition(rb.position + direccionMovimiento * velocidadMovimiento * Time.fixedDeltaTime);
    }

    public override void Morir()
    {
        base.Morir();
        Debug.Log(gameObject.name + " ha muerto.");

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.VerificarEstadoJugadores();
        }

        // --- CÓDIGO ACTUALIZADO DE LA CÁMARA ---
        // Busca si tiene una cámara asignada como hija y la suelta en el mapa
        Camera miCamara = GetComponentInChildren<Camera>();
        if (miCamara != null)
        {
            miCamara.transform.SetParent(null);
        }

        gameObject.SetActive(false);
    }
    public void OnLinterna(InputValue valor)
    {
        if (valor.isPressed && objetoLinterna != null)
        {
            objetoLinterna.SetActive(!objetoLinterna.activeSelf);
        }
    }
}