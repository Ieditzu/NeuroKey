using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 120f;

    private void Awake()
    {
        EnsureTrigger();
    }

    private void Reset()
    {
        EnsureTrigger();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BeanController>() != null || other.GetComponent<CharacterController>() != null)
        {
            gameObject.SetActive(false);
        }
    }

    private void EnsureTrigger()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = true;
        }
    }
}
