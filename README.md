# 🏭 Figure Beverage Factory

## Current Status

🚧 Work in Progress

Current Working Version: V2 — DES

This project is being developed incrementally from a simple
production-flow model toward DES, analytics, automation,
virtual control integration, and optimization.

## Why this project?

**제가 하고 싶은 일을 직관적으로 보여주기 위해 작은 Factory를 직접 만들기로 했습니다.**

제가 관심 있는 것은 단순히 3D 엔진에서 Factory를 시각화하는 것이 아니라,

- 생산 시스템을 모델링하고
- Simulation하고
- 상태를 관찰하고
- Bottleneck을 찾고
- 조건을 변경하여 결과를 비교하고
- Robot / PLC 등의 Automation System과 연결하고
- 최종적으로 더 나은 운영 방법을 찾는 것

입니다.

이런 일을 하고 싶다고 말로 설명하는 것보다,  
이 과정이 실제로 일어나는 작은 Factory 하나를 직접 만드는 것이 가장 빠르다고 생각했습니다.

그래서 `Figure Beverage Factory`를 만들기 시작했습니다.

전체 Production Line은 Krones의 Beverage Production Line과 Equipment를 참고하고 있으며, Arm Robot 등 학습하고 싶은 기술을 실험하기 위해 Figure Insertion과 같은 가상의 공정을 추가했습니다.

> 이 프로젝트는 Software-based Factory Simulation과 Virtual Control Integration을 중심으로 진행하며, 실제 산업용 Hardware 구축은 현재 Project Scope에 포함하지 않습니다.

---

## Version-based Development

**이 프로젝트는 Top-down 방식으로 공부하기 위해 Version을 나누어 개발합니다.**

기술을 먼저 정하고 억지로 프로젝트에 넣기보다 Factory를 발전시키면서 필요해진 기술을 공부하고 적용하는 방식이 효율적이라고 생각하기 때문입니다.

처음부터 PLC, Robot, Sensor, DES, Optimization을 각각 공부한 뒤 마지막에 Factory를 만드는 구조보다는,

```text id="b8n60w"
전체 Factory를 먼저 만든다
        ↓
직접 돌려본다
        ↓
문제를 발견한다
        ↓
"이 문제를 해결하려면 무엇이 필요하지?"
        ↓
필요한 개념을 공부한다
        ↓
Factory에 적용한다
        ↓
다시 문제를 발견한다
```

라는 구조로 프로젝트를 진행하기로 했습니다.

그래서 각 Version에는 **구체적으로 해보고 싶은 것**이 하나씩 있습니다.

---

## Each Version

```text id="0nqzhm"
V1 - Production Flow
     : Raw Material부터 Finished Goods까지 일단 Factory를 동작시킨다.

V2 - Process Time / Buffer / Queue
     : 각 Unit이 실제로 "시간을 가지고 작업하고 기다리는" 생산 흐름을 만든다.

V3 - Dashboard / KPI
     : 생산량, 가동률, Queue, WIP 등 Factory에서 일어나는 일을 숫자로 본다.

V4 - Bottleneck / Scenario
     : Machine 수, Buffer 크기, Recipe 등을 바꾸면서 어디가 막히고 결과가 어떻게 달라지는지 비교한다.

V5 - Robot / Sensor / Actuator
     : 필요한 Equipment의 내부를 열어보고 실제 동작을 구현한다.

V6 - Virtual PLC / Industrial Communication
     : Control Logic을 Unity 밖으로 분리하고 Virtual Controller와 Factory Simulation을 연결한다.

V7 - Scheduling / Routing / Resource Allocation
     : 여러 선택지 중 어떤 운영 방법이 더 좋은지 Simulation으로 찾는다.
```

즉,

```text id="1wr88v"
Flow
 ↓
Time
 ↓
Data
 ↓
Experiment
 ↓
Automation
 ↓
Virtual Control
 ↓
Decision
```

순서로 Factory의 해상도를 높여가려고 합니다.

현재 Roadmap은 이 흐름을 기준으로 구성했습니다.

따라서 이 Roadmap은 최종 확정된 계획이 아닙니다.

**각 Version을 직접 구현하면서 새롭게 발견한 문제나 학습 결과에 따라 Version의 범위와 순서는 변경될 수 있습니다.**

---

# Project Roadmap

```text id="2t6dgd"
V1  Operational Factory
        ↓
V2  Discrete Event Simulation
        ↓
V3  Monitoring & Analytics
        ↓
V4  Bottleneck & What-if
        ↓
V5  Equipment / Robotics / Material Flow
        ↓
V6  Virtual Control System Integration
        ↓
V7  Decision Support & Optimization
```

---

# V1 — Operational Factory

## 먼저 전체 Factory를 만들어본다.

**첫 번째 Version에서는 Raw Material부터 Finished Goods까지 전체 Production Flow를 실제로 동작시킵니다.**

Factory Simulation을 공부하려고 하는데 처음부터 개별 Equipment 내부 구조만 깊게 구현하면 정작 **전체 생산 시스템이 어떻게 연결되는지 경험하지 못할 것**이라고 생각했습니다.

그래서 V1에서는 현실의 설비를 정밀하게 재현하는 대신:

```text id="8shpzp"
Raw Material
→ Production
→ Inspection
→ Packaging
→ Palletizing
→ Finished Goods
```

라는 전체 흐름을 먼저 만들었습니다.

다만 이후 Version에서 계속 수정할 것을 전제로, Rough하게 구현하더라도 각 요소의 결합도를 낮추려고 했습니다.

### 왜 PackML을 넣었나?

**Factory를 Rough하게 만들더라도 Equipment State까지 임의로 만들고 싶지는 않았습니다.**

이후 Virtual PLC나 Equipment Control까지 발전시킬 가능성을 생각하여 Unit의 Operational State는 PackML을 참고해 설계했습니다.

이를 통해 V1부터:

```text id="efqacx"
Equipment State
≠
Material Processing State
```

라는 구분을 유지하려고 했습니다.

### V1에서 일부러 하지 않은 것

PLC, Sensor, Robot, 실제 Vision, DES 등은 V1에서 구현하지 않았습니다.

필요 없다고 생각해서가 아니라,

> **전체 Factory를 먼저 만들어본 뒤 어떤 요소가 왜 필요한지 알고 추가하고 싶었기 때문입니다.**

---

# V2 — Discrete Event Simulation

## Bottle이 Machine을 기다리고, Machine도 Bottle을 기다리게 만든다.

**V2에서는 `Process Time / Buffer / Queue`를 추가하여 각 공정의 시간과 대기를 표현합니다.**

이 Version을 넣기로 한 이유는 V1을 실제로 돌리면서 바로 문제가 발생했기 때문입니다.

V1에서는 Conveyor가 계속 움직이고 Bottle이 Unit의 Trigger에 들어오면 Process를 수행하도록 구현했습니다.

하지만 전체 Line을 연결하자 Conveyor의 이동과 Unit의 Processing Timing이 맞지 않으면서 일부 Bottle이 의도대로 처리되지 않았습니다.

이 과정에서 처음에는 단순한 Conveyor라고 생각했던 **Buffer의 필요성을 직접 확인했습니다.**

그래서 V2에서는:

```text id="ks1q7u"
Conveyor
    ↓
Enter Zone
    ↓
Machine
    ↓
Exit Zone
    ↓
Conveyor
```

처럼 Material Handoff를 명시적으로 표현하려고 합니다.

그리고 이 구조 위에:

```text id="f3rm5j"
Process Time
Queue
Buffer Capacity
Waiting
Blocking
Starvation
Resource Availability
```

을 추가합니다.

결과적으로:

```text id="5v0t8f"
Bottle이 없어서 Machine이 기다린다.
Machine이 작업 중이라 Bottle이 기다린다.
Downstream이 가득 차서 Machine이 내보내지 못한다.
Buffer가 가득 차서 Upstream이 멈춘다.
```

같은 상황을 Simulation에서 표현하는 것이 목표입니다.

또한 실제 Beverage Line에서는 Accumulation Conveyor, Machine Infeed / Outfeed, Line Control이 어떻게 동작하는지 Krones 사례를 추가로 조사하고, 필요한 의미를 Simulation Model에 반영할 예정입니다.

### Recipe도 Line 전체에서 관리한다.

**각 Unit에 흩어져 있는 생산 설정을 하나의 Production Recipe로 묶고 싶습니다.**

V1에서는 빠른 구현을 위해 필요한 Definition을 각 Unit이 직접 참조하는 부분이 있습니다.

V2에서는:

```text id="25ggsu"
Production Recipe
        ↓
Production Line
        ↓
각 Unit
```

처럼 Line 전체가 하나의 생산 목표를 공유하는 방향으로 변경하려고 합니다.

---

# V3 — Monitoring & Analytics

## Factory에서 무슨 일이 일어나는지 숫자로 본다.

**V3에서는 `Dashboard / KPI`를 추가하여 생산 시스템의 상태를 관찰합니다.**

Bottle이 잘 움직이는지만 눈으로 보는 것으로는 생산 시스템이 잘 동작하고 있는지 판단하기 어렵습니다.

V2에서 시간과 생산 흐름이 의미를 가지게 되면:

```text id="yrz27d"
Throughput
WIP
Cycle / Lead Time
Machine Utilization
Queue Length
Buffer Occupancy
Machine State History
Production / Quality Events
```

등을 수집할 수 있습니다.

예를 들어:

```text id="utg06s"
오늘 몇 개 생산했지?
어떤 Machine이 가장 오래 일했지?
어디에 Bottle이 가장 많이 쌓이지?
Bottle 하나를 생산하는 데 얼마나 걸리지?
```

같은 질문에 UI와 데이터로 답할 수 있도록 만드는 것이 목표입니다.

즉 질문이:

> "Factory가 움직이는가?"

에서

> **"Factory가 얼마나 잘 움직이고 있는가?"**

로 바뀌는 Version입니다.

---

# V4 — Bottleneck & What-if

## Machine과 Buffer 조건을 직접 바꾸면서 Factory를 실험한다.

**V4에서는 `Bottleneck / Scenario Comparison`을 통해 Factory Simulation을 실험 도구로 사용합니다.**

V3에서 특정 Machine 앞에 Queue가 계속 쌓이는 것을 발견했다면:

> "이 Machine이 Bottleneck인가?"

라는 질문을 할 수 있습니다.

그렇다면 실제 Factory를 바꾸기 전에 Simulation에서:

```text id="g01d3m"
Filler를 한 대 추가하면?
Process Time을 줄이면?
Buffer를 크게 만들면?
Machine 하나가 고장나면?
Cola 생산량이 많아지면?
```

같은 조건을 변경해봅니다.

이를 위해:

```text id="t9cd9c"
Process Time
Buffer Capacity
Machine Count
Failure
Production Mix
Routing Policy
```

등을 Scenario Parameter로 만들고 결과를 비교합니다.

### Flavor / Figure에 따라 Machine을 선택한다.

**직선으로만 흐르는 Line에서 벗어나 제품에 따라 사용할 수 있는 Equipment가 달라지는 상황도 만들어보고 싶습니다.**

예를 들어:

```text id="yj0wn8"
Cola   → Cola를 처리할 수 있는 Filler
Orange → Orange를 처리할 수 있는 Filler

Figure A → Figure A를 처리할 수 있는 Inserter
```

처럼 Machine Capability를 정의할 수 있습니다.

그러면:

```text id="5d1wmq"
Recipe
+
Machine Capability
+
Machine State
+
Queue
        ↓
Routing
```

이라는 문제가 생깁니다.

어떤 Routing Policy가 더 좋은 결과를 만드는지도 Simulation으로 비교해보고 싶습니다.

---

# V5 — Equipment / Robotics / Material Flow

## 필요한 Equipment의 내부를 열어본다.

**V5에서는 `Robot / Sensor / Actuator`를 추가하여 지금까지 결과만 표현했던 Equipment의 실제 동작을 필요한 부분부터 구현합니다.**

모든 Equipment를 실제 기계 수준으로 만드는 것이 목적은 아닙니다.

V1~V4에서 하나의 Unit으로 충분했던 Equipment는 그대로 유지하고, 실제 내부 동작을 알아보고 싶은 Equipment만:

```text id="6hgl67"
Unit
 ↓
Parts
 ↓
Sensor / Actuator
```

수준으로 해상도를 높입니다.

### Figure Insertion을 실제 Robot 작업으로 바꾼다.

**Figure Insertion은 처음부터 Robot을 학습하기 위한 확장 지점으로 추가한 가상의 공정입니다.**

V1에서는:

```text id="cr6o8s"
Bottle Enter
→ Figure 생성
→ Insert 완료
```

정도로 결과만 표현했습니다.

V5에서는 이를:

```text id="ymb1yr"
Figure Supply
→ Robot Pick
→ Move
→ Bottle 접근
→ Insert
→ Vision Inspection
```

과 같은 실제 Robot Cycle로 발전시키고 싶습니다.

Robot Arm의 Motion / Pick & Place 등을 구현하고, 필요하다면 ROS2와 Simulation을 연결하는 것도 이 단계에서 실험합니다.

### Palletizing도 Robot 작업으로 발전시킨다.

V1에서는 Pack 8개가 들어오면 Pallet 하나를 생성했습니다.

V5에서는 실제 Palletizing처럼 Robot이 Pack을 Pick하고 Layer Pattern에 맞게 배치하는 동작으로 발전시킬 수 있습니다.

### AMR로 Finished Goods를 Warehouse까지 운반한다.

**V1에서 일부러 멈춰둔 Finished Goods 이후의 물류도 이 단계에서 연결합니다.**

```text id="o16dyk"
Finished Goods
→ Transport Request
→ AMR Pickup
→ 이동
→ Warehouse
→ Drop-off
```

AMR을 단순히 움직이는 Object로 먼저 만들지 않은 이유도 여기에 있습니다.

실제 Intralogistics를 다룰 이유가 생겼을 때 Navigation / Dispatch / Pickup / Drop-off까지 하나의 시스템으로 구현하고 싶었습니다.

ROS2 역시 이 과정에서 사용할 수 있습니다.

---

# V6 — Virtual Control System Integration

## Unity가 혼자 제어하던 Factory를 외부 Virtual Controller와 연결한다.

**V6에서는 `Virtual PLC / Industrial Communication`을 이용하여 Simulation과 Control Logic의 책임을 분리합니다.**

이 프로젝트에서는 실제 산업용 PLC Hardware를 구매하여 연결하는 것을 목표로 하지 않습니다.

대신 Software 기반의 Virtual Controller를 구성하여:

```text id="9td6e1"
┌──── Virtual Controller ────┐
│ PLC Logic                  │
│ Sequence                   │
│ Interlock                  │
│ Equipment Control          │
└────────────┬───────────────┘
             │
     Industrial Communication
             │
┌────────────▼───────────────┐
│ Factory Simulation         │
│                            │
│ Equipment / Material       │
│ Sensor / Actuator          │
│ Process Simulation         │
└────────────────────────────┘
```

처럼 역할을 나누는 것이 목표입니다.

### 왜 Control Logic을 밖으로 분리하나?

**V1~V5에서는 대부분의 판단과 동작이 Simulation 내부에서 이루어지지만, 실제 Automation System에서는 Physical Process와 Controller의 역할이 분리되어 있기 때문입니다.**

예를 들어 Simulation에서 Sensor가 Bottle을 감지하면:

```text id="ptos7p"
Simulation
BottleSensor = TRUE
        ↓
Virtual PLC
Control Logic 실행
        ↓
ConveyorRun = FALSE
FillerStart = TRUE
        ↓
Simulation
Actuator 동작
```

과 같은 흐름을 만들어볼 수 있습니다.

이를 통해:

```text id="yjrg91"
Sensor / Actuator I/O
PLC Logic
Sequence
Interlock
Equipment Control
```

을 Factory Simulation과 분리하여 경험하고 싶습니다.

### Industrial Communication도 이때 사용한다.

Virtual Controller와 Factory Simulation 사이의 통신에는 필요에 따라:

```text id="t8d2ov"
OPC UA
MQTT
REST
```

등을 검토합니다.

통신 기술을 많이 사용하는 것 자체가 목적은 아닙니다.

앞선 Version에서 Factory와 Equipment를 충분히 만들어본 뒤:

```text id="ibkq5p"
어떤 상태를 Controller가 읽어야 하지?
어떤 명령을 Controller가 내려야 하지?
Simulation과 Controller의 상태를 어떻게 맞추지?
통신이 끊기면 각각의 상태를 어떻게 처리하지?
```

같은 질문을 기준으로 필요한 기술을 선택하려고 합니다.

즉 V6의 목적은 실제 Hardware를 구축하는 것이 아니라,

> **실제 장비 없이도 Control System과 Factory Simulation의 Interface 및 Synchronization 구조를 경험하는 것**

입니다.

이를 통해 Software 기반의 Virtual Commissioning / Digital Twin Integration 방향까지 확장해보고 싶습니다.

---

# V7 — Decision Support & Optimization

## 사람이 하나씩 바꿔보던 조건을 시스템이 찾아보게 한다.

**V7에서는 `Scheduling / Routing / Resource Allocation`을 대상으로 더 나은 운영 방법을 찾습니다.**

V4에서는 사람이 직접 Scenario를 만들고:

```text id="rd1u4h"
Machine 한 대 추가
→ Simulation
→ 결과 비교
```

했다면, 마지막에는 한 단계 더 나아가고 싶습니다.

예를 들어:

```text id="dm0ljd"
어떤 순서로 생산해야 할까?
어떤 Machine으로 보내야 할까?
AMR은 어떤 작업부터 처리해야 할까?
Buffer는 얼마나 커야 할까?
Robot을 어디에 배치해야 할까?
```

같은 문제입니다.

이를 위해:

```text id="6f5bbt"
Production Scheduling
Routing
Machine Allocation
Robot Allocation
AMR Dispatch
Buffer Sizing
Resource Allocation
```

등을 대상으로 Simulation과 Optimization / Operations Research를 연결해보고 싶습니다.

즉 Simulation이:

> "이렇게 하면 이런 결과가 나옵니다."

에서 끝나는 것이 아니라,

> **"여러 선택지 중 어떤 방법이 더 나은가?"**

에 답할 수 있도록 발전시키는 것이 마지막 목표입니다.

---

# 결국 이 프로젝트에서 하고 싶은 것

**하나의 Mini Factory를 점점 발전시키면서 제가 관심 있는 일을 직접 경험하고 보여주는 것이 목표입니다.**

```text id="5v43uh"
MODEL
  ↓
SIMULATE
  ↓
OBSERVE
  ↓
ANALYZE / TEST
  ↓
AUTOMATE
  ↓
CONNECT / TWIN
  ↓
OPTIMIZE
```

처음부터 모든 것을 공부한 다음 Factory를 만드는 것이 아니라,

```text id="knmfji"
만든다
 ↓
문제가 생긴다
 ↓
왜 그런지 공부한다
 ↓
필요한 기술을 적용한다
 ↓
다시 발전시킨다
```

를 반복합니다.

그래서 이 프로젝트에서 가장 중요하게 생각하는 원칙은:

> **Data Model은 미래 확장을 고려하되, Interface는 현재 필요한 만큼만 만들고, Implementation은 현재 Version의 목표까지만 구현한다.**

그리고:

> **모델의 상세도는 현실이 얼마나 복잡한지가 아니라, 내가 어떤 질문에 답하고 싶은지가 결정한다.**

입니다.
