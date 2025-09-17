using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StoryController : MonoBehaviour
{
    [SerializeField] GameObject[] StoryPrefabs; // 실행하고 싶은 스토리 GameObject 프리팹들을 할당해주세요.
    [SerializeField] Image BackGround;
    private GameObject currentStoryInstance; // 현재 활성화된 스토리 오브젝트를 저장할 변수
    private int stageIndex;
    private StoryDialog story;
    

    private void Start()
    {
        Init();
    }
    private void Init()
    {

        stageIndex = IngameData.ChapterIdx;
        SpawnStoryObject(stageIndex); // 스토리 인스턴트화
        BackGround.sprite = story.backGroundImage;
    }

    public void SpawnStoryObject(int index)
    {
        // 이전에 생성된 오브젝트가 있다면 파괴
        if (currentStoryInstance != null)
        {
            Destroy(currentStoryInstance);
        }

        // 유효한 인덱스인지 확인
        if (index >= 0 && index < StoryPrefabs.Length)
        {
            if (StoryPrefabs[index] != null)
            {
                // 프리팹을 사용하여 새로운 GameObject 인스턴스 생성
                currentStoryInstance = Instantiate(StoryPrefabs[index], transform.position, Quaternion.identity);
                story = currentStoryInstance.GetComponent<StoryDialog>();
                // 필요하다면 생성된 오브젝트의 부모를 현재 오브젝트로 설정
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