using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FloatingGameEntityRealist))]
public class Controller : MonoBehaviour
{
    [SerializeField] private float motorForce = 2000f;
    [SerializeField] private float turnForce = 1000f;
    [SerializeField] private float turningResponseTime = 0.1f;
    [SerializeField] private float forwardDrag = 0.1f;
    [SerializeField] private float sidewaysDrag = 2f;

    private Rigidbody rb;
    private FloatingGameEntityRealist floatingEntity;
    private float currentSteerAngle;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        floatingEntity = GetComponent<FloatingGameEntityRealist>();
    }

    private void FixedUpdate()
    {
        ApplyMotorForce();
        ApplySteeringForce();
        ApplyDrag();
    }

    private void ApplyMotorForce()
    {
        rb.AddForceAtPosition(transform.forward * motorForce, transform.position);
    }

    private void ApplySteeringForce()
    {
        float steeringInput = Input.GetAxis("Horizontal");

        // Smooth the steering input
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, steeringInput, turningResponseTime);

        Vector3 steeringForce = transform.right * turnForce * currentSteerAngle;
        rb.AddForceAtPosition(steeringForce, transform.position - transform.forward * 2f);
    }

    private void ApplyDrag()
    {
        Vector3 forwardVelocity = Vector3.Project(rb.velocity, transform.forward);
        Vector3 sidewaysVelocity = Vector3.Project(rb.velocity, transform.right);

        rb.AddForce(-forwardVelocity * forwardDrag);
        rb.AddForce(-sidewaysVelocity * sidewaysDrag);
    }
}