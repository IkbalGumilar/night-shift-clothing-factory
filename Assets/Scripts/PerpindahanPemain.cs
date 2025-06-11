using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StaminaPemain))]
public class PergerakanPemain : MonoBehaviour
{
    [System.Serializable]
    public class MovementSettings
    {
        public float jalan = 4f;
        public float lari = 7f;
        public float lariJongkok = 5f;
        public float bungkuk = 2f;
        public float lompatTinggi = 0.75f;
        public float mundurModifier = 0.7f;
        public float biayaLompat = 5f;
    }

    [System.Serializable]
    public class CrouchSettings
    {
        public float tinggiNormal = 2f;
        public float tinggiJongkok = 1f;
        public float kecepatanTransisi = 5f;
    }

    [System.Serializable]
    public class PhysicsSettings
    {
        public float massa = 70f;
        public float akselerasi = 10f;
        public float gravityNaik = 9.8f;
        public float gravityTurun = 20f;
        public float damping = 5f;
    }

    [Header("Referensi")]
    [SerializeField] private Camera kamera;
    [SerializeField] private AudioSource audioLangkah;
    [SerializeField] private AudioClip[] langkahClips;
    [SerializeField] private LayerMask kepalaLayer = 0;

    [Header("Head Bobbing")]
    [SerializeField] private float bobFrekuensi = 10f;
    [SerializeField] private float bobAmplitudo = 0.05f;

    [Header("Gerakan")]
    [SerializeField] private MovementSettings movement;
    [SerializeField] private CrouchSettings crouch;
    [SerializeField] private PhysicsSettings physics;
    [SerializeField] private float cooldownLompat = 0.1f;
    [SerializeField] private float jarakLangkah = 2f;

    private PemainInputAksi input;
    private CharacterController controller;
    private StaminaPemain stamina;
    public Vector2 inputGerak;
    private Vector3 velocity;
    private float velocityY;
    private float timerLompat;
    private bool sedangBerlari;
    public bool sedangBungkuk { get; private set; }
    public bool lompatInput;
    private float jarakTempuh;
    private float headBobTimer;
    private Vector3 posisiKameraAwal;
    private bool sudahMulai;

    private void Awake()
    {
        input = new PemainInputAksi();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Pemain.Jalan.performed += ctx => inputGerak = ctx.ReadValue<Vector2>();
        input.Pemain.Jalan.canceled += ctx => inputGerak = Vector2.zero;
        input.Pemain.Menunduk.performed += ctx => ToggleCrouch();
        input.Pemain.Lompat.performed += ctx => lompatInput = true;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = GetComponent<StaminaPemain>();
        controller.enabled = false;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        controller.enabled = true;
        if (kamera != null)
        {
            posisiKameraAwal = kamera.transform.localPosition;
        }
    }

    private void Update()
    {
        if (!sudahMulai)
        {
            sudahMulai = true;
            return;
        }

        if (timerLompat > 0)
        {
            timerLompat -= Time.deltaTime;
        }

        bool diTanah = controller.isGrounded;

        PerbaruiSprint();
        Gerak(diTanah);
        Lompat(diTanah);
        Gravitasi(diTanah);
        TransisiTinggi();
        HeadBob(diTanah);
        lompatInput = false;
    }

    private void Gerak(bool diTanah)
    {
        Vector3 arah = kamera.transform.right * inputGerak.x + kamera.transform.forward * inputGerak.y;
        arah.y = 0;
        arah.Normalize();

        float targetSpeed = movement.jalan;

        if (sedangBungkuk)
            targetSpeed = sedangBerlari ? movement.lariJongkok : movement.bungkuk;
        else if (sedangBerlari)
            targetSpeed = movement.lari;

        if (inputGerak.y < 0)
            targetSpeed *= movement.mundurModifier;

        targetSpeed *= stamina.DapatkanModifierKecepatan();

        Vector3 targetVelocity = arah * targetSpeed;
        Vector3 deltaVelocity = targetVelocity - new Vector3(velocity.x, 0, velocity.z);

        if (targetVelocity.magnitude > 0.1f)
        {
            Vector3 acceleration = Vector3.ClampMagnitude(deltaVelocity, physics.akselerasi * Time.deltaTime);
            velocity.x += acceleration.x;
            velocity.z += acceleration.z;
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * physics.damping);
            velocity.z = Mathf.Lerp(velocity.z, 0, Time.deltaTime * physics.damping);
        }

        if (diTanah && inputGerak.magnitude > 0.1f)
        {
            jarakTempuh += new Vector3(velocity.x, 0, velocity.z).magnitude * Time.deltaTime;
            if (jarakTempuh >= jarakLangkah)
            {
                PlayLangkah();
                jarakTempuh = 0f;
            }
        }

        Vector3 finalVelocity = new Vector3(velocity.x, velocityY, velocity.z);
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void Lompat(bool diTanah)
    {
        if (lompatInput && diTanah && timerLompat <= 0 && stamina.DapatkanStamina() >= movement.biayaLompat)
        {
            velocityY = Mathf.Sqrt(2f * movement.lompatTinggi * physics.gravityNaik);
            stamina.TambahStamina(-movement.biayaLompat);
            timerLompat = cooldownLompat;
        }
    }

    private void Gravitasi(bool diTanah)
    {
        if (!diTanah)
        {
            float gravity = (velocityY > 0) ? physics.gravityNaik : physics.gravityTurun;
            velocityY -= gravity * Time.deltaTime;
        }
        else if (velocityY < 0)
        {
            velocityY = -2f;
        }
    }

    private void ToggleCrouch()
    {
        if (!sedangBungkuk)
        {
            sedangBungkuk = true;
        }
        else
        {
            Vector3 origin = transform.position + new Vector3(0, controller.radius, 0);
            if (!Physics.SphereCast(origin, controller.radius, Vector3.up, out _, crouch.tinggiNormal - crouch.tinggiJongkok, kepalaLayer))
            {
                sedangBungkuk = false;
            }
        }
    }

    private void TransisiTinggi()
    {
        float targetHeight = sedangBungkuk ? crouch.tinggiJongkok : crouch.tinggiNormal;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouch.kecepatanTransisi);
        controller.center = new Vector3(0, controller.height / 2f, 0);
    }

    private void PerbaruiSprint()
    {
        bool maju = inputGerak.y > 0.1f;
        bool bergerak = inputGerak.magnitude > 0.1f;
        bool tombolSprint = input.Pemain.Lari.IsPressed();
        sedangBerlari = bergerak && maju && tombolSprint && stamina.DapatSprint();
        stamina?.SetSprint(sedangBerlari);
    }

    private void HeadBob(bool diTanah)
    {
        if (inputGerak.magnitude > 0.1f && diTanah)
        {
            headBobTimer += Time.deltaTime * bobFrekuensi * (sedangBerlari ? 1.5f : 1f);
            float bobOffset = Mathf.Sin(headBobTimer) * bobAmplitudo;
            kamera.transform.localPosition = posisiKameraAwal + new Vector3(0, bobOffset, 0);
        }
        else
        {
            headBobTimer = 0f;
            kamera.transform.localPosition = Vector3.Lerp(kamera.transform.localPosition, posisiKameraAwal, Time.deltaTime * 5f);
        }
    }

    private void PlayLangkah()
    {
        if (langkahClips.Length > 0)
        {
            AudioClip clip = langkahClips[Random.Range(0, langkahClips.Length)];
            audioLangkah.PlayOneShot(clip);
        }
    }
}
