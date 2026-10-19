using UnityEngine;

public class Rock : InteractObject, IReaction
{
    PlayerInteractor playerInteractor = null;

    private void Awake()
    {
        playerInteractor = FindAnyObjectByType<PlayerInteractor>();
    }

    int hp;
    public Rock()
    {
        hp = 10;
    }

    public void Use()
    {
        if (hp >= 1)
        {
            hp -= 1;
            Debug.Log("남은 Hp : " + hp);
        }
        if (hp <= 0)
        {
            Breaking();
        }
    }

    public void Breaking()
    {
        Destroy(gameObject);
        StartCoroutine(playerInteractor.ComeCamera());
    }
}

