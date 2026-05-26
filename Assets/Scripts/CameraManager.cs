using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform target;

    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    private void Start()
    {
        if (target==null && DiceManager.instance!=null)
        {
            target = DiceManager.instance.transform;
        }
        if(target != null)
        {
            if(offset==Vector3.zero)
            {
                offset = transform.position - target.position;
            }
        }
    }

    private void Update()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(target);
    }
}


