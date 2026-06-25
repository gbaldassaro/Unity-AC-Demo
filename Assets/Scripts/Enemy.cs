using UnityEngine;

public enum EnemyState
{
    Searching,
    Engaged
}

public enum FightState
{
    Orbit,
    Chase
}

public class Enemy : MonoBehaviour
{
    [HideInInspector] public Vector3 velocitySendToPlayer;
    [HideInInspector] public Vector3 lastPosition;

    void Awake()
    {
        lastPosition = transform.position;
    }
    void Update()
    {
        velocitySendToPlayer = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}
