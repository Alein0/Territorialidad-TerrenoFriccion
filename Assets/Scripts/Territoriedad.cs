using UnityEngine;

[RequireComponent(typeof(Predator))] //hace que el gameobgect tenga la clase predator
public class Territoriedad : MonoBehaviour
{
    private Vector3 territoryCenter;
    public float territoryRadius = 6f;

    private Predator predator;// para controlar el script predator

    private void Start()
    {
        predator = GetComponent<Predator>(); //obtiene el componente predator
        territoryCenter = transform.position; //la posicion inicial se vuelve centro del territorio
        predator.SetTerritoryLimits(territoryCenter, territoryRadius);// Le comunica al Predator los límites para que SelectNewDestination los respete
    }

    private void Update()
    {
        if (!predator.isAlive) return; // Se ejecuta cada frame, Si está muerto no hace nada, Si está vivo revisa su territorio

        CheckTerritory();
    }

    void CheckTerritory()
    {
        float dist = Vector3.Distance(transform.position, territoryCenter);

        // Si el depredador salió del territorio (puede ocurrir durante una persecución)
        if (dist > territoryRadius)
        {
            // Fuerza volver a explorar: SelectNewDestination ya elegirá un punto dentro del radio
            predator.currentState = PredatorState.Exploring;

            // Empujón suave de regreso al centro para que no quede flotando en el borde
            Vector3 dir = (territoryCenter - transform.position).normalized;
            transform.position += dir * predator.speed * Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(territoryCenter, territoryRadius);
    }
}