using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    Rigidbody playerRb = null;
    CapsuleCollider playerCc = null;
    PlayerInteractor playerInteractor = null;
    float moveSpeed = 7f;
    Vector3 jumpRange = Vector3.zero;
    Vector3 jumpRaySetting = Vector3.zero;
    bool isJump = false;
    private void Reset()
    {

    }

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerCc = GetComponent<CapsuleCollider>();
        playerInteractor = GetComponent<PlayerInteractor>();
        jumpRange = new Vector3(0f, 5f, 0f);
    }

    private void Update()
    {
        ControllingPlayer();
        Jumping();

        jumpRaySetting = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
        //Update로 옮겨야 Ray위치가 계속 플레이어값으로 보정됨. 이걸 안해서 안된거였음 또한 시작지점을 + 0.5f로 해버리니 땅으로 가기도 전에 플레이어한테 부딪혀서 밑으로 쏴지지 않는거였음

        if (Physics.Raycast(jumpRaySetting, -transform.up, out RaycastHit jumpRay, 1f))
        {
            isJump = false;
        }
        else
        {
            isJump = true; //공중에서 false가 되게 하여 연속 점프가 안되게 만듬
        }
    }

    private void ControllingPlayer()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 Direction = new Vector3(horizontal, 0f, vertical);
        Vector3 normalizedDirection = Direction.normalized;
        Vector3 velocity = normalizedDirection * moveSpeed;
        playerRb.linearVelocity = new Vector3(velocity.x, playerRb.linearVelocity.y, velocity.z); //linearVelocity의 x, y, z에 값을 지정해주지않아서(특히 y값에 지정을 안해서 중력이 적용이 제대로 안됐음)

        if (playerInteractor.GetInteracting() == true)
        {
            playerRb.linearVelocity = Vector3.zero;
            // 지금 현재 카메라가 rock으로 도착해야만 true가 되는 상황이라
            // 카메라가 움직일때 체크하는 변수를 하나 만들고 그게 true라면 zero로 만들면 될듯
        }
    }

    private void Jumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isJump == false)
        {
                playerRb.AddForce(jumpRange, ForceMode.Impulse);
        }
    }
    //플레이어의 조작을 WASD를 통해서 할거임
    //오브젝트가 바라보는 방향과 관계없이 월드 기준으로 움직이게 할거임
    //점프도 가능해야함
    //Rigidbody를 이용해서 해야함
}
