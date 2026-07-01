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

    public float speed = 5f;
    public float magnitude = 10f;
    private Vector3 startPos;

    void Awake()
    {
        lastPosition = transform.position;

        startPos = transform.position;
    }
    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * magnitude;
        transform.position = startPos + new Vector3(x, 0, 0);
        velocitySendToPlayer = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}
