using UnityEngine;

[CreateAssetMenu(fileName = "NuevosDatosMelee", menuName = "Armas/Datos Melee")]
public class DatosArmaMelee : DatosArma
{
    [Header("Área de Impacto")]
    [Tooltip("Tamaño del área de colisión")]
    public float radioImpacto = 1.2f;

    [Tooltip("Fuerza con la que aleja a los zombis")]
    public float fuerzaEmpuje = 5f;

    [Tooltip("Si es motosierra, aplica daño continuo en vez de un solo golpe")]
    public bool esMotosierra = false;
}