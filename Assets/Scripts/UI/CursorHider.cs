using UnityEngine;

public class CursorHider : MonoBehaviour
{
    public static CursorHider singleton;

    private void Start()
    {
        singleton = this;
        Hide();
    }

    public void Show()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Hide()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}