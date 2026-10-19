using Unity.VisualScripting;
using UnityEngine;

public class Interact : MonoBehaviour
{
    PlayerController playerController = null;
    public IReaction reactionObject = null;
    Vector3 interactRaySetting = Vector3.zero;
    Vector3 targetTransform = Vector3.zero;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        Debug.DrawRay(playerController.transform.position + Vector3.down * 0.5f, Vector3.forward * 5f, Color.yellow);
    }

    public void Interacting()
    {
        Debug.Log("interacting 함수는 실행된 상태");
        interactRaySetting = playerController.transform.position + Vector3.down * 0.5f;

        if (Physics.Raycast(interactRaySetting, Vector3.forward * 5f, out RaycastHit target))
        {
            reactionObject = target.transform.gameObject.GetComponent<IReaction>();
            if (reactionObject != null)
            {
                targetTransform = target.transform.position;
            }
        }
    }

    public Vector3 GetTargetTransform()
    {
        return targetTransform;
    }

    public IReaction GetIReaction()
    {
        return reactionObject;
    }
}
