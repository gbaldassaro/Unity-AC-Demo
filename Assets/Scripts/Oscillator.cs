using UnityEngine;

public class Oscillator : MonoBehaviour
{
    public float speed = 5f;
    public float magnitude = 10f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * magnitude;
        transform.position = startPos + new Vector3(x, 0, 0);
    }
}
