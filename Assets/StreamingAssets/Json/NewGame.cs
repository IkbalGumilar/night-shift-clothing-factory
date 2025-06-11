using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameManager : MonoBehaviour
{
    private string playerDataPath;
    private string settingsDataPath;
    private string worldDataPath;

    private void Start()
    {
        // Lokasi file save
        playerDataPath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
        settingsDataPath = Path.Combine(Application.persistentDataPath, "Settings.json");
        worldDataPath = Path.Combine(Application.persistentDataPath, "WorldData.json");
    }

    public void NewGame()
    {
        // Hapus semua data lama
        if (File.Exists(playerDataPath)) File.Delete(playerDataPath);
        if (File.Exists(settingsDataPath)) File.Delete(settingsDataPath);
        if (File.Exists(worldDataPath)) File.Delete(worldDataPath);

        Debug.Log("Memulai game baru...");

        // Mulai ulang dari scene awal (sesuaikan dengan nama scene awal)
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        // Periksa apakah ada save data
        if (File.Exists(playerDataPath))
        {
            Debug.Log("Memuat permainan...");
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("Tidak ada save data, mulai game baru.");
            NewGame();
        }
    }
}
