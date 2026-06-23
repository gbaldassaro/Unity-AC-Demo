using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RangedWeaponController rightHandWeapon;
    [SerializeField] private RangedWeaponController leftHandWeapon;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI healsLeftText; 
    [SerializeField] private TextMeshProUGUI rightAmmoText; 
    [SerializeField] private TextMeshProUGUI leftAmmoText; 

    [SerializeField] private Camera mainCamera;
    private CameraController cameraController;
    [SerializeField] private RectTransform aimPoint;
    private Vector3 aimPointScreenSpace;

    private void Start()
    {
        cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void Update()
    {
        healthText.text = "Health: " + playerHealth.currentHealth.ToString();
        healsLeftText.text = "Heals: " + playerController.healsLeft.ToString();
        rightAmmoText.text = "Right Weapon Ammo: " + rightHandWeapon.currentAmmo.ToString();
        leftAmmoText.text = "Left Weapon Ammo: " + leftHandWeapon.currentAmmo.ToString();

        switch (cameraController.cameraState)
        {
            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
                aimPointScreenSpace = new Vector2(Screen.width / 2, Screen.height / 2);
                break;
            case CameraState.LockedOn:
                aimPointScreenSpace = mainCamera.WorldToScreenPoint(cameraController.lockOnLookAt.transform.position);
                break;
        }

        aimPoint.anchoredPosition = new Vector2(aimPointScreenSpace.x, aimPointScreenSpace.y);
        }
}
