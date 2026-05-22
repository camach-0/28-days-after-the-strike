using UnityEngine;

public class ControladorMelee : ControladorArma
{
    [Header("Configuración Melee")]
    public DatosArmaMelee datosMelee;

    [Tooltip("Define qué capas (Layers) reciben daño para no golpear paredes")]
    public LayerMask capaEnemigos;

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (datosMelee == null) return;

        // Comprobamos si ya pasó el cooldown del golpe anterior
        if (Time.time >= tiempoProximoAtaque)
        {
            EjecutarGolpe(direccionApuntado);

            // Calculamos cuándo podremos dar el siguiente machetazo
            tiempoProximoAtaque = Time.time + datosMelee.cadenciaAtaque;
        }
    }

    private void EjecutarGolpe(Vector2 direccionApuntado)
    {
        if (datosMelee.sonidoAtaque != null)
        {
            GameObject objSonido = PoolManager.Instancia.SolicitarObjeto("EfectoSonido", transform.position, Quaternion.identity);
            if (objSonido != null)
            {
                objSonido.GetComponent<AudioReciclable>().Reproducir(datosMelee.sonidoAtaque);
            }
        }

        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, datosMelee.radioImpacto, capaEnemigos);

        foreach (Collider2D colision in enemigosGolpeados)
        {
            IReceptorDano receptor = colision.GetComponent<IReceptorDano>();

            if (receptor != null)
            {
                receptor.RecibirDano(datosMelee.danoBase, direccionApuntado, datosMelee.fuerzaEmpuje);
                Debug.Log($"¡Machetazo certero a {colision.name}!");
            }
        }
    }

    // --- Herramienta Visual para Unity (Solo se ve en el Editor) ---
    private void OnDrawGizmosSelected()
    {
        if (puntoDisparo == null || datosMelee == null) return;

        // Dibuja un círculo rojo para que acomodes el alcance de tu machete a simple vista
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoDisparo.position, datosMelee.radioImpacto);
    }
}