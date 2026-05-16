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
        // 1. Reproducir el sonido del hachazo o "swoosh"
        if (datosMelee.sonidoAtaque != null)
        {
            AudioSource.PlayClipAtPoint(datosMelee.sonidoAtaque, transform.position);
        }

        // 2. Crear un "círculo invisible" en la punta del arma y capturar todo lo que toque
        // Nota: Usamos 'puntoDisparo' que heredaste de ControladorArma como centro del golpe
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoDisparo.position, datosMelee.radioImpacto, capaEnemigos);

        // 3. Revisar cada cosa que golpeamos y aplicarle daño
        foreach (Collider2D colision in enemigosGolpeados)
        {
            // Buscamos tu nueva Interfaz en el objeto golpeado
            IReceptorDano receptor = colision.GetComponent<IReceptorDano>();

            if (receptor != null)
            {
                // ¡Magia! Le pasamos el daño, hacia dónde lo empujamos, y con qué fuerza
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