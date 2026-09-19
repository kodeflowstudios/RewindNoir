using UnityEngine;

public class SunGideon : MonoBehaviour
{
	public PlayerController playerController;
	public ObjOfInterest objOfInterest;
	public Animator anim;
	public SunSceneSwitch swap;

    void Start()
    {
		Invoke("StartDialogue", 3f);
    }

	void StartDialogue()
	{
		objOfInterest.StartDialogue();
		playerController.EnableMoving();
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.Confined;
	}

	public void DetermineChoice()
	{
		if (objOfInterest.parleyYaml.Flags.IsFlagSet("go_back"))
		{
			GoBack();
		}
		else if (objOfInterest.parleyYaml.Flags.IsFlagSet("die"))
		{
			Shoot();
		}
	}

	void Shoot()
	{
		anim.Play("Shoot");
	}

	void GoBack()
	{
		GameManager.Instance.hasTalked = false;
		GameManager.Instance.inDialogue = false;
		GameManager.Instance.enteredEntropy = false;
		GameManager.Instance.TransitionToNormal();
	}
}
