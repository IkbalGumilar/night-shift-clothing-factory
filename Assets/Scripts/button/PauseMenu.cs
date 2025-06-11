using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Panel Pause Menu
    private bool isPaused = false;
    private PemainInputAksi controls; // Input Actions

    private void Awake()
    {
        controls = new PemainInputAksi();
    }

    private void OnEnable()
    {
        controls.Pemain.Next.performed += ctx => TogglePause();
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Pemain.Next.performed -= ctx => TogglePause();
        controls.Disable();
    }

    private void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // Fungsi ini bisa dipanggil dari UI Button
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Fungsi ini bisa dipanggil dari UI Button
    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
