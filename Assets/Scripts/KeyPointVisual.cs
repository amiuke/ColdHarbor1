using UnityEngine;

public class KeyPointVisual : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private bool rotateClockwise = true;

    [Header("Float Settings")]
    [SerializeField] private float floatAmplitude = 0.5f; // How high/low the object moves
    [SerializeField] private float floatFrequency = 1f; // How fast the object floats
    [SerializeField] private float floatOffset = 0f; // Phase offset for the sine wave

    private Vector3 initialPosition;
    private float time;

    private void Start()
    {
        initialPosition = transform.position;
        time = floatOffset;
    }

    private void Update()
    {
        // Rotation around Y-axis
        float rotationDirection = rotateClockwise ? 1f : -1f;
        transform.Rotate(Vector3.up, rotationSpeed * rotationDirection * Time.deltaTime);

        // Floating effect using sine wave
        time += Time.deltaTime * floatFrequency;
        float yOffset = Mathf.Sin(time * 2f * Mathf.PI) * floatAmplitude;
        
        Vector3 newPosition = initialPosition;
        newPosition.y += yOffset;
        transform.position = newPosition;
    }

    // Call this if the initial position changes (e.g., object is moved)
    public void UpdateInitialPosition()
    {
        initialPosition = transform.position;
    }

    // Public methods to adjust visual settings at runtime
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void SetFloatAmplitude(float amplitude)
    {
        floatAmplitude = amplitude;
    }

    public void SetFloatFrequency(float frequency)
    {
        floatFrequency = frequency;
    }
}
