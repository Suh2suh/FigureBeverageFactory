[Code Convention]

이 프로젝트에서는 아래 C# 코드 컨벤션을 따른다.

1. 필드 / 프로퍼티 / 상수 네이밍
- private field: _camelCase
  - 예: _currentTarget, _moveSpeed
- public property / public field: PascalCase
  - 예: CurrentTarget, MoveSpeed
- private const: k_camelCase
  - 예: k_defaultMoveSpeed, k_maxRetryCount
- public const: k_PascalCase
  - 예: k_DefaultMoveSpeed, k_MaxRetryCount
- static 멤버는 별도 prefix를 붙이지 않는다.
  - 접근 제한자 기준으로 private이면 _camelCase, public이면 PascalCase를 따른다.

2. 메서드 네이밍
- public / private 메서드는 PascalCase를 사용한다.
  - 예: TryInitialize(), SetCombatTarget(), ClearPatrolPoint()

3. 직렬화 필드
- Unity Inspector에 노출되는 private SerializeField도 private field 규칙을 따른다.
  - 예: [SerializeField] private float _moveSpeed;
  - 예: [SerializeField] private Transform _eyeTransform;

4. Boolean 네이밍
- bool 값은 의미가 드러나도록 Is / Has / Can / Should 계열을 우선 사용한다.
  - 예: IsInitialized, HasTarget, CanAttack, ShouldSprint

5. enum 네이밍
- enum 타입명과 enum 값은 PascalCase를 사용한다.
  - 예:
    public enum AICombatAlertIntent
    {
        Support,
        WatchThreat,
        WatchAround,
    }

6. 간결성 원칙
- 불필요한 방어 코드, 과한 추상화, 사용하지 않는 변수/함수 추가를 피한다.
- 기존 코드 구조와 스타일을 우선 따른다.
- 단순한 기능은 단순하게 구현한다.

7. Unity 기준
- MonoBehaviour의 Unity 이벤트 함수는 Unity 표준 이름을 그대로 사용한다.
  - Awake(), OnEnable(), Start(), Update(), OnDisable()
- Inspector에서 조절해야 할 값은 const보다 [SerializeField] private field를 우선 고려한다.
- 코드에서 고정되어야 하는 값만 const로 둔다.