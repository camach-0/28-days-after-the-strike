using UnityEngine;

public class ControladorMelee : ControladorArma
{
    [Header("Configuración Melee")]
    public DatosArmaMelee datosMelee;

    [Tooltip("Define qué capas (Layers) reciben daño para no golpear paredes")]
    public LayerMask capaEnemigos;

    // Exponemos la etiqueta del suelo
    public override string EtiquetaPoolSuelo => datosMelee != null ? datosMelee.etiquetaPoolSuelo : "";

    public override void IntentarAtaque(Vector2 direccionApuntado)
    {
        if (datosMelee == null) return;

        if (Time.time >= tiempoProximoAtaque)
        {
            EjecutarGolpe(direccionApuntado);
            tiempoProximoAtaque = Time.time + datosMelee.cadenciaAtaque;
        }
    }

    private void EjecutarGolpe(Vector2 direccionApuntado)
    {
        if (datosMelee.sonidoAtaque != null)
        {
            GameObject objSonido = PoolManager.Instancia.SolicitarObjeto("EfectoSonido", transform.position, Quaternion.identity);
            if (objSonido != null) objSonido.GetComponent<AudioReciclable>().Reproducir(datosMelee.sonidoAtaque);
        }

        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, datosMelee.radioImpacto, capaEnemigos);

        foreach (Collider2D colision in enemigosGolpeados)
        {
            // =========================================================
            // ¡EL MISMO BLINDAJE QUE LA BALA! Buscamos en plural y en los padres
            // =========================================================
            IReceptorDano[] receptores = colision.GetComponentsInParent<IReceptorDano>();

            foreach (IReceptorDano receptor in receptores)
            {
                receptor.RecibirDano(datosMelee.danoBase, direccionApuntado, datosMelee.fuerzaEmpuje);
            }
        }
    }

    // El machete no tiene culatazo, pero la estructura exige la función
    public override void IntentarEmpujon(Vector2 direccion) { }

    private void OnDrawGizmosSelected()
    {
        if (puntoDisparo == null || datosMelee == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoDisparo.position, datosMelee.radioImpacto);
    }
}