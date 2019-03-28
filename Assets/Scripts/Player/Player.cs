using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour, IKillable
{
    [Header("Mechanics")]
    public int health = 100;
    public float runSpeed = 7.5f;
    public float walkSpeed = 6f;
    public float gravity = 10f;
    public float crouchSpeed = 4f;
    public float jumpHeight = 20f;
    public int maxJumps = 2;
    public float interactRange = 10f;
    public float groundRayDistance = 1.1f;

    [Header("References")]
    public Camera attachedCamera;
    public Transform hand;

    // Animation
    private Animator anim;

    // Movement
    private CharacterController controller;
    private Vector3 movement; // Movement for current frame

    // Weapons
    public Weapon currentWeapon; // Public for testing
    private List<Weapon> weapons = new List<Weapon>();
    private int currentWeaponIndex = 0;
    //UI
    [Header("UI")]
    public GameObject interactUIPrefab;//prefab for ui
    public Transform interactUIParent;

    [Header("Jumping")]
    int jumps = 0;

    private GameObject interactUI;
    private TextMeshProUGUI interactText;
    /*
    private int score = 0;
    void UpdateUI(int score)
    {

    }
    // Update's UI when score changes
    public int Score
    {
        get { return score; }
        set
        {
            UpdateUI(value);
            score = value;
        }
    }
    */
    void DrawRay(Ray ray, float distance)
    {
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * distance);
    }
    void OnDrawGizmosSelected()
    {
        Ray interactRay = attachedCamera.ViewportPointToRay(new Vector2(.5f, .5f));
        Gizmos.color = Color.blue;
        DrawRay(interactRay, interactRange);

        Gizmos.color = Color.red;
        Ray groundRay = new Ray(transform.position, -transform.up);
        Gizmos.DrawLine(groundRay.origin, groundRay.origin + groundRay.direction * groundRayDistance);
    }
    
    #region Initialisation
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        CreateUI();
    }
    void Start()
    {
        RegisterWeapons();
        // Select current weapon at start
        SelectWeapon(0);
    }
    void CreateUI()
    {
        interactUI = Instantiate(interactUIPrefab, interactUIParent);
        interactText = interactUI.GetComponentInChildren<TextMeshProUGUI>();
    }
    void RegisterWeapons()
    {
        weapons = new List<Weapon>(GetComponentsInChildren<Weapon>());
        foreach (var weapon in weapons)
        {
            Pickup(weapon);
        }
    }
    #endregion

    #region Controls
    /// <summary>
    /// Moves the Character Controller in direction of input
    /// </summary>
    /// <param name="inputH">Horizontal Input</param>
    /// <param name="inputV">Vertical Input</param>
    void Move(float inputH, float inputV)
    {
        // Create direction from input
        Vector3 input = new Vector3(inputH, 0, inputV);
        // Localise direction to player transform
        input = transform.TransformDirection(input);
        // Set Move Speed Note(Manny): Add speed mechanic here
        float moveSpeed = walkSpeed;
        // Apply movement
        movement.x = input.x * moveSpeed;
        movement.z = input.z * moveSpeed;
    }
    #endregion

    #region Combat
    void AttachWeapon(Weapon weaponToAttach)
    {
        //call pickup on weapon
        weaponToAttach.Pickup();
        //Attach weapon to hand
        Transform weaponTransform = weaponToAttach.transform;
        weaponTransform.SetParent(hand);
        //Zero rotation and position
        weaponTransform.localRotation = Quaternion.identity;
        weaponTransform.localPosition = Vector3.zero;
        //Add to list
    }
    void DetachWeapon(Weapon weaponToDetach)
    {
        //Drop weapon
        weaponToDetach.Drop();

        //get the transform
        Transform weaponTransform = weaponToDetach.transform;

        weaponTransform.SetParent(null);
    }
    /// <summary>
    /// Switches between weapons with given direction
    /// </summary>
    /// <param name="direction">-1 to 1 number for list selection</param>
    void SwitchWeapon(int direction)
    {
        currentWeaponIndex += direction;

        if(currentWeaponIndex < 0)
        {
            currentWeaponIndex = weapons.Count - 1;
        }
        if (currentWeaponIndex >= weapons.Count)
        {
            currentWeaponIndex = 0;
        }
        SelectWeapon(currentWeaponIndex);
    }
    /// <summary>
    /// Disables GameObjects of every attached weapon
    /// </summary>
    void DisableAllWeapons()
    {
        foreach (var item in weapons)
        {
            item.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Adds weapon to list and attaches to player's hand
    /// </summary>
    /// <param name="weaponToPickup">Weapon to place in Hand</param>
    void Pickup(Weapon weaponToPickup)
    {
        AttachWeapon(weaponToPickup);

        weapons.Add(weaponToPickup);
        //select new weapon
        SelectWeapon(weapons.Count - 1);
    }
    /// <summary>
    /// Removes weapon to list and removes from player's hand
    /// </summary>
    /// <param name="weaponToDrop">Weapon to remove from hand</param>
    void Drop(Weapon weaponToDrop)
    {
        DetachWeapon(weaponToDrop);

        //Remove weapon from list
        weapons.Remove(weaponToDrop);
    }
    /// <summary>
    /// Sets currentWeapon to weapon at given index
    /// </summary>
    /// <param name="index">Weapon Index</param>
    void SelectWeapon(int index)
    {

        //is the index in range
        if(index >= 0 && index < weapons.Count)
        {
            //disable all weapons
            DisableAllWeapons();
            //select weapon
            currentWeapon = weapons[index];
            //enable the current weapon
            currentWeapon.gameObject.SetActive(true);
            //update the current index
            currentWeaponIndex = index;
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Player movement using CharacterController
    /// </summary>
    void Movement()
    {
        // Get Input from User
        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");
        Move(inputH, inputV);
        // Is the controller grounded?
        Ray groundRay = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        bool isGrounded = Physics.Raycast(groundRay, out hit, groundRayDistance);
        bool isJumping = Input.GetButtonDown("Jump");
        bool canJump = jumps < maxJumps; // jumps = int, maxJumps = int

        // Is grounded?
        if (isGrounded)
        {
            // If jump is pressed
            if (isJumping)
            {
                jumps = 1;
                // Move controller up
                movement.y = jumpHeight;
            }
        }
        // Is NOT grounded?
        else
        {
            if (isJumping && canJump)
            {
                movement.y = jumpHeight * jumps;
                jumps++;
            }


        }

        // Apply gravity
        movement.y -= gravity * Time.deltaTime;
        // Limit the gravity 
        movement.y = Mathf.Max(movement.y, -gravity);
        // Move the controller
        controller.Move(movement * Time.deltaTime);
    }
    /// <summary>
    /// Interaction with items in the world
    /// </summary>
    void Interact()
    {
        //nothing is nearby to interact, so no ui comes up
        interactUI.SetActive(false);
        //Create a ray from centre of screen
        Ray interactRay = attachedCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        RaycastHit hit;
        //shoots a ray in a range
        if(Physics.Raycast(interactRay, out hit, interactRange))
        {
            //try getting interactable object
            IInteractable interact = hit.collider.GetComponent<IInteractable>();
            //is interactable
            if(interact != null)
            {
                //enable ui
                interactUI.SetActive(true);
                interactText.text = interact.GetTitle();
                //if the E button is pressed down
                if (Input.GetKeyDown(KeyCode.E))
                {
                    
                    Weapon weapon = hit.collider.GetComponent<Weapon>();
                    if (weapon)
                    {
                        Pickup(weapon);

                    }
                }


            }
        }
    }
    /// <summary>
    /// Using the current weapon to fire a bullet
    /// </summary>
    void Shooting()
    {
        if (currentWeapon)
        {
            if (Input.GetButton("Fire1"))
            {
                currentWeapon.Shoot();
            }
        }
    }
    /// <summary>
    /// Cycling through available weapons
    /// </summary>
    void Switching()
    {
        //if there is more than one weapon
        if (weapons.Count > 1)
        {
            float inputScroll = Input.GetAxis("Mouse ScrollWheel");
            //if scroll input has been made
            if(inputScroll != 0)
            {
                //Switch weapons up or down
                int direction = inputScroll > 0 ? Mathf.CeilToInt(inputScroll) : Mathf.FloorToInt(inputScroll);

                SwitchWeapon(direction);
            }
        }
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        Movement();
        Interact();
        Shooting();
        Switching();
    }
    public void Kill()
    {
        throw new System.NotImplementedException();
    }
    public void TakeDamage(int damage)
    {
        throw new System.NotImplementedException();
    }
}
