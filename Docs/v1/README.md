# V1 — Operational Factory

## Goal

**V1에서는 Raw Material부터 Finished Goods까지 전체 Production Flow를 먼저 동작시켰습니다.**

처음부터 PLC, Sensor, Robot, DES 등을 각각 깊게 구현하기보다, 전체 Factory를 먼저 만들어봐야 이후 어떤 부분을 더 자세히 모델링해야 하는지 직접 발견할 수 있다고 생각했습니다.

- **목표**
  - Raw Material → Finished Goods까지 End-to-End Production Flow 구현
  - 전체 Production Line의 구조와 Material Flow 경험
  - 이후 Version을 확장할 수 있는 기본 구조 설계

- **V1에서 중요하게 생각한 것**
  - 현실의 Equipment를 정밀하게 재현하기보다 전체 Flow를 먼저 완성
  - Rough하게 구현하더라도 Equipment 간 결합도는 낮게 유지
  - 실제로 Factory를 돌려보면서 다음에 필요한 기술을 발견

- **의도적으로 미룬 것**
  - DES
  - PLC / Virtual PLC
  - Sensor / Actuator
  - Robot Motion
  - 실제 Computer Vision
  - ROS2
  - AMR / Warehouse
  - Monitoring / Analytics
  - Optimization

> **정밀한 Factory를 처음부터 만드는 것이 아니라, Rough한 Factory 전체를 먼저 만들고 필요한 부분의 해상도를 점점 높인다.**

---

# Production Flow

## Raw Material부터 Finished Goods까지 연결한다.

**실제 Beverage Production의 전체 구조를 경험하기 위해 생산 시작부터 완제품 Pallet까지 하나의 Flow로 연결했습니다.**

Krones의 Beverage Production Line과 Equipment를 참고했지만, 실제 Equipment를 정밀하게 복제하는 것이 V1의 목적은 아닙니다.

### 구현한 Production Flow

```text id="df5ycv"
Raw Material
    ↓
Blow Moulding
    ↓
Filling
    ↓
Capping
    ↓
Fill / Cap Inspection
    ↓
Bottle Buffer
    ↓
Figure Insertion
    ↓
Figure Inspection
    ↓
Labeling
    ↓
Coding
    ↓
Label / Code Inspection
    ↓
Packaging
    ↓
Pack Buffer
    ↓
Palletizing
    ↓
Stretching
    ↓
Finished Goods
```

- **Krones Production Line 참고**
  - 공정의 전체적인 흐름과 Equipment 구성을 잡기 위해 참고
  - V1에서는 실제 Equipment 내부 Mechanics까지 구현하지 않음

- **Figure Insertion / Figure Inspection 추가**
  - 실제 Krones 공정을 그대로 재현한 것이 아닌 가상의 공정
  - 이후 Robot Arm / Vision을 학습할 수 있는 확장 지점을 미리 만들기 위해 추가

- **Trade-off**
  - 실제 Beverage Line을 그대로 재현하면 현실성은 높아짐
  - 대신 Robot / Vision 등 내가 학습하고 싶은 기술을 자연스럽게 넣기 어려움
  - 따라서 실제 Production Flow를 기반으로 하되 일부 공정은 학습 목적에 맞게 추가

---

# Material Flow

## Bottle → Pack → Pallet으로 Material Unit을 변경한다.

**생산 과정에 따라 Material의 물류 단위가 실제로 변경되도록 구현했습니다.**

- **Bottle → Pack**
  - Beverage 6개를 하나의 Pack으로 Packaging

- **Pack → Pallet**
  - Pack 8개를 하나의 Pallet으로 Palletizing

```text id="7lq9cl"
Bottle × 6
    ↓
Pack × 1

Pack × 8
    ↓
Pallet × 1
```

- **Material Data 전달**
  - Bottle이 가지고 있던 Beverage Data를 Pack에 전달
  - Pack의 데이터를 다시 Pallet에 전달
  - Material Unit이 변경되어도 Product 정보를 유지

```text id="50imic"
Beverage Data
     ↓
   Bottle
     ↓
    Pack
     ↓
   Pallet
```

- **왜 이렇게 만들었나**
  - 단순히 GameObject의 외형만 바꾸는 것이 아니라 실제 생산 과정에서 물류 단위가 변경되는 의미를 표현하고 싶었음
  - 이후 Lot / Traceability / Production Data로 발전할 수 있는 기반을 남기기 위함

---

# Equipment State

## PackML 기반의 공통 Equipment State를 사용한다.

**Factory는 Rough하게 만들더라도 Equipment State까지 임의의 Unity State로 만들고 싶지는 않았습니다.**

- **`UnitBase`**
  - Equipment가 공통 Operational State를 가지도록 구성
  - PackML을 참고한 State 사용

```text id="7ep5jn"
Stopped
Resetting
Idle
Starting
Execute
Stopping
Aborting
Aborted
Clearing
```

- **`LineController`**
  - Line 수준의 Start / Stop 명령 담당
  - 실제 Production Logic은 각 Unit이 담당

```text id="2p2f3g"
LineController
      ↓
 Start / Stop
      ↓
    Units
```

- **왜 PackML을 사용했나**
  - 이후 Virtual PLC / Equipment Control로 확장할 가능성을 고려
  - V1부터 Equipment의 Operational State에 산업 Automation의 의미를 유지하고 싶었음

- **Trade-off**
  - V1만 생각하면 `Running / Stop` 정도의 단순 State로도 충분
  - 하지만 이후 State Model을 전부 교체해야 할 가능성이 있음
  - 따라서 구현 부담이 크지 않은 범위에서 PackML의 일부 State를 먼저 적용

---

# Execute ≠ Processing

## Equipment State와 Material Processing State를 분리한다.

**PackML을 적용하면서 `Execute`와 실제 Material Processing은 다른 개념이라고 판단했습니다.**

- **Execute**
  - Equipment가 자동 생산 가능한 Operational State

- **Processing**
  - 현재 Material에 실제 작업을 수행하고 있는 상태

예를 들어:

```text id="1unb7a"
Filler = Execute

Bottle 있음
→ Filling

Bottle 없음
→ 대기
```

- **왜 구분했나**
  - Material이 없다고 Equipment가 Fault인 것은 아님
  - Downstream이 막혔다고 Equipment가 Abort되는 것도 아님

- **여기서 발견한 다음 문제**
  - Material이 없으면? → `Starved`
  - Downstream이 막히면? → `Blocked`
  - Material / Resource를 기다리면? → `Waiting`

- **V1의 선택**
  - Operational State와 Processing의 의미까지만 구분
  - Waiting / Blocking / Starvation의 실제 Simulation은 V2로 미룸

---

# Unit Boundary

## 모든 Factory Object를 Unit으로 만들지 않는다.

**`UnitBase`는 독립적으로 운전되는 Equipment의 경계로 사용했습니다.**

### Unit으로 모델링

- Filler
- Capper
- Inspector
- Figure Inserter
- Labeler
- Coder
- Packager
- Palletizer
- Stretcher

### Unit으로 모델링하지 않음

- Material
- Conveyor
- Buffer
- Storage

### 기준

- **Unit**
  - 독립적인 Operational State를 가짐
  - Start / Stop / Reset / Abort 등의 Equipment Command를 받을 수 있음

- **Conveyor / Buffer / Storage**
  - Material Transport / Accumulation / Storage 역할
  - V1에서는 독립적인 Equipment State가 필요하지 않다고 판단

### Trade-off

- **모든 Object를 Unit으로 만들기**
  - 구조가 통일된다는 장점
  - 하지만 단순 Conveyor와 Storage까지 불필요한 PackML State를 가지게 됨

- **Equipment만 Unit으로 만들기**
  - Object마다 역할이 달라 구조는 완전히 동일하지 않음
  - 대신 Operational Equipment와 Material Flow 요소의 책임이 명확해짐

- **V1의 선택**
  - 독립적으로 운전되는 Equipment만 `UnitBase` 사용

---

# Product Definition

## Product 정보와 Equipment Logic을 분리한다.

**어떤 Beverage를 생산해야 하는지와 Equipment가 실제로 무엇을 하는지를 분리했습니다.**

### Product Definition

```text id="akj31j"
BeverageDefinition
 ├─ PETDefinition
 ├─ FillingDefinition
 └─ Figure
```

- `BeverageDefinition`
  - 완성할 Beverage의 전체적인 Product 정보

- `PETDefinition`
  - Bottle / PET 관련 정보

- `FillingDefinition`
  - FillingType
  - FillAmount

### 역할 분리

```text id="7nm3bd"
Definition
= 무엇을 만들어야 하는가?

Equipment
= 어떻게 처리하는가?

Material
= 실제로 어떤 상태가 되었는가?
```

예를 들어:

- **Filler**
  - `FillingDefinition`을 읽음
  - Bottle에 실제 Filling 결과를 적용

- **Inspector**
  - 같은 Definition을 기준으로 실제 결과를 검사

### 왜 분리했나

- Product 종류가 Machine Logic에 직접 종속되지 않도록 하기 위해
- 같은 Product Definition을 여러 Equipment에서 공유하기 위해
- 이후 Recipe / Routing / Production Order로 확장할 수 있도록 하기 위해

### V1의 Trade-off

- 현재 일부 Production Setting은 각 Unit이 직접 Definition을 참조
  - 구현이 단순함
  - V1 End-to-End Flow를 빠르게 만들기 좋음

- Line 수준 Recipe를 먼저 만들 수도 있었음
  - 구조적으로는 더 완성도가 높음
  - 하지만 V1에서는 실제로 어떤 정보가 공통 Recipe에 필요한지 아직 경험하지 못함

- **V1의 선택**
  - 필요한 Definition만 먼저 분리
  - 실제 Factory를 돌려본 뒤 V2에서 Production Recipe를 공통화

---

# Inspection

## 실제 Vision보다 Inspection Flow를 먼저 만든다.

**V1에서는 Camera / Computer Vision보다 PASS / FAIL이 Production Flow에 어떤 영향을 주는지를 먼저 구현했습니다.**

### Fill / Cap Inspection

- Filling Amount 검사
- Cap 상태 검사
- PASS / FAIL 판정

### Figure Inspection

- Figure 상태 검사
- PASS → 다음 공정
- FAIL → Figure Insertion으로 Rework

```text id="67zscw"
Figure Insertion
       ↓
Figure Inspection
   ┌───┴────┐
 PASS      FAIL
   ↓         ↓
 Next      Rework
             ↓
       Figure Insertion
```

### Label / Code Inspection

- Label 검사
- Code 검사
- FAIL → 정상 Production Flow에서 Reject

### 왜 실제 Vision을 구현하지 않았나

- V1에서 중요한 것은 Vision Algorithm 자체가 아니라 Inspection 공정의 역할
- 실제 Vision까지 구현하면 Camera / Image Processing 등으로 Scope가 크게 증가
- 이후 Vision을 추가하더라도 Production Flow 자체는 유지할 수 있도록 먼저 구조를 만듦

### Trade-off

- **실제 Vision**
  - 현실적인 Inspection 구현 가능
  - V1 Scope가 크게 증가

- **Data 기반 Inspection**
  - 실제 Vision 경험은 없음
  - 대신 PASS / FAIL / Rework / Reject라는 Production Logic을 빠르게 구현 가능

- **V1의 선택**
  - Data 기반 Inspection
  - 실제 Vision은 Equipment Detail이 필요한 Version에서 확장

---

# Finished Goods

## Finished Goods를 V1 Production Flow의 끝으로 사용한다.

**완성된 Pallet이 실제로 적재되는 지점까지 구현했습니다.**

- **Palletizing**
  - Pack 8개 → Pallet 1개

- **Stretching**
  - Pallet의 `IsStretched` 상태 변경

- **FinishedGoodsContainer**
  - 완성된 Pallet 보관
  - Multi-Layer Stacking
  - Pallet Count 표시

```text id="1h0uk7"
Bottle
  ↓
Pack
  ↓
Pallet
  ↓
Stretching
  ↓
Finished Goods
```

### 왜 여기서 끝냈나

초기에는:

```text id="9xg0h4"
Finished Goods
→ AMR
→ Warehouse
```

까지 구현하는 것도 고려했습니다.

하지만 AMR을 단순히 Pallet을 움직이는 Object로 추가하면 Navigation / Dispatch / Transport Request 등 Intralogistics의 의미가 빠지게 됩니다.

### Trade-off

- **AMR / Warehouse까지 V1에서 구현**
  - Factory의 시각적인 범위가 넓어짐
  - 대신 단순 이동 Animation 수준으로 끝날 가능성이 높음

- **Finished Goods에서 종료**
  - V1에서는 Warehouse까지 연결되지 않음
  - 대신 Production과 Intralogistics의 Scope를 명확하게 분리할 수 있음

- **V1의 선택**
  - `FinishedGoodsContainer`를 Production Simulation의 Sink로 사용
  - AMR / Warehouse는 Intralogistics를 제대로 다룰 수 있는 Version에서 확장

---

# What I Learned — Buffer

## Buffer는 단순한 Storage가 아니었다.

**V1에서 가장 크게 발견한 문제는 Continuous Conveyor와 Machine Processing Timing의 차이였습니다.**

### V1의 초기 구조

```text id="pkpqxd"
Conveyor 이동
    ↓
Bottle Trigger 진입
    ↓
Process 실행
    ↓
계속 이동
```

### 실제로 돌려보니

- Conveyor는 계속 Material을 공급함
- 각 Machine의 Processing Timing은 서로 다름
- Bottle은 계속 이동함
- 일부 Bottle이 의도한 Timing에 처리되지 않음

```text id="61hw0a"
Continuous Flow
      +
Processing Timing 차이
      ↓
Material Flow 문제
```

### 여기서 생긴 질문

- Bottle은 Machine이 작업 중이면 어디서 기다리지?
- Buffer는 몇 개까지 받을 수 있지?
- Downstream이 가득 차면 어떻게 하지?
- Upstream에 Bottle이 없으면 Machine은 어떤 상태지?
- Machine은 언제 Material을 가져가고 언제 다음 공정으로 넘기지?

### 결국 필요해진 개념

- Queue
- Buffer Capacity
- Process Time
- Material Handoff
- Waiting
- Blocking
- Starvation

### 왜 중요한 경험이었나

처음부터 DES 개념을 공부하고 억지로 적용한 것이 아니라:

```text id="y7sxv2"
Factory 구현
    ↓
직접 실행
    ↓
Material Flow 문제 발견
    ↓
"왜 이러지?"
    ↓
Buffer의 의미 이해
    ↓
Queue / Capacity / Process Time 필요
    ↓
DES 필요
```

라는 순서로 필요성을 발견했습니다.

**이 과정 자체가 이 프로젝트를 Top-down 방식으로 진행하고 싶었던 이유와 가장 잘 맞았습니다.**

---

# Next — V2

## 움직이는 Factory를 생산 시스템 Simulation으로 바꾼다.

**V2에서는 V1에서 발견한 Material Flow 문제를 해결하기 위해 시간과 대기를 명시적으로 모델링합니다.**

### Material Handoff

현재:

```text id="6rfj6q"
Conveyor
→ Trigger
→ Process
→ Conveyor
```

V2:

```text id="5e56vf"
Conveyor
    ↓
Enter Zone
    ↓
Machine
    ↓
Processing
    ↓
Exit Zone
    ↓
Conveyor
```

### 추가할 Simulation 요소

- Process Time
- Queue
- Buffer Capacity
- Waiting
- Blocking
- Starvation
- Resource Availability

### Production Recipe

- V1
  - 일부 Unit이 필요한 Definition을 직접 참조

- V2
  - Line 수준 Production Recipe 검토
  - 여러 Unit이 하나의 생산 목표를 공유

### 실제 Production Line 조사

V2에서는 구현과 함께 실제 Beverage Production Line의:

- Accumulation Conveyor
- Machine Infeed / Outfeed
- Line Control
- Buffer / Flow Control

등이 어떻게 구성되는지도 조사합니다.

실제 시스템을 그대로 복제하기보다는 **Simulation에서 필요한 의미가 무엇인지 판단하여 모델에 반영하는 것**이 목표입니다.

---

# V1 → V2

**V1의 가장 중요한 결과는 Factory가 동작했다는 것뿐만 아니라, 다음에 무엇을 공부해야 하는지 발견했다는 것입니다.**

```text id="3dbff5"
전체 Factory 구현
        ↓
Raw Material → Finished Goods 동작
        ↓
Material Flow 문제 발견
        ↓
Buffer의 의미 이해
        ↓
Process Time / Queue / Capacity 필요
        ↓
Waiting / Blocking / Starvation 필요
        ↓
DES로 발전
```

### V1 Complete ✅

- Raw Material → Finished Goods Production Flow
- PackML 기반 Equipment State
- Line-level Start / Stop
- Bottle → Pack → Pallet Material Flow
- Product Definition
- Filling / Capping
- Inspection / Rework / Reject
- Figure Insertion
- Labeling / Coding
- Packaging
- Palletizing
- Stretching
- Finished Goods Stacking

### Next

**V2 — Discrete Event Simulation**

> V1의 **"움직이는 Factory"**를  
> V2에서는 **"생산 시스템을 설명할 수 있는 Simulation"**으로 발전시킵니다.