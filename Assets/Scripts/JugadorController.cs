using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class JugadorController : Entidad
{
    private Vector2 direccionMovimiento;
    private Vector2 direccionMirando = Vector2.right;
    private Rigidbody2D rb;

    [Header("Referencias Visuales")]
    public Transform pivoteArma;
    [Header("Armas y Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    [Header("Filtros Anti-Bugs")]
    private Vector2 direccionPendiente;
    private float temporizadorGhosting = 0f;
    private float tiempoGracia = 0.05f; // 50 milisegundos para perdonar errores humanos al soltar teclas

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public override void Start()
    {
        // 1. Obtenemos el índice del jugador (P1 es 0, P2 es 1, etc.)
        PlayerInput pi = GetComponent<PlayerInput>();
        if (pi != null)
        {
            int idJugador = pi.playerIndex;

            // 2. Le pedimos al UIManager la barra que nos toca
            if (UIManager.Instancia != null && idJugador < UIManager.Instancia.barrasDeVida.Length)
            {
                // Asignamos la barra a la variable que heredamos de Entidad
                barraDeVidaUI = UIManager.Instancia.barrasDeVida[idJugador];
            }
        }

        // 3. Llamamos al Start base de Entidad para que inicialice la vida
        base.Start();
        velocidadMovimiento = 6f;
        direccionPendiente = direccionMirando;
    }

    public void OnMover(InputValue valor)
    {
        Vector2 inputBruto = valor.Get<Vector2>();

        // 1. SOLUCIÓN AL MANDO: Ignoramos el rebote del resorte con un límite mayor (0.15f)
        if (inputBruto.magnitude > 0.15f)
        {
            direccionMovimiento = inputBruto;

            // Redondeamos para mantener el estilo 16-bits
            Vector2 nuevaDir = new Vector2(Mathf.Round(inputBruto.x), Mathf.Round(inputBruto.y)).normalized;

            // SOLUCIÓN AL MANDO (Parte 2): Si por error se redondea a cero, NO lo guardamos
            if (nuevaDir != Vector2.zero)
            {
                direccionPendiente = nuevaDir;
            }
        }
        else
        {
            // El jugador se detuvo por completo
            direccionMovimiento = Vector2.zero;

            // SOLUCIÓN AL TECLADO (Parte 1): Cancelamos el cambio de dirección accidental del último milisegundo
            direccionPendiente = direccionMirando;
        }
    }
    // Unity detecta automáticamente la acción "Disparar" de tu ControlesJuego
    public void OnDisparar(InputValue valor)
    {
        // isPressed verifica si apretamos el botón (y no si lo estamos soltando)
        if (valor.isPressed && !estaMuerto && balaPrefab != null && puntoDisparo != null)
        {
            // 1. Instanciar (Crear) la bala en las coordenadas del PuntoDisparo
            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);

            // 2. Comunicarnos con el script de la bala para decirle hacia dónde ir
            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccionMirando);
        }
    }

    private void Update()
    {
        // SOLUCIÓN AL TECLADO (Parte 2): Aplicamos el Tiempo de Gracia
        if (direccionMirando != direccionPendiente)
        {
            temporizadorGhosting -= Time.deltaTime;
            if (temporizadorGhosting <= 0)
            {
                // Si el jugador sostuvo la nueva dirección por más de 50ms, la aceptamos como real
                direccionMirando = direccionPendiente;
            }
        }
        else
        {
            temporizadorGhosting = tiempoGracia; // Reiniciamos el temporizador
        }

        // Rotar el pivote hacia la dirección oficial
        if (pivoteArma != null && !estaMuerto)
        {
            float angulo = Mathf.Atan2(direccionMirando.y, direccionMirando.x) * Mathf.Rad2Deg;
            pivoteArma.rotation = Quaternion.Euler(0, 0, angulo);
        }
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return;
        rb.MovePosition(rb.position + direccionMovimiento * velocidadMovimiento * Time.fixedDeltaTime);
    }
}