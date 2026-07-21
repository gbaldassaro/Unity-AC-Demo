using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MenuManager : MonoBehaviour
{
    public static MenuManager singleton;

    [SerializeField] private DeathMenu deathMenu;
    
    void Start()
    {
        singleton = this;
    }

    public void ShowDeathMenu()
    {
        deathMenu.Show();
    }

    public void HideDeathMenu()
    {
        deathMenu.Hide();
    }
}
