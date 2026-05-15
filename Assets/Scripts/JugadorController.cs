using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class JugadorController : Entidad
{
    private Vector2 direccionMovimiento;
    private Vector2 direccionMirando = Vector2.right;
    private Rigidbody2D rb;

    [Header("¡OBLIGATORIO: Configuración de Cámara!")]
    public Camera camaraPrincipal;
    private bool usandoRaton = true;

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
        if (inputApunte.sqrMagnitude <= 2f && inputApunte.sqrMagnitude > 0.05f)
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

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            usandoRaton = true;
        }

        if (usandoRaton && camaraPrincipal != null && Mouse.current != null)
        {
            Vector2 posRatonPantalla = Mouse.current.position.ReadValue();
            float distanciaZ = Mathf.Abs(camaraPrincipal.transform.position.z - transform.position.z);
            Vector3 screenPoint = new Vector3(posRatonPantalla.x, posRatonPantalla.y, distanciaZ);
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

    public override void Morir() { /* Tu lógica de muerte intacta */ }
}