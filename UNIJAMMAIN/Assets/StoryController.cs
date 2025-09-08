using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StoryController : MonoBehaviour
{
    [SerializeField] GameObject[] StoryPrefabs; // �����ϰ� ���� ���丮 GameObject �����յ��� �Ҵ����ּ���.
    [SerializeField] Image BackGround;
    private GameObject currentStoryInstance; // ���� Ȱ��ȭ�� ���丮 ������Ʈ�� ������ ����
    private int stageIndex;
    private StoryDialog story;
    

    private void Start()
    {
        Init();
    }
    private void Init()
    {

        stageIndex = IngameData.ChapterIdx;
        SpawnStoryObject(stageIndex); // ���丮 �ν���Ʈȭ
        BackGround.sprite = story.backGroundImage;
    }

    public void SpawnStoryObject(int index)
    {
        // ������ ������ ������Ʈ�� �ִٸ� �ı�
        if (currentStoryInstance != null)
        {
            Destroy(currentStoryInstance);
        }

        // ��ȿ�� �ε������� Ȯ��
        if (index >= 0 && index < StoryPrefabs.Length)
        {
            if (StoryPrefabs[index] != null)
            {
                // �������� ����Ͽ� ���ο� GameObject �ν��Ͻ� ����
                currentStoryInstance = Instantiate(StoryPrefabs[index], transform.position, Quaternion.identity);
                story = currentStoryInstance.GetComponent<StoryDialog>();
                // �ʿ��ϴٸ� ������ ������Ʈ�� �θ� ���� ������Ʈ�� ����
                // currentStoryInstance.transform.SetParent(this.transform);
                Debug.Log($"Story prefab at index {index} instantiated.");
            }
            else
            {
                Debug.LogError($"StoryPrefab at index {index} is null.");
            }
        }
        else
        {
            Debug.LogError($"Invalid index: {index}. StoryPrefabs array size is {StoryPrefabs.Length}.");
        }
    }
}