using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    [Obsolete]
    public class FactoryDTO : SingletonBase<FactoryDTO>
    {
        // Obsolete
        // - BOM ScriptableObject를 각 Order의 데이터로 사용
        //   ㄴ has Factory Material(Figure/Filling...) information
        // ∴ 기존에 Material 정보 저장용으로 사용했던 FactoryDTO는 Obsolete 처리 
    }
}
