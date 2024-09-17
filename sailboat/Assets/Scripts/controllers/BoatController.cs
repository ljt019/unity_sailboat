using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FloatingGameEntityRealist))]
public class BoatController : MonoBehaviour
{
    [Header("Force Settings")]
    [SerializeField] private float motorForce = 2000f;
    [SerializeField] private float turnForce = 1000f;
    [SerializeField] private float constantForwardForce = 500f; // New constant forward force

    [Header("Drag Settings")]
    [SerializeField] private float forwardDrag = 0.1f;
    [SerializeField] private float sidewaysDrag = 2f;

    [Header("Steering Settings")]
    [SerializeField] private float turningResponseTime = 0.1f;
    [SerializeField] private float potentiometerDeadZone = 0.1f;
    [SerializeField] private int udpPort = 3030;

    private Rigidbody rb;
    private FloatingGameEntityRealist floatingEntity;
    private float currentSteerAngle;
    private UdpReceiver udpReceiver;
    private float potentiometerValue = 0.5f; // Default to middle position

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        floatingEntity = GetComponent<FloatingGameEntityRealist>();

        // Initialize UDP receiver with the specified port
        udpReceiver = new UdpReceiver(udpPort);
    }

    private void OnDestroy()
    {
        // Properly dispose of the UDP receiver to release resources
        udpReceiver?.Dispose();
    }

    private void Update()
    {
        ProcessUdpMessages();
        ProcessLogMessages(); // Optional: Handle log messages
    }

    private void FixedUpdate()
    {
        ApplyMotorForce();
        ApplySteeringForce();
        ApplyDrag();
    }

    private void ProcessUdpMessages()
    {
        ushort[] messages = udpReceiver.GetMessages();
        foreach (ushort adcValue in messages)
        {
            // Map ADC value (256 to 65535) to -1 to 1 range
            potentiometerValue = Mathf.InverseLerp(256, 65535, adcValue) * 2 - 1;
        }
    }

    /// <summary>
    /// Optional: Processes and logs any messages from the UDP receiver.
    /// </summary>
    private void ProcessLogMessages()
    {
        string[] logs = udpReceiver.GetLogMessages();
        foreach (string log in logs)
        {
            Debug.Log($"[UdpReceiver] {log}");
        }
    }

    private void ApplyMotorForce()
    {
        // Apply constant forward force
        rb.AddForceAtPosition(transform.forward * constantForwardForce, transform.position);

        // Apply additional force based on vertical input
        float verticalInput = Input.GetAxis("Vertical");
        rb.AddForceAtPosition(transform.forward * motorForce * verticalInput, transform.position);
    }

    private void ApplySteeringForce()
    {
        float keyboardInput = Input.GetAxis("Horizontal");
        float steeringInput = CombineSteeringInputs(keyboardInput, potentiometerValue);

        // Smooth the steering input
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, steeringInput, turningResponseTime * Time.fixedDeltaTime);

        Vector3 steeringForce = transform.right * turnForce * currentSteerAngle;
        rb.AddForceAtPosition(steeringForce, transform.position - transform.forward * 2f);
    }

    private float CombineSteeringInputs(float keyboardInput, float potentiometerInput)
    {
        // Apply dead zone to potentiometer input
        if (Mathf.Abs(potentiometerInput) < potentiometerDeadZone)
        {
            potentiometerInput = 0f;
        }
        else
        {
            potentiometerInput = Mathf.Sign(potentiometerInput) *
                (Mathf.Abs(potentiometerInput) - potentiometerDeadZone) / (1 - potentiometerDeadZone);
        }

        // Combine inputs, giving priority to keyboard if it's being used
        return Mathf.Abs(keyboardInput) > 0.01f ? keyboardInput : potentiometerInput;
    }

    private void ApplyDrag()
    {
        Vector3 forwardVelocity = Vector3.Project(rb.velocity, transform.forward);
        Vector3 sidewaysVelocity = Vector3.Project(rb.velocity, transform.right);

        rb.AddForce(-forwardVelocity * forwardDrag);
        rb.AddForce(-sidewaysVelocity * sidewaysDrag);
    }
}
