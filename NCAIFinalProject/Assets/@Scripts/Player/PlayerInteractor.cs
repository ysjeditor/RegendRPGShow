using NUnit.Framework.Internal;
using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
//현재 아무리 숫자를 높여도 Ray길이가 길어지지않는 상황임
// => 마지막 인자가 거리가 아니라 보이는 시간이었음 두번째 direction에 숫자를 대입하니 해결됨.
//15:26 현재 Ray 사정거리 안에서 좌클릭을 해도 상호작용이 안되는 상태임 왜그런지 모르곘음 원인을 찾아봐야함
//15:40 디버깅 해보니 Ray hit 자체가 안되는 것 같음
//15:43 Rock에다가 BoxCollider를 입혀보니 이제서야 인식이 됨 Ray는 BoxCollider가 있어야 인식이되나?? 싶음
//15:44 그래서 Rock에다가 RequireComponent를 통해 BoxCollider를 가져오게 만들었더니 잘 됨.
public class PlayerInteractor : MonoBehaviour
{
    Camera camera = null;
    Transform[] transfromArray = null;
    Interact interact = null;
    Vector3 tempPosition = Vector3.zero;
    Vector3 finalPos = Vector3.zero;
    Vector3 rocation = Vector3.zero;
    Vector3 rocationNormalized = Vector3.zero;
    bool interacting = false;
    bool onTheMove = false;
    float progress;
    [SerializeField] float duration = 3f;
    [SerializeField] float timeSpeed = 1f;
    float cameraMoveSpeed;

    //15:57 좌클릭한 그 순간의 위치를 담을 변수를 하나 만들고 거기서 Slerp를 이용해서 카메라를 이동시켜야함
    //그리고 이동 중일 때는 좌클릭을 해도 이 변수의 값이 변화되면 안됨
    //이동중인지 아닌지확인하는 bool 변수가 필요할듯
    //근데 돌이 여러개인 경우 카메라가 이동중이지 않고, 좌클릭도 했을 경우 그 전에 좌클릭했었던 돌로 시점이 이동할 것 같기도함 우선 보류
    //16:24 우선 좌클릭했을때 카메라가 호를그리면서 가게 하고싶은데 지금 Slerp로 코드 짜는중
    //16:25 플레이해봤더니 좌클릭하면 카메라가 그냥 제자리에서 내려가기만함
    //16:28 저번에 한거 기억나서 tempPosition 및 rock의 position에 -center하고 Slerp에서 다시 더해줬는데 앞으로 가긴 하나 돌보다 앞으로 나감
    //16:40 Slerp에서 tempPosition과 rock의 position을 썼는데 그게 아니라 startPosition과 FinalPosition을 써줬으며 center에다가 y에 -2 벡터를 대입시킴
    //그러나 현재 마우스 좌클릭해도 아무런 변화가 안일어남
    //원인분석을 해봐야겠다.
    //17:28 원인분석 결과 InteractedRock 함수에서 카메라를 움직이는 로직을 넣다 보니 마우스 클릭 한번 마다 1프레임 단위로 움직이는 것이 문제였다.
    //그래서 InteractedRock 함수에서는 rock의 위치만 가져오도록 하고
    //GoingCamera함수에서 rock의 컴포넌트가 비어있지않을때 움직이도록 하니 해결됐다.
    //현재 남은 문제는 그럼 결국 코루틴이 필요한가? 인데 없어도 구현이 되기 때문
    //우선 카메라의 위치가 목표지점까지 가면 더이상 이동하지 않도록 구현을 해보자.
    //17:54 카메라가 호를 그리면서 가게하는 것 + 카메라의 x값과 y값을 transform.forward랑 up으로 위치를 수정했으나
    //progress가 1이 되지않아서 계속해서 finalPos로 가려고하는 것이 지금 문제가 됨.
    //그래서 progress >= 0.9f가 되면 멈추게 하던가 아니면 1의 90%까지도달하면 멈추게하던가 해보려고함
    // 90%까지 도달하면 멈추게하는 것을 한번 해봐야겠다.


    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
        transfromArray = GetComponentsInChildren<Transform>();
        interact = GetComponent<Interact>();
        for (int i = 0; i < transfromArray.Length; ++i)
        {
            Debug.Log(transfromArray[5].name);
        }
        Debug.Log("Awake : " + interacting);
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            InteractedRock();
        }

        if (interacting == true && Input.GetMouseButtonDown(0))
        {
            interact.GetIReaction().Use();
        }
    }

    private void InteractedRock()
    {
        interact.Interacting();
        if (interacting == false && camera.transform.position != (finalPos) && interact.GetIReaction() != null)
        {
            progress = 0f;
            Debug.Log("GoCamera");
            SetIReaction();
            StartCoroutine(GoCamera());
        }
        else
        {
            Debug.Log("대상의 태그가 맞지않거나 찾지못함");
        }

        if (Input.GetMouseButtonDown(1) && interacting == true)
        {
            progress = 0f;
            Debug.Log("ComeCamera");
            StartCoroutine(ComeCamera());
        }
    }

    private void SetIReaction()
    {
        interact.reactionObject = null;
    }

    private IEnumerator GoCamera()
    {
        tempPosition = camera.transform.position;
        while (interacting == false)
        {
            onTheMove = true;

            cameraMoveSpeed = timeSpeed / duration;
            progress += cameraMoveSpeed * Time.deltaTime;
            // 현재 문제는 속도가 점점 느려져 목적지까지 도달하지 않는 문제가 발생함
            // 일정한 속도로 목적지 까지 도착해야하려면
            // 속도가 필요하고, 시간이 필요함
            // 시간을 속도로 나눈 값을 변수에 저장하고 그걸 Time.deltaTime에 곱하면 될 것 같음

            Vector3 center = ((tempPosition + interact.GetTargetTransform()) * 0.5f) - new Vector3(0f, 2f, 0f);
            Vector3 startPos = tempPosition - center;
            finalPos = (interact.GetTargetTransform() - center) + (-transform.forward * 2f) + (transform.up * 5f);
            camera.transform.position = Vector3.Slerp(startPos, finalPos, progress) + center;

            RotatedCamera();

            if (progress >= 1f)
            {
                interacting = true;
            }
            yield return null;
        }
    }

    public IEnumerator ComeCamera()
    {
        tempPosition = transfromArray[5].transform.position;
        while (interacting == true)
        {
            cameraMoveSpeed = timeSpeed / duration;
            progress += cameraMoveSpeed * Time.deltaTime;

            Vector3 center = ((tempPosition + interact.GetTargetTransform()) * 0.5f) - new Vector3(0f, 2f, 0f);
            Vector3 startPos = tempPosition - center;
            camera.transform.position = Vector3.Slerp(finalPos, startPos, progress) + center;

            RotatedCamera();

            if (progress >= 1f)
            {
                onTheMove = false;

                interacting = false;
                Debug.Log("isMove false로 바뀜");
            }
            yield return null;
        }
    }

    private void RotatedCamera()
    {
        switch (interacting)
        {
            case false:
                rocation = interact.GetTargetTransform() - camera.transform.position;
                rocationNormalized = rocation.normalized;
                camera.transform.rotation = Quaternion.LookRotation(rocationNormalized);
                break;
            case true:
                rocation = transfromArray[6].transform.position - camera.transform.position;
                rocationNormalized = rocation.normalized;
                camera.transform.rotation = Quaternion.LookRotation(rocationNormalized);
                break;
        }
    }

    public bool GetInteracting()
    {
        return onTheMove;
    }
}