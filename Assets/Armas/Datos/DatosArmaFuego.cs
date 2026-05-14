using UnityEngine;


[CreateAssetMenu(fileName = "NuevaArmaFuego", menuName = "Left4Dead2D/Armas/Arma de Fuego")]
public class DatosArmaFuego : DatosArma
{
    [Header("Munición")]
    public int municionMaxima = 360;
    public int tamanoCargador = 30;
    public float tiempoRecarga = 2f;

    [Header("Disparo")]
    public float alcance = 20f;
    public float dispersionBalas = 2f; 
    public int perdigonesPorDisparo = 1; 
}