using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float smoothTime = 0.12f;

    private Vector3 offset;
    private Vector3 velocity;
    private Quaternion fixedRotation;

    private void Start()
    {
        if (target == null)
        {
            return;
        }

        offset = transform.position - target.position;
        fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.rotation = fixedRotation;
    }
}
