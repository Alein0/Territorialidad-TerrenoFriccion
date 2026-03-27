using UnityEngine;
using System.Collections.Generic;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject foodPrefab;
    public float spawnInterval = 2f;
    public int maxFood = 50;

    [Header("Spawn Area (Rectangular)")]
    public Vector2 areaSize = new Vector2(20, 20);

    [Header("Zone status")] // Probabilidad para que el terreno sea fertil, árido o normal
    public float fertileZoneChance = 0.7f;
    public float aridZoneChance = 0.2f;
    public float normalZoneChance = 0.1f;

    private float time = 0f;

    // Lista en donde se guardan todas las posibles zonas
    private List<Zone> allZones = new List<Zone>();

    //Variable en la que se guarda la zona encontrada
    private Zone foundZone;

    private void Start()
    {
        // Cuando la simulación empieze, revisará toda el área para saber que terreno tiene y lo guardará
        allZones.AddRange(FindObjectsByType<Zone>(FindObjectsSortMode.InstanceID));
    }

    public void Simulate(float h)
    {
        time += h;

        if (time >= spawnInterval)
        {
            time = 0f;

            if (CountFood() < maxFood) // Si hay menos de 50 comidas...
            {
                SpawnFoodDependZone(); // Te dirigue al método de generador de comida segun la zona
                //SpawnFood();
            }
        }
    }

    void SpawnFoodDependZone() // GENERADOR DE COMIDA SEGÚN LA ZONA
    {
        Vector2 spawnPos = new Vector2( // Crear una variable para guardar la posición
            // Elige un número al azar para la posición de la comida
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
        );

        spawnPos += (Vector2)transform.position; // Suma la posición del generador de comida a la posición aleatoria

        GetZoneAtPosition(spawnPos); // Se dirigue al método para detectar la zona

        float probability = normalZoneChance; // Mientras tanto, por defecto será determinada como zona normal (Está en el 10%)

        // Luego, al tener el dato de spawnpos, se corroboran los siguientes aspectos:
        // Primero, si la posición está en alguna zona especial
        if (foundZone != null)
        {
            // Segundo, Si la zona encontrada está señalada como fertil...
            if (foundZone.zonetype == Zone.ZoneType.Fertile)
            {
                probability = fertileZoneChance; // Entonces es señalada como probabilidad del 70%
            } 
            else if (foundZone.zonetype == Zone.ZoneType.Arid)// Si no, entonces Si la zona encontrada está señalada como arida...
            {
                probability = aridZoneChance; // Entonces es señalada como probabilidad del 20%
            }
        }

        if (Random.value <= probability) // Para saber si esa probabilidad se va a cumplir, se realiza este método para determinar si en esa zona aparecerá comida o no
        { 
            Instantiate(foodPrefab, spawnPos, Quaternion.identity); // Si el valor da dentro del porcentaje, entonces aparecerá comida
        } //Si no, no aparece nada en este intento
    }

    void GetZoneAtPosition(Vector2 position) // Verifica si la posición está dentro de alguna zona especial (arida o fertil)
    {
        foundZone = null; // Inicialmente esta zona no se encuentra registrada

        foreach (Zone zone in allZones) // Revisa cada zona guardada en la lista
        {
            Collider2D collider = zone.GetComponent<Collider2D>(); //Obtienes el collider de la zona

            if (collider != null && collider.OverlapPoint(position)) // Verifica, si tiene collider e indica que tipo de zona es, entonces...
            {
                foundZone = zone; // Guarda la zona encontrada
                return; // Salir del método ya que se tiene la información que necesitamos
            }
        }
    }

    int CountFood()
    {
        return FindObjectsByType<Food>(FindObjectsSortMode.InstanceID).Length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1));
    }
}
