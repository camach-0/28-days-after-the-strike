using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Bala : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [Tooltip("Debe ser el mismo nombre que pusiste en el PoolManager")]
    public string etiquetaPool = "BalaBase";

    [Header("Atributos Físicos")]
    public float velocidad = 20f;

    [HideInInspector] public int dano;
    [HideInInspector] public float fuerzaEmpuje;
    [HideInInspector] public int penetracionRestante;
    [HideInInspector] public int idAtacante = -1;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // ========================================================
    // NUEVO CICLO DE VIDA (Mecánica clave para objetos en Pool)
    // ========================================================
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
       
        StopAllCoroutines();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // ========================================================
    // PUENTE PARA EL BOT (¡ESTO SOLUCIONA EL ÚLTIMO ERROR!)
    // ========================================================
    public void ConfigurarDireccion(Vector2 direccion)
    {

        ConfigurarBala(direccion, 10, 0.5f, 1, 15f, -1);
    }

    // ========================================================
    // Función unificada que recibe toda la balística del jugador
    // ========================================================
    public void ConfigurarBala(Vector2 direccion, int danoArma, float empuje, int penetracion, float alcanceMaximo, int idAtacanteFinal = -1)
    {
        if (rb != null)
        {
            rb.linearVelocity = direccion.normalized * velocidad;
        }

       
        dano = danoArma;
        fuerzaEmpuje = empuje;
        penetracionRestante = penetracion;
        idAtacante = idAtacanteFinal;


        float tiempoDeVida = alcanceMaximo / velocidad;

        StopAllCoroutines(); 
        StartCoroutine(DesactivarTrasTiempo(tiempoDeVida));
    }

    IEnumerator DesactivarTrasTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D colision)
    {
        
        if (colision.CompareTag("Player") || colision.CompareTag("Bala") || colision.CompareTag("Recogible")) return;

     
        IReceptorDano[] receptores = colision.GetComponentsInParent<IReceptorDano>();

        if (receptores.Length > 0)
        {
            foreach (IReceptorDano receptor in receptores)
            {
                
                Debug.Log($"[BALA] Chocó con {colision.name}. Avisando al script: {receptor.GetType().Name}. Daño enviado: {dano}");

                receptor.RecibirDano(dano, rb.linearVelocity.normalized, fuerzaEmpuje, idAtacante);
            }

         
            penetracionRestante--;
        }
        else
        {

            penetracionRestante = 0;
        }

  
        if (penetracionRestante <= 0)
        {
            PoolManager.Instancia.DevolverObjeto(etiquetaPool, gameObject);
        }
    }
}