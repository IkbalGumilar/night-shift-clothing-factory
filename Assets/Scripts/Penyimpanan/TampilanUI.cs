using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UIItemDisplay : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject inventoryPanel;
    public ItemObject item;
    public TextMeshProUGUI namaText;
    public TextMeshProUGUI kategoriText;
    public TextMeshProUGUI deskripsiText;
    public RawImage ikonImage;

    [Header("Referensi Kontrol Pemain")]
    public MonoBehaviour mouseLookScript;

    private PemainInputAksi inputAksi;
    private bool isInventoryOpen = false;

    private void Awake()
    {

        inputAksi = new PemainInputAksi();
        inputAksi.Pemain.Inventory.performed += _ => ToggleInventory();

        if (inventoryPanel) inventoryPanel.SetActive(false);
    }

    private void OnEnable() => inputAksi.Enable();
    private void OnDisable() => inputAksi.Disable();

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel) inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            OpenInventory();
        else
            CloseInventory();
    }

    private void OpenInventory()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (mouseLookScript) mouseLookScript.enabled = false;

        inputAksi.Pemain.Next.Disable();
        inputAksi.UI.Enable();
    }

    private void CloseInventory()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (mouseLookScript) mouseLookScript.enabled = true;

        inputAksi.UI.Disable();
        inputAksi.Pemain.Next.Enable();
    }

    void UpdateUI()
    {
        if (item == null) return;

        if (namaText) namaText.text = item.nama;
        if (kategoriText) kategoriText.text = item.kategori;
        if (deskripsiText) deskripsiText.text = item.deskripsi;
        if (ikonImage && item.ikon) ikonImage.texture = item.ikon.texture;
    }
}
