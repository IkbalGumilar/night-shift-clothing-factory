using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class PandanganPemain : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sliderSensitivitas;
    [SerializeField] private Slider sliderBalikX;
    [SerializeField] private Slider sliderBalikY;
    [SerializeField] private Slider sliderRotasiLangsung;

    [Header("Efek Visual")]
    [SerializeField] private float FOVSprint = 80f;
    [SerializeField] private float sudutMiringSprint = 5f;
    [SerializeField] private PostProcessVolume volumePostProcess;

    [Header("Referensi")]
    [SerializeField] private Transform tubuhPemain;
    [SerializeField] private Camera kameraPemain;
    private PemainInputAksi inputAksi;
    private StaminaPemain staminaPemain;
    private PergerakanPemain pergerakanPemain;
    private DepthOfField dof;

    private Vector2 inputLihat;
    private Vector2 lihatHalus;
    private Vector2 kecepatanLihat;

    private float sensitivitasMouse = 15f;
    private float sensitivitasHalus;
    private float sensitivitasKecepatan;
    private float faktorBalikX = 1f;
    private float faktorBalikY = 1f;

    private float rotasiX = 0f;
    private float rotasiY = 0f;
    private float miringSaatIni = 0f;
    private float kecepatanDOF = 0f;
    private float kecepatanFOV;
    private float kecepatanRotasiY;
    private float FOVDefault;
    private float lastMundurTime = -1f;
    private bool prevMundur = false;
    private bool rotasiLangsung = false;
    private bool isTurning = false;
    private float targetRotasiY;
    private float turnCooldownTimer = 0f;

    private const float doubleTapThreshold = 0.3f;
    private const float turnSmoothSpeed = 10f;
    private const float turnCooldown = 1f;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        tubuhPemain ??= transform.parent;
        kameraPemain ??= Camera.main;
        staminaPemain = tubuhPemain.GetComponent<StaminaPemain>();
        pergerakanPemain = tubuhPemain.GetComponent<PergerakanPemain>();
        inputAksi = new PemainInputAksi();
        inputAksi.Enable();

        FOVDefault = kameraPemain.fieldOfView;

        if (volumePostProcess != null && volumePostProcess.profile.TryGetSettings(out DepthOfField depthOfField))
            dof = depthOfField;

        sensitivitasMouse = PlayerPrefs.GetFloat("SensitivitasMouse", sensitivitasMouse);
        faktorBalikX = PlayerPrefs.GetInt("BalikX", 0) == 1 ? -1f : 1f;
        faktorBalikY = PlayerPrefs.GetInt("BalikY", 0) == 1 ? -1f : 1f;

        if (sliderRotasiLangsung)
        {
            rotasiLangsung = sliderRotasiLangsung.value > 0.5f;
            sliderRotasiLangsung.onValueChanged.AddListener(SetRotasiLangsung);
        }

        if (sliderSensitivitas)
        {
            sliderSensitivitas.value = sensitivitasMouse;
            sliderSensitivitas.onValueChanged.AddListener(SetSensitivitas);
        }

        if (sliderBalikX)
        {
            sliderBalikX.value = faktorBalikX == -1f ? 1 : 0;
            sliderBalikX.onValueChanged.AddListener(SetBalikX);
        }

        if (sliderBalikY)
        {
            sliderBalikY.value = faktorBalikY == -1f ? 1 : 0;
            sliderBalikY.onValueChanged.AddListener(SetBalikY);
        }
    }

    private void Update()
    {
        inputLihat = inputAksi.Pemain.Lihat.ReadValue<Vector2>();

        if (turnCooldownTimer > 0f)
            turnCooldownTimer -= Time.deltaTime;

        Vector2 inputGerak = inputAksi.Pemain.Jalan.ReadValue<Vector2>();
        bool mundurSekarang = inputGerak.y < -0.9f;

        if (mundurSekarang && !prevMundur && turnCooldownTimer <= 0f)
        {
            if (rotasiLangsung || (Time.time - lastMundurTime < doubleTapThreshold))
            {
                targetRotasiY = rotasiY + 180f;
                isTurning = true;
                turnCooldownTimer = turnCooldown;
            }

            lastMundurTime = Time.time;
        }

        if (isTurning)
        {
            rotasiY = Mathf.SmoothDampAngle(rotasiY, targetRotasiY, ref kecepatanRotasiY, 0.1f);
            if (Mathf.Abs(Mathf.DeltaAngle(rotasiY, targetRotasiY)) < 0.5f)
            {
                rotasiY = targetRotasiY;
                isTurning = false;
            }
            tubuhPemain.rotation = Quaternion.Euler(0f, rotasiY, 0f);
        }

        prevMundur = mundurSekarang;
    }

    void LateUpdate()
    {
        AturLihat();
        AturEfekSprint();
        AturPosisiKamera(); 
    }


    private void AturLihat()
    {
        if (inputLihat == Vector2.zero || Time.timeScale == 0 || kameraPemain == null || tubuhPemain == null)
            return;

        lihatHalus = Vector2.SmoothDamp(lihatHalus, inputLihat, ref kecepatanLihat, 0.05f);
        sensitivitasHalus = Mathf.Lerp(sensitivitasHalus, sensitivitasMouse, Time.deltaTime * 10f);

        float mouseX = lihatHalus.x * sensitivitasHalus * Time.unscaledDeltaTime * faktorBalikX;
        float mouseY = lihatHalus.y * sensitivitasHalus * Time.unscaledDeltaTime * faktorBalikY;

        if (!isTurning)
        {
            rotasiY += mouseX;
            rotasiX = Mathf.Clamp(rotasiX - mouseY, -60f, 70f);
            tubuhPemain.rotation = Quaternion.Euler(0f, rotasiY, 0f);
        }

        kameraPemain.transform.localRotation = Quaternion.Euler(rotasiX, 0f, miringSaatIni);
    }
    private void AturPosisiKamera()
    {
        if (pergerakanPemain == null || kameraPemain == null)
            return;

        float tinggiController = pergerakanPemain.GetComponent<CharacterController>().height;
        float targetY = tinggiController / 2f;

        Vector3 posisiKamera = kameraPemain.transform.localPosition;
        posisiKamera.y = Mathf.Lerp(posisiKamera.y, targetY, Time.deltaTime * 10f);
        kameraPemain.transform.localPosition = posisiKamera;
    }


    private void AturEfekSprint()
    {
        if (kameraPemain == null || staminaPemain == null || pergerakanPemain == null)
            return;

        bool sprintInput = inputAksi.Pemain.Lari.ReadValue<float>() > 0.5f;
        bool bergerak = pergerakanPemain.inputGerak.magnitude > 0.1f;
        bool bisaSprint = sprintInput && bergerak && staminaPemain.DapatkanStamina() > 0 && pergerakanPemain.lompatInput;

        float targetFOV = bisaSprint ? FOVSprint : FOVDefault;
        kameraPemain.fieldOfView = Mathf.SmoothDamp(kameraPemain.fieldOfView, targetFOV, ref kecepatanFOV, 0.2f);

        float targetMiring = bisaSprint ? sudutMiringSprint : 0f;
        miringSaatIni = Mathf.Lerp(miringSaatIni, targetMiring, Time.deltaTime * 5f);

        if (dof != null)
        {
            float fokusTarget = bisaSprint ? 1f : 10f;
            dof.focusDistance.value = Mathf.SmoothDamp(dof.focusDistance.value, fokusTarget, ref kecepatanDOF, 0.1f);
        }
    }

    public void SetBalikX(float nilai)
    {
        faktorBalikX = nilai == 1 ? -1f : 1f;
        PlayerPrefs.SetInt("BalikX", nilai == 1 ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetBalikY(float nilai)
    {
        faktorBalikY = nilai == 1 ? -1f : 1f;
        PlayerPrefs.SetInt("BalikY", nilai == 1 ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSensitivitas(float nilai)
    {
        sensitivitasMouse = nilai;
        PlayerPrefs.SetFloat("SensitivitasMouse", sensitivitasMouse);
        PlayerPrefs.Save();
    }

    public void SetRotasiLangsung(float value)
    {
        rotasiLangsung = value > 0.5f;
    }

    private void OnDestroy()
    {
        inputAksi.Disable();
    }
}
