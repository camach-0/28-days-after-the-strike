using UnityEngine;

public class ControladorM60 : ControladorArma
{
    [Header("Configuración M60")]
    [Tooltip("Arrastra aquí el DatosArmaFuego de tu M60")]
    public DatosArmaFuego datosM60;

    /*[Tooltip("Punto desde donde salen las balas")]
    public Transform puntoDisparo;*/

    [Tooltip("Nombre de la bala en el PoolManager")]
    public string etiquetaPoolBala = "BalaBase";

    [Tooltip("Nombre del arma en el suelo (para el drop)")]
    public string etiquetaSuelo = "ItemM60";

    private int balasRestantes;
    private InventarioJugador miInventario;

    // Cumplimos el contrato de la clase abstracta
    public override string EtiquetaPoolSuelo => etiquetaSuelo;

    // Si la M60 es pesada, frenará al jugador usando el dato de tu ScriptableObject
    public override float ModificadorVelocidad => datosM60 != null ? datosM60.modificadorVelocidad : 1f;

    private void Awake()
    {
        if (datosM60 != null)
        {
            // La M60 arranca con su cargador al máximo (ej. 150 balas)
            balasRestantes = datosM60.tamanoCargador;
        }
    }

    private void Start()
    {
        miInventario = GetComponentInParent<InventarioJugador>();
    }

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (datosM60 == null) return;

        if (Time.time >= tiempoProximoAtaque && balasRestantes > 0)
        {
            Disparar(direccionApuntado);
            tiempoProximoAtaque = Time.time + datosM60.cadenciaAtaque;
        }
    }

    private void Disparar(Vector2 direccion)
    {
        balasRestantes--;

        // 1. Reproducir sonido de disparo
        if (datosM60.sonidoAtaque != null)
        {
            GameObject objSonido = PoolManager.Instancia.SolicitarObjeto("EfectoSonido", transform.position, Quaternion.identity);
            if (objSonido != null) objSonido.GetComponent<AudioReciclable>().Reproducir(datosM60.sonidoAtaque);
        }

        // 2. Disparar la bala usando el PoolManager
        GameObject balaObj = PoolManager.Instancia.SolicitarObjeto(etiquetaPoolBala, puntoDisparo.position, Quaternion.identity);
        if (balaObj != null)
        {
            Bala scriptBala = balaObj.GetComponent<Bala>();
            if (scriptBala != null)
            {
                scriptBala.ConfigurarBala(
                    direccion,
                    (int)datosM60.danoBase,
                    datosM60.fuerzaEmpuje,
                    datosM60.penetracionZombis,
                    datosM60.alcance
                );
            }
        }

        // 3. ¿Nos quedamos sin balas? ¡Rompemos el arma!
        if (balasRestantes <= 0)
        {
            RomperM60();
        }
    }

    private void RomperM60()
    {
        Debug.Log("<color=red>¡La M60 se quedó sin balas y fue descartada!</color>");

        if (miInventario != null)
        {
            // Vaciamos la ranura principal (Slot 0)
            for (int i = 0; i < miInventario.ranuras.Length; i++)
            {
                if (miInventario.ranuras[i] == this)
                {
                    miInventario.ranuras[i] = null;
                    break;
                }
            }

            // Obligamos al jugador a sacar su arma secundaria (Pistola)
            miInventario.CambiarSlot(1);
        }

        // Destruimos el objeto visual de las manos del jugador
        Destroy(gameObject);
    }

    public override void IntentarEmpujon(Vector2 direccion)
    {
        // Si tienes código de culatazo general, puedes llamarlo aquí
    }
}