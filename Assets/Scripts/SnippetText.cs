using UnityEngine;

public class SnippetText : MonoBehaviour
{
    void Start()
    {
		Invoke("DestroyMe", 5f);
    }

	void DestroyMe()
	{
		Destroy(gameObject);
	}

    void Update()
    {
		transform.position += new Vector3(0, 1, 0) * Time.deltaTime;
    }
}
