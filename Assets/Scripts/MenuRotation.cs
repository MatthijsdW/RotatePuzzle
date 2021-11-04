using UnityEngine;

public class MenuRotation : MonoBehaviour
{
    public float rotationSpeed;

    void Update()
    {
        this.transform.Rotate(Vector3.up * rotationSpeed);
    }
}
