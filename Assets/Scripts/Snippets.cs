using KodeFlowStudios.Parley.YamlCore;
using TMPro;
using UnityEngine;

public class Snippets : MonoBehaviour
{
	ParleyYaml parleyYaml;
	public GameObject textPrefab;
	RectTransform canvasRect;

    void Start()
    {
		canvasRect = GetComponent<RectTransform>();
		parleyYaml = new ParleyYaml("Dialogues", "ScottVoiceSnippents");
		InvokeRepeating("SpawnText", 5f, 9);
    }

	void SpawnText()
	{
		float w = canvasRect.rect.width;
		float h = canvasRect.rect.height;

		Vector2 pos = new(Random.Range(-w/2.8f, w/2.8f), Random.Range(-h/2.2f, h/3f));

		var txt = Instantiate(textPrefab, transform);
		txt.GetComponent<RectTransform>().localPosition = pos;
		txt.GetComponent<TMP_Text>().text = parleyYaml.CurrentNode.Text;
		parleyYaml.ProgressDialogue();
	}
}
