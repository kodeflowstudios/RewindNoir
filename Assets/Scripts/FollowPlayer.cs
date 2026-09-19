using UnityEngine;
using UnityEngine.UIElements;

public class FollowPlayer : MonoBehaviour
{
	public float speed = 4f;
	public PlayerController playerController;
	public UIDocument resultScreen;
	public Animator anim;

	void Awake()
	{
		var lookPos = playerController.transform.position - transform.position;
		transform.rotation = Quaternion.LookRotation(lookPos);
	}

    void OnEnable()
    {
		anim.Play("Minimi Fly");
    }

    void OnDisable()
    {
		anim.Play("Minimi Idle");
    }

    void Update()
    {
		float step =  speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, playerController.transform.position, step);

		var lookPos = playerController.transform.position - transform.position;
		transform.rotation = Quaternion.LookRotation(lookPos);
    }

	private void OnTriggerEnter(Collider other)
	{	
		resultScreen.rootVisualElement.style.display = DisplayStyle.Flex;
		resultScreen.GetComponent<ResultScreen>().ShowResult("You Lose!", Color.red);
		playerController.DisableMoving();
	}
}
