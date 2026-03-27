using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("Predator Settings")]
    public float energy = 10;
    public float age = 0;
    public float maxAge = 20;
    public float speed = 1f;
    public float visionRange = 5f;

    //CONDICIÓN FÍSICA DEL DEPREDADOR EN CASO DE CANSANCIO
    [Header("Physical condition settings")]
    public float maxResistancePursue = 10f; // Resistencia máxima para perseguir a su presa
    public float resistanceLostSecondPursue = 2f; // Resistencia perdida por segundo de persecución
    public float resistanceRecoveredWithResting = 2f; // Resistencia recuperada por descansar
    public float PursueDuration = 3f; //Tiempo máximo de persecución
    public float restingDuration = 3f; //Tiempo de descanso después de abandonar

    [Header("Predator States")]
    public bool isAlive = true;
    public PredatorState currentState = PredatorState.Exploring;

    private Vector3 destination;
    private float h; 
    // h es el tiempo que dura cada paso de la simulación, esto viene de SimulationManager.secondsPerIteration =1f;

    // VARIABLES PARA EL CANSANCIO DEL DEPREDADOR
    private float currentResistance; // Resistencia actual del depredador
    private float pursueTimer = 0f; // Tiempo que lleva la persecución (valor inicial)
    private float restingTimer = 0f; // Tiempo que lleva descansando el depredador (valor inicial)
    private bool isResting = false;
    private Bunny currentPrey; // Conejo que esgtá persiguiendo

    private void Start()
    {
        destination = transform.position;

        //Al iniciar la simuación, el depredador estará con la resistencia al máximo
        currentResistance = maxResistancePursue;
    }

    public void Simulate(float h) // h es el tiempo de cada paso (1 segundo)
    {
        if (!isAlive) return;

        this.h = h;

        if (isResting) // Si el depredador está descansando,...
        {
            Rest();
            return;
        }

        switch (currentState)
        {
            case PredatorState.Exploring:
                Explore();
                break;
            case PredatorState.SearchingFood:
                SearchFood();
                break;
            case PredatorState.Eating:
                Eat();
                break;
        }

        Move();
        Age();
        CheckState();
    }

    void Explore()
    {
        // Si hay comida a la vista, cambiar de estado a persecución (caza)
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny != null && isResting == false) // Si detecta un conejo cerca y no está descansando...
        {
            startPersecution(nearestBunny); // Inicia la persecución
            return;
        }

        // Si ya llegó al destino, elegir uno nuevo
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestination();
        }
    }

    // INICIO DE LA PERSECUCIÓN
    void startPersecution(Bunny prey)
    {
        currentPrey = prey; // Se guarda el conejo que va a perseguir el depredador

        pursueTimer = 0f; // Se reinicia el contador de la persecución 
        currentState = PredatorState.SearchingFood; // Se dirigue al estado de buscar comida y ...
        destination = prey.transform.position; // Diriguirse hacia su presa
    }

    void SearchFood()
    {
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny == null)
        {
            // Si no hay comida, volver a explorar
            currentState = PredatorState.Exploring;
            return;
        }

        //Actualizar destino hacia la presa más cercana
        destination = nearestBunny.transform.position;

        // PERSECUCIÓN EN PROCESO
        currentResistance -= resistanceLostSecondPursue * h;  // h es el tiempo de cada paso (1 segundo)
        //Cada segundo que persigue, pierde resistencia
        //Esto también se puede escribir sin la h

        pursueTimer += h; // h es el tiempo de cada paso (1 segundo)
        //Cada segundo que persigue, aumenta el contador de tiempo
        //Esto también se puede escribir como: pursueTimer++

        // VERIFICAR SI DEBE ABANDONAR POR CANSANCIO
        if (currentResistance <=0 || pursueTimer >= PursueDuration) // Si el depredador se quedó sin resistencia (energía) o persiguió al conejo demasiado tiempo, entonces ...
        {
            AbandonPursue(); // Se rinde y abandona esa cacería
            return;
        }

        // Si alcanzó el conejo, pasará al método para comerselo
        if (Vector3.Distance(transform.position, nearestBunny.transform.position) < 0.2f)
        {
            currentState = PredatorState.Eating;
        }
    }

    // ABANDONAR LA PERSECUCIÓN (CACERÍA)
    void AbandonPursue()
    {
        isResting = true; // Pasa a modo descanso
        restingTimer = 0f; // Inicialmente el contador de descanso está en 0
        currentPrey = null; // Olvida completamente al conejo por ahora
        //currentState = PredatorState.Exploring
        Debug.Log("El depredador abandonó la persecución por cansancio");
    }

    //MÉTODO DESCANSO O TIEMPO FUERA DEL DEPREDADOR
    void Rest()
    {
        restingTimer += h; // h es el tiempo de cada paso (1 segundo)
        //El contador del descanso va a aumentar
        //Esto también se puede escribir como: restingTimer++

        currentResistance += resistanceRecoveredWithResting * h; // h es el tiempo de cada paso (1 segundo)
        // Cada segundo que pasa, recupera resistencia
        //Esto también se puede escribir sin la h

        currentResistance = Mathf.Min(currentResistance, maxResistancePursue); // Para evitar que la resistencia supere el máximo

        if (restingTimer >= restingDuration) // Si el depredador ya descansó lo suficiente,...
        {
            isResting = false; // Volverá a estar activo
            Debug.Log("El depredador ha descansado y está listo para cazar");
        }
    }
    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Bunnies"));
        if (foodHit != null)
        {
            Bunny food = foodHit.GetComponent<Bunny>();
            if (food != null)
            {
                energy += food.age;
                Destroy(food.gameObject);
            }
        }

        // Después de comer vuelve a explorar
        currentState = PredatorState.Exploring;
    }

    void Flee()
    {
        SelectNewDestination();
        currentState = PredatorState.Exploring;
    }

    void SelectNewDestination()
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
        );

        Vector3 targetPoint = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, visionRange, LayerMask.GetMask("Obstacles"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)direction.normalized * offset;
        }
        else
        {
            destination = targetPoint;
        }
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
        );

        energy -= speed * h;
    }

    void Age()
    {
        age += h;
    }

    void CheckState()
    {
        if (energy <= 0 || age > maxAge)
        {
            isAlive = false;
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }

    Bunny FindNearestBunny()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Bunnies"));
        Debug.Log($"Predator {name} encontró {hits.Length} colliders en su rango");
        Bunny nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Bunny food = hit.GetComponent<Bunny>();
            if (food != null)
            {
                float dist = Vector2.Distance(transform.position, food.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = food;
                }
            }
        }

        return nearest;
    }
}
