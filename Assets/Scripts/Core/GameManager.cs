using Wonjeong.Core;

namespace Core
{
    /// <summary>
    /// 프로젝트 전용 게임 매니저.
    /// 싱글톤 유지, 디버그 토글, Settings 비동기 로드는 GameManagerBase가 제공하며
    /// 프로젝트 고유의 전역 흐름 제어를 이곳에 추가함.
    /// </summary>
    public class GameManager : GameManagerBase<GameManager>
    {
    }
}
