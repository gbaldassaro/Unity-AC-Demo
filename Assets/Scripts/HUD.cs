using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform; 

    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RangedWeaponController rightHandWeapon;
    [SerializeField] private RangedWeaponController leftHandWeapon;

    [SerializeField] private TextMeshProUGUI healthNumber;
    [SerializeField] private RectTransform healthBar;
    private float healthBarWidth;
    [SerializeField] private TextMeshProUGUI healsLeftNumber; 

    [SerializeField] private TextMeshProUGUI rightAmmoNumber;
    [SerializeField] private RectTransform rightAmmoBar;
    private float rightAmmoBarWidth;
    [SerializeField] private RectTransform rightReloadBar;
    private float rightReloadBarWidth;
    [SerializeField] private TextMeshProUGUI leftAmmoNumber;
    [SerializeField] private RectTransform leftAmmoBar;
    private float leftAmmoBarWidth;
    [SerializeField] private RectTransform leftReloadBar;
    private float leftReloadBarWidth;

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

    [SerializeField] private RectTransform energyBar;
    private float energyBarWidth;


    private void Start()
    {
        cameraController = mainCamera.GetComponent<CameraController>();
        energyBarWidth = energyBar.rect.width;
        healthBarWidth = healthBar.rect.width;
        rightAmmoBarWidth = rightAmmoBar.rect.width;
        leftAmmoBarWidth = leftAmmoBar.rect.width;
        rightReloadBarWidth = rightReloadBar.rect.width;
        leftReloadBarWidth = leftReloadBar.rect.width;
    }

    private void LateUpdate()
    {
        // health info
        healthNumber.text = playerHealth.currentHealth.ToString() + "/" + playerHealth.maxHealth.ToString();
        healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthBarWidth * playerHealth.currentHealth / playerHealth.maxHealth);

        // healing info
        healsLeftNumber.text = playerController.healsLeft.ToString();

        // ammo info
        rightAmmoNumber.text = rightHandWeapon.currentAmmo.ToString() + "/" + rightHandWeapon.maxAmmo.ToString();
        rightAmmoBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rightAmmoBarWidth * rightHandWeapon.currentAmmo / rightHandWeapon.maxAmmo);
        rightReloadBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rightReloadBarWidth * rightHandWeapon.elapsedReloadTime / rightHandWeapon.reloadTime);

        leftAmmoNumber.text = leftHandWeapon.currentAmmo.ToString() + "/" + leftHandWeapon.maxAmmo.ToString();
        leftAmmoBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, leftAmmoBarWidth * leftHandWeapon.currentAmmo / leftHandWeapon.maxAmmo);
        leftReloadBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, leftReloadBarWidth * leftHandWeapon.elapsedReloadTime / leftHandWeapon.reloadTime);


        // energy info
        energyBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, energyBarWidth * playerController.currentEnergy / playerController.maxEnergy);

        // reticles
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
