using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
        {
            // Z eksenini koruyarak sadece X ve Y'de takip eder
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}