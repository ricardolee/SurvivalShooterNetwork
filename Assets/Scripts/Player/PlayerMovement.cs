using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 6f;

    Vector3 movement;
    Animator anim;
    Rigidbody playerRigidbody;
    int floorMask;
    float camRayLenght = 100f;

    [SyncVar (hook = "OnWalkingChange")]
    bool isWalking;
    
    void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        anim.SetBool("IsWalking", isWalking);

    }
    
    void Start() {
        isWalking = false;
        if (isLocalPlayer)
        {
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            cf.target = transform;
            cf.enabled = true;
        }

    }

    [Client]
    void OnWalkingChange(bool isWalkingNew) {
        if (isWalkingNew != isWalking)
        {
            isWalking = isWalkingNew;
            anim.SetBool("IsWalking", isWalking);
        }
    }

    void FixedUpdate()
    {
        if (isLocalPlayer) {
            Movement();
        }
    }

    private void Movement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Move(h, v);
        Animating(h, v);
        Turning();
    }

    private void Move(float h, float v)
    {
        movement.Set(h, 0f, v);
        movement = movement.normalized * speed * Time.deltaTime;
        playerRigidbody.MovePosition(transform.position + movement);
    }

    private void Turning()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLenght, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0f;
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            playerRigidbody.MoveRotation(newRotation);
        }
    }


    void Animating(float h, float v) {
        bool newWalkingStatus = h != 0f || v != 0f;
        if (newWalkingStatus != isWalking) {
            CmdChangeWalkingStatus(newWalkingStatus);
        }
    }

    [Command]
    void CmdChangeWalkingStatus(bool walkingStatus) {
        isWalking = walkingStatus;
        anim.SetBool("IsWalking", isWalking);
    }
}
