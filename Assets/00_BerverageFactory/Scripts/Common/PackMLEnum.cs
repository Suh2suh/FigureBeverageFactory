using UnityEngine;


namespace FBFactory
{
    //  [PackMLEnum.cs]
    /// - 임의의 상태 체계 대신 산업 표준인 PackML 기반 상태 모델을 표현 (현재 버전인 v1 기준으로, 추후 확장될 수 있음)
    /// [의도]
    /// - 실제 산업 설비와 유사한 상태 구조를 사용
    /// - 추후 veresion에서 PLC/MES 등 외부 시스템과의 연동 및 확장 필요

    public enum PackMLState
    {
        /// <summary> 정상 정지 상태 </summary>
        Stopped,

        /// <summary> 초기화 및 운전 준비 중 </summary>        
        Resetting,

        /// <summary> 시작 가능한 대기 상태 </summary>        
        Idle,

        /// <summary> 기동 절차 수행 중 </summary>        
        Starting,

        /// <summary> 작업 수행 중 </summary>        
        Execute,

        /// <summary> 정상 정지 절차 수행 중 </summary>        
        Stopping,

        /// <summary> 비정상 중단 절차 수행 중 </summary>        
        Aborting,

        /// <summary> 비정상 중단 완료 </summary>        
        Aborted,

        /// <summary> Aborted 상태 복구 중 </summary>        
        Clearing
    }



    public enum PackMLMode
    {
        Production = 1,  // 정상적으로 제품을 생산하는 모드
        Maintenance = 2, // 정비/시험을 위해 장비를 독립적으로 운전하는 모드
        Manual = 3       // 개별 장치/요소를 직접 수동 제어하는 모드
    }


    // PackMLCommand는, Machine Interface로 뺐기 때문에 Enum으로 사용하지 않음
    // Command 또한 PackMLModel의 일부이기 때문에 기록용으로 남겨둠
    /*
    public enum PackMLCommand
    {
        Start,          // 생산/작업 실행 시작 요청
        Reset,          // Stopped/Complete 등에서 초기화하여 준비 요청


        Stop,           // 정상적인 정지 요청
        Abort,          // 즉각적인 비정상 중단 요청
        Clear,          // Aborted 상태에서 복구 절차 요청

        StateComplete   // 현재 transient state의 작업 완료 신호


        // Hold,           // 현재 작업을 일시적으로 Hold 요청
        // Unhold,         // Held 상태에서 작업 재개 요청

        // Suspend,        // 외부 공정 조건 등에 의해 작업 중단 요청
        // Unsuspend,      // Suspend 원인이 해소되어 작업 재개 요청

        // Complete,       // 현재 작업/생산의 완료 절차 요청
    }
    */


}
