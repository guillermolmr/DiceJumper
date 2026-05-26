using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FitObjectInView : MonoBehaviour
{
    public float padding = 1.1f;

    Camera cam;

    [SerializeField] Transform A;
    [SerializeField] Transform B;

    void Awake() => cam = GetComponent<Camera>();

    void LateUpdate() => FitToObject();

    void FitToObject()
    {
        Vector3 center = (A.position + B.position) / 2f;

        // Mantener la dirección actual de la cámara, solo mover distancia
        Vector3 direction = (transform.position - center).normalized;
        if (direction == Vector3.zero) direction = -transform.forward;

        float halfFovV = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float halfFovH = Mathf.Atan(Mathf.Tan(halfFovV) * cam.aspect);

        // Probamos distancias hasta que A y B quepan en el frustum
        // Lo hacemos analíticamente: para cada punto, calculamos la distancia mínima
        float requiredDist = 0f;

        foreach (Transform point in new[] { A, B })
        {
            // Vector del centro al punto
            Vector3 offset = point.position - center;

            // Proyectamos el offset en el plano perpendicular a la dirección de la cámara
            // para saber cuánto se desvía en X e Y de pantalla
            // Usamos los ejes right/up de la cámara actual
            float projRight = Vector3.Dot(offset, transform.right);
            float projUp = Vector3.Dot(offset, transform.up);

            // Distancia mínima para que este punto quepa horizontalmente
            float distForH = Mathf.Abs(projRight) / Mathf.Tan(halfFovH);
            // Distancia mínima para que quepa verticalmente
            float distForV = Mathf.Abs(projUp) / Mathf.Tan(halfFovV);

            requiredDist = Mathf.Max(requiredDist, distForH, distForV);
        }

        requiredDist *= padding;

        transform.position = center + direction * requiredDist;
        transform.LookAt(center);
    }
}