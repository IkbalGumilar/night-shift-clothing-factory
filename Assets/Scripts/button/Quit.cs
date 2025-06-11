using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        // Jika dijalankan di editor Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Jika dijalankan sebagai build game
        Application.Quit();
        #endif
    }
}
