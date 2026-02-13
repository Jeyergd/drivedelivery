using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IsometricCarController : MonoBehaviour
{
    [Header("Movimentação")]
    public float acceleration = 40f;
    public float steeringSpeed = 180f;
    public float maxSpeed = 25f;
    [Range(0, 1)] public float driftFactor = 0.95f;

    [Header("Física e Rampas")]
    public float alignmentSpeed = 8f;   // Suavidade da inclinação
    public float rayDistance = 1.8f;    // Tamanho do sensor de chão
    public float downforce = 40f;       // Empuxo para baixo (grudar no chão)
    public float extraGravity = 25f;    // Gravidade estilo arcade
    public LayerMask groundLayer;       // Layer do chão e rampas

    private Rigidbody rb;
    private float moveInput;
    private float steerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Configurações ideais de Rigidbody para carro arcade
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Baixa o centro de massa para evitar tombamentos
        rb.centerOfMass = new Vector3(0, -0.7f, 0);
    }

    void Update()
    {
        // Captura de Inputs
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        ApplyEngineForce();
        ApplySteering();
        ApplyDrift();
        AlignAndStabilize();
        ApplyDownforce();
    }

    void ApplyEngineForce()
    {
        if (rb.linearVelocity.magnitude > maxSpeed) return;

        Vector3 forceDirection = transform.forward;
        RaycastHit hit;

        // Projeta a força paralelamente ao chão da rampa
        if (Physics.Raycast(transform.position, -transform.up, out hit, rayDistance, groundLayer))
        {
            forceDirection = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        }

        rb.AddForce(forceDirection * moveInput * acceleration, ForceMode.Acceleration);
    }

    void ApplySteering()
    {
        // Só vira se houver movimento mínimo
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            float turnMultiplier = (moveInput < 0) ? -1f : 1f;
            float rotation = steerInput * steeringSpeed * turnMultiplier * Time.fixedDeltaTime;

            // Aplica a rotação apenas no eixo Y local para não conflitar com a rampa
            Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    void ApplyDrift()
    {
        // Filtra a velocidade lateral para dar aderência
        Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 rightVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        rb.linearVelocity = forwardVel + (rightVel * driftFactor);
    }

    void AlignAndStabilize()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, rayDistance, groundLayer))
        {
            // ALINHADO AO CHÃO: Copia a inclinação da rampa
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * alignmentSpeed);
        }
        else
        {
            // NO AR: Tenta manter o carro nivelado com o horizonte (evita loops)
            Quaternion levelRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, levelRotation, Time.fixedDeltaTime * 2f);
        }
    }

    void ApplyDownforce()
    {
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, -transform.up, out hit, rayDistance, groundLayer);

        if (isGrounded)
        {
            // No chão: Aplica o downforce normal baseado na velocidade para dar aderência
            rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude, ForceMode.Force);
        }
        else
        {
            // NO AR: Se o carro decolou, aplicamos uma gravidade "falsa" muito mais forte
            // Isso faz o carro cair como um tijolo, eliminando o efeito de "flutuar"
            rb.AddForce(Vector3.down * (extraGravity * 3f), ForceMode.Acceleration);

            // Opcional: Empurra o carro um pouco para baixo se ele estiver subindo rápido demais no ar
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
            }
        }
    }

    // Visualização do sensor no Editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -transform.up * rayDistance);
    }
}