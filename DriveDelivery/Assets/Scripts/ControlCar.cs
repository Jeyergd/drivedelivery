using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IsometricCarController : MonoBehaviour
{
    [Header("Configura��es do Carro")]
    public float velocidadeMaxima = 1500f;
    public float aceleracao = 100f;
    public float velocidadeRotacao = 100f;

    [Header("F�sica")]
    public float forcaDrift = 2f; // Controla a derrapagem lateral

    private Rigidbody rb;
    private float inputVertical;
    private float inputHorizontal;
    private Vector3 velocidadeAtual;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Baixa o centro de massa para o carro ser mais est�vel
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        // Pega inputs (W/S ou Setas, A/D ou Setas)
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        MoverCarro();
        RotacionarCarro();
        AplicarAtritoLateral();
    }

    void MoverCarro()
    {
        // Move para frente/tr�s baseado na dire��o atual
        Vector3 forcaMovimento = transform.forward * inputVertical * aceleracao;
        rb.AddForce(forcaMovimento, ForceMode.Acceleration);

        // Limita a velocidade m�xima
        if (rb.linearVelocity.magnitude > velocidadeMaxima)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * velocidadeMaxima;
        }
    }

    void RotacionarCarro()
    {
        // S� rotaciona se o carro estiver se movendo
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float direcao = inputHorizontal * velocidadeRotacao * Time.deltaTime;
            // Inverte a rota��o se estiver dando r�
            if (inputVertical < 0) direcao *= -1;

            transform.Rotate(0, direcao, 0);
        }
    }

    void AplicarAtritoLateral()
    {
        // Cria a derrapagem (impede que o carro deslize de lado como sabonete)
        Vector3 velocidadeLateral = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-velocidadeLateral * forcaDrift, ForceMode.Acceleration);
    }
}