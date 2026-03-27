using UnityEngine;

public class Bunny : MonoBehaviour
{
    [Header("Bunny Settings")]
    public float energy = 10f;
    public float age = 0f;
    public float maxAge = 20f;
    public float speed = 1f;
    public float visionRange = 5f;
    public float baseSpeed;


    [Header("Sleeping Settings")]
    public float sleepRecoveryRate = 5f;   // energía por segundo
    public float sleepRecoveryAmount = 20f; // cuánto recupera al dormir

    [Header("Bunny States")]
    public bool isAlive = true;
    public BunnyState currentState = BunnyState.Exploring;

    [Header("Mutation")]
    public GameObject bunnyPrefab;

    private Vector3 destination;
    private float h;

    private float sleepTargetEnergy;

    private void Start()
    {
        destination = transform.position;

        //PRUEBA TEMPORAL
       // Bunny[] bunnies = FindObjectsByType<Bunny>(FindObjectsSortMode.None);

        //foreach (var b in bunnies)
        //{
        //    if (b != this)
        //    {
        //        CreateChildWith(b);
        //        break;
        //    }
        //}
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h;

        EvaluateState();

        switch (currentState)
        {
            case BunnyState.Exploring:
                Explore();
                break;

            case BunnyState.SearchingFood:
                SearchFood();
                break;

            case BunnyState.Eating:
                Eat();
                break;

            case BunnyState.Fleeing:
                Flee();
                break;

            case BunnyState.Sleeping:
                Sleep();
                break;
        }

        Move();
        Age();
        CheckState();
    }

    void EvaluateState()
    {
        // 🔵 PRIORIDAD: dormir
        if (energy <= 0f)
        {
            if (currentState != BunnyState.Sleeping)
            {
                currentState = BunnyState.Sleeping;
                sleepTargetEnergy = Mathf.Min(sleepRecoveryAmount);
            }
            return;
        }

        if (currentState == BunnyState.Sleeping)
            return;

        // 🔴 Depredador cerca → huir
        if (PredatorInRange())
        {
            currentState = BunnyState.Fleeing;
            return;
        }

        // 🟢 Buscar comida
        if (energy < 50f)
        {
            Food nearestFood = FindNearestFood();
            if (nearestFood != null)
            {
                currentState = BunnyState.SearchingFood;
                destination = nearestFood.transform.position;
            }
        }

        // 🍽 Comer si está encima
        Collider2D foodHit = Physics2D.OverlapCircle(
            transform.position,
            0.2f,
            LayerMask.GetMask("Food")
        );

        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>();
            if (food != null)
            {
                currentState = BunnyState.Eating;
                return;
            }
        }

        // 🌿 Explorar
        if (currentState != BunnyState.Eating)
        {
            currentState = BunnyState.Exploring;
        }
    }

    void Explore()
    {
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestination();
        }
    }

    void SearchFood()
    {
        Food nearestFood = FindNearestFood();

        if (nearestFood == null)
        {
            currentState = BunnyState.Exploring;
            SelectNewDestination();
            return;
        }

        Vector2 origin = transform.position;
        Vector2 target = nearestFood.transform.position;
        Vector2 dir = target - origin;
        float dist = dir.magnitude;

        if (dist <= 0.001f)
        {
            currentState = BunnyState.Eating;
            return;
        }

        Vector2 dirNorm = dir / dist;

        // 🔴 BLOQUEO: Obstáculos o agua
        RaycastHit2D blockHit = Physics2D.Raycast(
            origin,
            dirNorm,
            dist,
            LayerMask.GetMask("Obstacles", "Water")
        );

        if (blockHit.collider != null)
        {
            // ❌ ignora esa comida
            currentState = BunnyState.Exploring;
            SelectNewDestination();
            return;
        }

        // ✅ camino libre
        destination = nearestFood.transform.position;

        if (Vector3.Distance(transform.position, nearestFood.transform.position) < 0.2f)
        {
            currentState = BunnyState.Eating;
        }
    }

    void Eat()
    {
        Collider2D foodHit = Physics2D.OverlapCircle(
            transform.position,
            0.2f,
            LayerMask.GetMask("Food")
        );

        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>();
            if (food != null)
            {
                energy += food.nutrition;
                energy = Mathf.Min(energy);
                Destroy(food.gameObject);
            }
        }

        currentState = BunnyState.Exploring;
        SelectNewDestination();
    }

    void Flee()
    {
        Vector3 predatorPos = GetNearestPredatorPosition();
        Vector3 fleeDir = (transform.position - predatorPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            fleeDir,
            visionRange,
            LayerMask.GetMask("Obstacles", "Water")
        );

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = (Vector3)hit.point - fleeDir * offset;
        }
        else
        {
            destination = transform.position + fleeDir * visionRange;
        }

        currentState = BunnyState.Exploring;
    }

    void Sleep()
    {
        // 💤 quieto
        energy += sleepRecoveryRate * h;

        if (energy >= sleepTargetEnergy)
        {
            energy = sleepTargetEnergy;
            currentState = BunnyState.Exploring;
            SelectNewDestination();
        }
    }

    void SelectNewDestination()
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
        );

        Vector3 targetPoint = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            visionRange,
            LayerMask.GetMask("Obstacles", "Water")
        );

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
        if (currentState == BunnyState.Sleeping)
            return;

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
        if (age > maxAge)
        {
            isAlive = false;
            Destroy(gameObject);
        }
    }

    //Mutacion
    public void CreateChildWith(Bunny partner)
    {
        GameObject childObj = Instantiate(bunnyPrefab, transform.position, Quaternion.identity);
        Bunny child = childObj.GetComponent<Bunny>();

        BunnyMutation.ApplyMutation(child, this, partner);

        //DEBUG (IMPORTANTE)
        Debug.Log($"Hijo creado -> Speed: {child.speed}, Energy: {child.energy}, Vision: {child.visionRange}");
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

    bool PredatorInRange()
    {
        Collider2D predator = Physics2D.OverlapCircle(
            transform.position,
            visionRange,
            LayerMask.GetMask("Foxes")
        );
        return predator != null;
    }

    Vector3 GetNearestPredatorPosition()
    {
        Collider2D[] predators = Physics2D.OverlapCircleAll(
            transform.position,
            visionRange,
            LayerMask.GetMask("Foxes")
        );

        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var p in predators)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                pos = p.transform.position;
            }
        }

        return pos;
    }

    Food FindNearestFood() // Busca la comida m�s cercana dentro del rango de visi�n, considerando obst�culos
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Food"));  // Busca todos los collider dentro del rango de visi�n
        Debug.Log($"Bunny {name} encontr� {hits.Length} colliders en su rango");
        Food nearest = null; // Inicializa la variable
        float minDist = Mathf.Infinity; // Inicializa la distancia m�nima a infinito

        foreach (Collider2D hit in hits) // Se ejecuta para cada collider encontrado
        {
            Food food = hit.GetComponent<Food>();
            if (food == null) continue;

            Vector3 direction = food.transform.position - transform.position;
            float dist = direction.magnitude;

            // 🔴 IGNORA comida si hay Obstáculo o Agua
            RaycastHit2D blockHit = Physics2D.Raycast(
                transform.position,
                direction.normalized,
                dist,
                LayerMask.GetMask("Obstacles", "Water")
            );

            if (blockHit.collider != null) continue;

            if (dist < minDist)
            {
                minDist = dist;
                nearest = food;
            }
        }
        return nearest; //Retornando la comida m�s cercana que el conejo puede ver, si no hay niguno entonces manda un null

    }
}
