using KodeFlowStudios.Parley.YamlCore;
using TMPro;
using UnityEngine;

public class Snippets : MonoBehaviour
{
	ParleyYaml parleyYaml;
	public GameObject textPrefab;

    void Start()
    {
		parleyYaml = new ParleyYaml("Dialogues", "ScottVoiceSnippents");
		InvokeRepeating("SpawnText", 5f, 9);
    }

	void SpawnText()
	{
		Vector3 randomPoint = new(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.7f));
		randomPoint.z = 10f;
		Vector3 worldPoint = Camera.main.ViewportToWorldPoint(randomPoint);

		var txt = Instantiate(textPrefab, transform);
		txt.transform.position = randomPoint;
		txt.GetComponent<TMP_Text>().text = parleyYaml.CurrentNode.Text;
		parleyYaml.UnBindNextEvent();
		parleyYaml.ProgressDialogue();
	}
}
