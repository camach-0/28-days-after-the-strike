using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ¡NUEVO! Necesario para controlar las barras de vida

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
    private float tiempoGracia = 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public override void Start()
    {
        base.Start();

        // Notificamos al GameManager que este superviviente ya está en el mapa
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarJugador(this);
        }

        // Inicializamos la barra de vida al 100%
        if (barraDeVidaUI != null)
        {
            barraDeVidaUI.fillAmount = 1f;
        }
    }

    

    public void OnMover(InputValue valor)
    {
        Vector2 inputBruto = valor.Get<Vector2>();

        if (inputBruto.magnitude > 0.15f)
        {
            direccionMovimiento = inputBruto;
            Vector2 nuevaDir = new Vector2(Mathf.Round(inputBruto.x), Mathf.Round(inputBruto.y)).normalized;
            if (nuevaDir != Vector2.zero)
            {
                direccionPendiente = nuevaDir;
            }
        }
        else
        {
            direccionMovimiento = Vector2.zero;
            direccionPendiente = direccionMirando;
        }
    }

    public void OnDisparar(InputValue valor)
    {
        if (valor.isPressed && !estaMuerto && balaPrefab != null && puntoDisparo != null)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);
            nuevaBala.GetComponent<Bala>().ConfigurarDireccion(direccionMirando);
        }
    }

    private void Update()
    {
        // El Update solo corre para los Humanos. Los Bots usan AliadoBotController.
        if (direccionMirando != direccionPendiente)
        {
            temporizadorGhosting -= Time.deltaTime;
            if (temporizadorGhosting <= 0)
            {
                direccionMirando = direccionPendiente;
            }
        }
        else
        {
            temporizadorGhosting = tiempoGracia;
        }

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

    public override void Morir()
    {
        // 1. Marcar como muerto internamente
        base.Morir();

        Debug.Log(gameObject.name + " ha muerto.");

        // 2. Avisamos al GameManager para ver si todos perdieron
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.VerificarEstadoJugadores();
        }

        // 3. Soltamos la cámara si nos estaba siguiendo
        if (Camera.main != null && Camera.main.transform.parent == this.transform)
        {
            Camera.main.transform.SetParent(null);
        }

        // 4. El personaje desaparece
        gameObject.SetActive(false);
    }
}