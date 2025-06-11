using UnityEngine;
using System.Collections;

public class ItemObject : MonoBehaviour
{
    [Header("Memuat")]
    public GameObject TampilanMemuat;
    public CanvasGroup canvasGroup;
    public float DurasiPudar = 1f;

    [Header("Index Item ")]
    public int indexKategori;
    public int itemId;

    [Header("Data Item ")]
    public string nama;
    public string kategori;
    [TextArea] public string deskripsi;
    public Sprite ikon; 

    [Header("Data Tambahan")]
    public string modelPath;
    public bool canBeUsed;
    public bool canBeCombined;
    public string[] combineWith;

    void Start()
    {
        StartCoroutine(TungguInventaris());
    }
    private IEnumerator TungguInventaris()
    {
        TampilanMemuat.SetActive(true);  
        canvasGroup.alpha = 1;
        yield return StartCoroutine(PengelolaInventaris.Instance.MuatInventaris());
        AmbilDataItem();
        yield return StartCoroutine(TampilanMemuatPudar());
        TampilanMemuat.SetActive(false);
    }
    private IEnumerator TampilanMemuatPudar()
    {
        float startAlpha = canvasGroup.alpha;
        float timeElapsed = 0f;
        while (timeElapsed < DurasiPudar)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, timeElapsed / DurasiPudar);
            yield return null;
        }


        canvasGroup.alpha = 0;
    }


    void AmbilDataItem()
    {
        if (PengelolaInventaris.Instance == null)
        {
            Debug.LogError("PengelolaInventaris belum ada di scene!");
            return;
        }

        PengelolaInventaris.ItemInventaris item = PengelolaInventaris.Instance.AmbilItemDariInventaris(indexKategori, itemId);

        if (item != null)
        {
            nama = item.nama;
            kategori = item.kategori;
            deskripsi = item.deskripsi;
            modelPath = item.modelPath;
            canBeUsed = item.canBeUsed;
            canBeCombined = item.canBeCombined;
            combineWith = item.combineWith.ToArray();
            ikon = Resources.Load<Sprite>(item.ikon.Replace("Icons/", "Sprites/"));

            Debug.Log($"Item ditemukan: {nama} dari kategori {kategori}");
        }
    }
}
