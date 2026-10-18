using System.Collections;
using UnityEngine;

namespace FBFactory
{
    public enum UnitIssueType
    {
        None,                   // 특이사항 없음
        UserRequest,            // 사용자 조작에 의한 상태 변경
        EmergencyStop,          // 비상 정지 발생
        EquipmentFault,         // 장비 자체 이상 발생
        UpstreamUnavailable,    // 이전 공정에서 제품 공급 불가
        DownstreamUnavailable   // 다음 공정으로 제품 배출 불가
    }


    public enum FigureType
    {
        None,

        Cat,
        Bear,
        Crocodile,
        Panda,
        Rabbit,
    }

    // 현재는 Apple만 사용
    public enum FlavorType
    {
        None,
        Apple,
        Grape,
        Orange,
    }



}