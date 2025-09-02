using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float speed = 200f;

    void Update()
    {
        // Rotate around Z axis (like a loading wheel)
        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }
}
