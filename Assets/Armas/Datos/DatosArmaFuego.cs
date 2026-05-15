using UnityEngine;

[CreateAssetMenu(fileName = "NuevosDatosArmaFuego", menuName = "Armas/Datos Arma Fuego")]
public class DatosArmaFuego : DatosArma // Asumo que hereda de tu clase base DatosArma
{
    [Header("Tipo de Disparo (Estilo L4D)")]
    [Tooltip("Si está marcado, mantienes apretado (Uzi, M16). Si no, haces clic por tiro (Pistola, Escopeta Pump).")]
    public bool esAutomatica;

    [Tooltip("Marca esto para armas que disparan ráfagas (SCAR). IGNORA 'esAutomatica' si esto está marcado.")]
    public bool esRafaga;
    public int balasPorRafaga = 3;
    public float tiempoEntreBalasRafaga = 0.05f;

    [Header("Munición")]
    public int municionMaxima;
    public int tamanoCargador;
    public float tiempoRecarga;

    [Header("Disparo")]
    public float dispersionBalas;
    public int perdigonesPorDisparo = 1; // 1 para rifles, 8 para escopetas
    public float alcance = 20f;
}