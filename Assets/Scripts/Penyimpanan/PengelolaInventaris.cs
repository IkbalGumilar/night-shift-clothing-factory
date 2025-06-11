using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class PengelolaInventaris : MonoBehaviour
{
    private string pathJSON;
    private Dictionary<string, Dictionary<string, ItemInventaris>> inventaris;

    public static PengelolaInventaris Instance; // Singleton untuk akses mudah

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        #if UNITY_ANDROID
            pathJSON = "file://" + Application.streamingAssetsPath + "/Json/items.json";
        #elif UNITY_IOS
            pathJSON = "file://" + Application.streamingAssetsPath + "/Json/items.json";
        #else
            pathJSON = Application.streamingAssetsPath + "/Json/items.json";
        #endif

        Debug.Log("Path JSON: " + pathJSON);
        StartCoroutine(MuatInventaris());
    }

    public IEnumerator MuatInventaris()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(pathJSON))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Data JSON diterima: " + json);

                try
                {
                    inventaris = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ItemInventaris>>>(json);
                    Debug.Log("Inventaris berhasil dimuat!");
                }
                catch (JsonException e)
                {
                    Debug.LogError("Gagal deserialisasi JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Gagal memuat JSON: " + request.error);
            }
        }
    }

    public ItemInventaris AmbilItemDariInventaris(int indexKategori, int itemId)
    {
        if (inventaris == null)
        {
            Debug.LogError("Inventaris belum dimuat!");
            return null;
        }

        List<string> kategoriKeys = new List<string>(inventaris.Keys);
        if (indexKategori < 0 || indexKategori >= kategoriKeys.Count)
        {
            Debug.LogError("Index kategori tidak valid!");
            return null;
        }

        string kategoriKey = kategoriKeys[indexKategori];
        Dictionary<string, ItemInventaris> kategoriItems = inventaris[kategoriKey];

        // Cari item berdasarkan ID menggunakan TryGetValue untuk pencarian yang lebih efisien
        ItemInventaris item = kategoriItems.Values.FirstOrDefault(i => i.id == itemId);
        
        if (item != null)
        {
            return item;
        }

        Debug.LogError($"Item dengan ID {itemId} tidak ditemukan dalam kategori {kategoriKey}!");
        return null;
    }


    [System.Serializable]
    public class ItemInventaris
    {
        public int id;
        public string nama;
        public string kategori;
        public string deskripsi;
        public string ikon;
        public string modelPath;
        public bool canBeUsed;
        public bool canBeCombined;
        public List<string> combineWith;
    }
}
