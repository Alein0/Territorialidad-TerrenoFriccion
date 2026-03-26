using UnityEngine;

//Hacemos lo mismo de crear estados como en los conejos y los zorros, pero acá es con respecto a las estaciones del año
public enum Season
{
    Spring, //Primavera
    Summer, //Verano
    Autumn, //Otoño
    Winter //Invierno
}

public class FoodSpawner : MonoBehaviour
{

    //Configuración de apareción de la comida en el escenario
    [Header("Spawner Settings")]
    public GameObject foodPrefab; //El sprite de la comida que va a aparecer en el escenario
    public float spawnInterval = 0.5f; //Cada cuántos segundos en el tiempo va a aparecer comida en el escenario
    public int maxFood = 50; //Cantidad máxima de comida que puede haber en el escenario

    //Configuración de las estaciones
    [Header("Season Settings")]
    public Season currentSeason = Season.Spring; //Iniciamos en primavera
    public float secondsPerSeason = 10f; //Cada una dura 15 segundos
    private float seasonTimer = 0f; //Iniciamos en ceros
    private float currentInterval; //Intervalo que se usara con el tiempo real


    //Área en el que puede aparecer la comida
    [Header("Spawn Area (Rectangular)")]
    public Vector2 areaSize = new Vector2(20, 20); //Tamaño del área
    private float time = 0f;

    private void Start()
    {
        currentInterval = spawnInterval; //Se inicia el intervalo de las estaciones con el que viene la simulacion
    }

    public void Simulate(float h)
    {
        //Esta parte sería de los cambios de estación
        seasonTimer += h;
        if (seasonTimer >= secondsPerSeason) //Si se cumple que ya paso el tiempo de cada estacion
        {
            seasonTimer = 0; //Se reinicia el contador para que cuente de nuevo en la próxima estación
            currentSeason = (Season)(((int)currentSeason + 1) % 4); //Como en el enum cada estación es un número (desde 0 hasta 3), entonces lo que le hace es sumarle 1 para pasar a la siguiente, y el módulo 4 hace que vuelva a empezar en primavera después de terminar en la estación invierno
            Debug.Log($"Estación actual: " + currentSeason); //Imprime en consola la estación actual
            UpdateInterval(); //Actualiza el intervalo de aparición de comida dependiendo de la estación actual
        }

        //Esta parte sería de la aparición de comida en el escenario
        time += h;
        if (time >= currentInterval)
        {
            time = 0f;
            if (CountFood() < maxFood)
            {
                SpawnFood();
            }
        }
    }

    //Acá configuramos qué tanto dura cada una de las estaciones
    void UpdateInterval()
    {
        switch (currentSeason)
        {
            case Season.Spring: //Si es primavera, dura un tiempo normal
                currentInterval = spawnInterval;
                break;
            case Season.Summer:
                currentInterval = spawnInterval * 2f; //Si es verano, se demora 2 veces más de lo normal en salir comida
                break;
            case Season.Autumn:
                currentInterval = spawnInterval * 3f; //Si es otoño, se demora 3 veces más de lo normal en salir comida
                break;
            case Season.Winter:
                currentInterval = spawnInterval * 4f; //Si es invierno, se demora 4 veces más de lo normal en salir comida
                break;
        }
    }

    void SpawnFood()
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
        );

        spawnPos += (Vector2)transform.position;

        Instantiate(foodPrefab, spawnPos, Quaternion.identity); 
    }

    int CountFood()
    {
        return FindObjectsByType<Food>(FindObjectsSortMode.InstanceID).Length;
    }

    private void OnDrawGizmosSelected() //Acá hace lo mismo que tiene el zorro y el conejo para saber el área en el que puede aparecer la comida
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 1));
    }
}
