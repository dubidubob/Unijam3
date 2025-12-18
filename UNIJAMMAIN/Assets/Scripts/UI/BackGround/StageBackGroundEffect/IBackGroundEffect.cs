using UnityEngine;

public interface IBackGroundEffect
{
    // 컨트롤러의 데이터(이미지, 비트 시간 등)를 가져오기 위한 초기화
    void Initialize(BackGroundController controller);

    // 비트가 떨어질 때 실행할 메인 액션
    void EffectActionGo();
}