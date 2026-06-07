using UnityEngine;

[CreateAssetMenu(fileName = "NuevosDatosArmaFuego", menuName = "Armas/Datos Arma Fuego")]
public class DatosArmaFuego : DatosArma
{
    [Header("Tipo de Disparo")]
    public bool esAutomatica;
    public bool esRafaga;
    public int balasPorRafaga = 3;
    public float tiempoEntreBalasRafaga = 0.05f;

    [Header("Munición")]
    public int municionMaxima;
    public int tamanoCargador;
    public float tiempoRecarga;
    [Tooltip("Activa esto para la pistola inicial. El cargador se vacía, pero la reserva es infinita.")]
    public bool reservaInfinita = false;

    [Header("Horda y Balística")]
    [Tooltip("¿A cuántos zombis atraviesa antes de destruirse? (Pistola = 1, Sniper = 5)")]
    public int penetracionZombis = 1;
    public float fuerzaEmpuje = 2f;
    public bool causaTambaleo = false;
    [Tooltip("Distancia máxima que recorre la bala antes de desaparecer")]
    public float alcance = 20f; // <-- ¡AQUÍ ESTÁ EL ALCANCE!

    [Header("Precisión Dinámica (Cono de Disparo)")]
    [Tooltip("Dispersión cuando estás quieto")]
    public float dispersionMinima = 1f;
    [Tooltip("Dispersión cuando corres")]
    public float dispersionMaxima = 8f;
    [Tooltip("Cuánto crece el cono por cada bala disparada (Recoil)")]
    public float incrementoRecoil = 2f;
    [Tooltip("Qué tan rápido se cierra el cono al dejar de disparar")]
    public float velocidadRecuperacion = 10f;

    [Header("Peso y Manejo")]
    [Tooltip("Tiempo en segundos antes de poder disparar al equiparla")]
    public float tiempoDespliegue = 0.5f;
    [Range(0.1f, 1f)]
    [Tooltip("1 = Velocidad normal. 0.5 = Te frena a la mitad (Ej: Ametralladora pesada)")]
    public float modificadorVelocidad = 1f;

    [Header("Culatazo / Empujón (¡NUEVO!)")]
    public float alcanceEmpujon = 1.5f;
    public float cadenciaEmpujon = 1f;
    public float fuerzaDelCulatazo = 10f; // El empuje puro para alejar zombis

    [Header("Escopetas")]
    public int perdigonesPorDisparo = 1;
}