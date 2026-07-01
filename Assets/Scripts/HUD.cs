using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform; 

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
    [SerializeField] private RectTransform lockOnReticle;
    [SerializeField] private RectTransform rightAimAtReticle;
    [SerializeField] private RectTransform leftAimAtReticle;

    // screen space positions of lock on and aim points
    private Vector2 lockOnPointScreenSpace;
    private Vector2 rightAimAtPointScreenSpace;
    private Vector2 leftAimAtPointScreenSpace;

    // physical positions of lock on and aim points
    [SerializeField] private Transform lockOnPoint;
    [SerializeField] private Transform rightAimAtPoint;
    [SerializeField] private Transform leftAimAtPoint;


    private void Start()
    {
        cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void LateUpdate()
    {
        healthText.text = "Health: " + playerHealth.currentHealth.ToString();
        healsLeftText.text = "Heals: " + playerController.healsLeft.ToString();
        rightAmmoText.text = "Right Weapon Ammo: " + rightHandWeapon.currentAmmo.ToString();
        leftAmmoText.text = "Left Weapon Ammo: " + leftHandWeapon.currentAmmo.ToString();

        switch (cameraController.cameraState)
        {
            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
                lockOnPointScreenSpace = Vector2.zero;
                rightAimAtPointScreenSpace = Vector2.zero;
                leftAimAtPointScreenSpace = Vector2.zero;

                break;
            case CameraState.LockedOn:
                lockOnPointScreenSpace = mainCamera.WorldToScreenPoint(lockOnPoint.transform.position);
                rightAimAtPointScreenSpace = mainCamera.WorldToScreenPoint(rightAimAtPoint.transform.position);
                leftAimAtPointScreenSpace = mainCamera.WorldToScreenPoint(leftAimAtPoint.transform.position);
                
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, lockOnPointScreenSpace, null, out Vector2 lockOnLocalPoint);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, rightAimAtPointScreenSpace, null, out Vector2 rightAimAtLocalPoint);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, leftAimAtPointScreenSpace, null, out Vector2 leftAimAtLocalPoint);


                lockOnPointScreenSpace = lockOnLocalPoint;
                rightAimAtPointScreenSpace = rightAimAtLocalPoint;
                leftAimAtPointScreenSpace = leftAimAtLocalPoint;
                break;
        }

        lockOnReticle.anchoredPosition = lockOnPointScreenSpace;
        rightAimAtReticle.anchoredPosition = rightAimAtPointScreenSpace;
        leftAimAtReticle.anchoredPosition = leftAimAtPointScreenSpace;

        }
}
