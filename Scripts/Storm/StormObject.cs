using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObject : MonoBehaviour
{
    [SerializeField] private float initialDistance;
    [SerializeField] private float shrinkSmoothness;

    private float targetDistance;

    // Start is called before the first frame update
    void Start()
    {
        targetDistance = initialDistance;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = transform.position.normalized;
        Vector3 targetPosition = direction * targetDistance;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * shrinkSmoothness);
        Debug.Log("TAGETDIUSTANCE " + targetPosition + "Direction " + direction);
    }

    public void MoveToDistance(float distance)
    {
        targetDistance = distance;
    }

    private void OnTriggerStay(Collider otherCollider)
    {
        if (otherCollider.GetComponent<Player>() != null)
        {
            otherCollider.GetComponent<Player>().StormDamage();
        }
    }
}
