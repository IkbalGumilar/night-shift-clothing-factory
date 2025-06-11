using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class StaminaPemain : MonoBehaviour
{
    [Header("Pengaturan Stamina")]
    public float staminaMaks = 100f;
    public float lajuRegenerasiStamina = 10f;
    public float lajuPengurasanStamina = 25f;
    public float modifierRegenerasiSaatHabis = 0.5f;
    public float ambangCooldownSprint = 30f;
    public float tundaRegenerasiSetelahSprint = 2f;

    [Header("Elemen UI")]
    public Slider barStamina;
    public Image gambarIsian;

    [Header("Pengaturan Warna")]
    public Color warnaStaminaTinggi = Color.blue;
    public Color warnaNormal = Color.green;
    public Color warnaPeringatan = Color.yellow;
    public Color warnaBahaya = Color.red;
    public Color warnaKedipStaminaPenuh1 = Color.cyan;
    public Color warnaKedipStaminaPenuh2 = Color.magenta;

    [Header("Pengaturan Kedip")]
    public float kecepatanKedip = 0.10f;
    
    private float staminaSaatIni;
    private bool sedangSprint = false;
    private bool kelelahan = false;
    private float waktuSprintTerakhir;
    private PergerakanPemain pergerakanPemain;
    private Coroutine coroutineKedip;
    private float staminaSebelumnya = -1f;

    [Header("Pengaturan Audio")]
    public AudioClip suaraStaminaHabis;
    public AudioClip suaraStaminaPulih;
    public AudioSource sumberAudio;

    public event Action<float> SaatStaminaBerubah;

    private enum LevelStamina { Penuh, Tinggi, Sedang, Rendah, Kelelahan }
    
    void Start()
    {
        staminaSaatIni = staminaMaks;
        pergerakanPemain = GetComponent<PergerakanPemain>();
        if (pergerakanPemain == null)
        {
            Debug.LogWarning("PergerakanPemain tidak ditemukan pada objek ini!");
        }
        PerbaruiUIStamina();
    }

    void Update()
    {
        AturStamina();
    }

    private void AturStamina()
    {
        bool bolehRegenerasi = Time.time - waktuSprintTerakhir > tundaRegenerasiSetelahSprint;
        float kecepatanDrain = lajuPengurasanStamina;

        if (pergerakanPemain != null && pergerakanPemain.sedangBungkuk)
        {
            kecepatanDrain *= 0.5f;
        }

        if (sedangSprint && staminaSaatIni > 0 && !kelelahan)
        {
            staminaSaatIni -= kecepatanDrain * Time.deltaTime;
            bolehRegenerasi = false;
            
            if (staminaSaatIni <= 0)
            {
                staminaSaatIni = 0;
                kelelahan = true;
                sedangSprint = false;
                PutarSuara(suaraStaminaHabis);
            }
        }
        else if (bolehRegenerasi)
        {
            float multiplierRegen = kelelahan ? modifierRegenerasiSaatHabis : 1f;
            staminaSaatIni += lajuRegenerasiStamina * multiplierRegen * Time.deltaTime;
            
            if (staminaSaatIni >= ambangCooldownSprint && kelelahan)
            {
                kelelahan = false;
                PutarSuara(suaraStaminaPulih);
            }
        }

        staminaSaatIni = Mathf.Clamp(staminaSaatIni, 0, staminaMaks);
        
        if (Mathf.Abs(staminaSaatIni - staminaSebelumnya) > Mathf.Epsilon)
        {
            PerbaruiUIStamina();
            SaatStaminaBerubah?.Invoke(staminaSaatIni);
            staminaSebelumnya = staminaSaatIni;
        }
    }

    private void PerbaruiUIStamina()
    {
        bool tampilUI = staminaSaatIni < staminaMaks;
        if (barStamina != null)
        {
            barStamina.gameObject.SetActive(tampilUI);
            barStamina.value = staminaSaatIni;
            barStamina.maxValue = staminaMaks;
        }

        if (!tampilUI) return;

        if (gambarIsian != null)
        {
            gambarIsian.color = DapatkanWarnaStamina();
            AturKedip();
        }
    }

    private Color DapatkanWarnaStamina()
    {
        if (staminaSaatIni > 90f) return warnaStaminaTinggi;
        if (staminaSaatIni < 20f) return warnaBahaya;
        if (staminaSaatIni < 50f) return warnaPeringatan;
        return warnaNormal;
    }

    private void AturKedip()
    {
        if (staminaSaatIni < 15f && coroutineKedip == null)
        {
            coroutineKedip = StartCoroutine(KedipStamina());
        }
        else if (staminaSaatIni >= 15f && coroutineKedip != null)
        {
            StopCoroutine(coroutineKedip);
            coroutineKedip = null;
            gambarIsian.enabled = true;
        }
    }

    private IEnumerator KedipStamina()
    {
        while (staminaSaatIni < 15f)
        {
            gambarIsian.enabled = !gambarIsian.enabled;
            yield return new WaitForSeconds(kecepatanKedip);
        }
        gambarIsian.enabled = true;
        coroutineKedip = null;
    }

    private void PutarSuara(AudioClip clip)
    {
        if (clip != null && sumberAudio != null && (!sumberAudio.isPlaying || sumberAudio.clip != clip))
        {
            sumberAudio.clip = clip;
            sumberAudio.Play();
        }
    }

    public void SetSprint(bool sprinting)
    {
        if (sprinting && kelelahan)
        {
            sedangSprint = false;
        }
        else if (sprinting && staminaSaatIni > 0)
        {
            sedangSprint = true;
            waktuSprintTerakhir = Time.time;
        }
        else
        {
            sedangSprint = false;
        }
    }

    public bool DapatSprint() => !kelelahan && staminaSaatIni > 0;
    public float DapatkanStamina() => staminaSaatIni;

    public void TambahStamina(float jumlah)
    {
        staminaSaatIni = Mathf.Clamp(staminaSaatIni + jumlah, 0, staminaMaks);
        PerbaruiUIStamina();
        SaatStaminaBerubah?.Invoke(staminaSaatIni);
    }

    public float DapatkanModifierKecepatan()
    {
        float persentaseStamina = (staminaSaatIni / staminaMaks) * 100;
        
        if (persentaseStamina == 0) return 0.25f;
        if (persentaseStamina < 20f) return 0.5f;
        if (persentaseStamina < 50f) return 0.75f;
        if (persentaseStamina < 90f) return 0.9f;  
        return 1f;
    }
}
