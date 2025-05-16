# 🧱 Unity UI Architecture Patterns – MVC / MVP / MVVM

게임 개발 또는 UI/UX 설계에서 **코드 구조를 명확하게 분리**하고  
유지보수하기 좋게 만드는 대표 아키텍처 패턴 3가지를 정리합니다.

- ✅ MVC (Model–View–Controller)
- ✅ MVP (Model–View–Presenter)
- ✅ MVVM (Model–View–ViewModel)

---

## 🧭 1. 흐름 요약

### 🧩 Traditional MVC (고전 MVC)

Player
   ↓ Input
Controller     ← Input processing, Events
   ↓ Updates
Model          ← Game logic
   ↓ OnChange ↑ Observes
View           ← Formatting, Rendering
   ↑ Sees
Player

### 🧩 MVP (Model–View–Presenter)

Player
   ↓ Input ↑ Sees
View            ← Rendering, Events
   ↓ OnEvent ↑ Updates
Presenter       ← Input processing, Formatting
   ↓ Updates ↑ OnChange
Model           ← Game logic

### 🧩 MVVM (Model–View–ViewModel)

Player
   ↓ Input ↑ Sees
View 
   ⇅ Binding
ViewModel 
   ↓ Updates ↑ OnChange
Model 

## 🧠 구성 비교
| 패턴   | View       | 중간 계층                   | Model    |
| ---- | ---------- | ----------------------- | -------- |
| MVC  | 수동 렌더링     | Controller: 입력 처리       | 상태 로직 보관 |
| MVP  | UI + 입력 수신 | Presenter: 로직 + 렌더링 포맷  | 상태 로직    |
| MVVM | 바인딩 + 렌더링  | ViewModel: 상태값 + 바인딩 대상 | 로직/데이터   |

## 🎮 유니티에서의 적용 예
| 구성 요소     | MVC (가능은 함)            | MVP (추천)               | MVVM (비추천)                |
| --------- | ---------------------- | ---------------------- | ------------------------- |
| View (UI) | MonoBehaviour          | MonoBehaviour          | MonoBehaviour (바인딩 필요)    |
| 중간 계층     | Controller             | Presenter (로직)         | ViewModel (Observable 필요) |
| 데이터/로직    | ScriptableObject / 클래스 | 클래스 / ScriptableObject | 클래스 / ScriptableObject    |
| UI 업데이트   | 이벤트 직접 구독              | Presenter가 직접 갱신       | 바인딩 자동화 필요 (외부 도구 필요)     |

## 📊 장단점 비교
| 패턴       | 장점                     | 단점                             |
| -------- | ---------------------- | ------------------------------ |
| **MVC**  | 구조 단순, 역사적으로 검증됨       | 유니티에서는 View-Controller 분리가 어색함 |
| **MVP**  | View와 로직 완전 분리, 테스트 쉬움 | 클래스 수 증가                       |
| **MVVM** | 완전한 역할 분리, 바인딩 자동화     | Unity에선 바인딩 도구가 없음 (복잡함)       |

## ✅ 정리
| 목적                   | 추천 패턴           |
| -------------------- | --------------- |
| UI와 로직을 명확히 분리하고 싶다  | ✅ MVP           |
| 데이터와 UI 상태를 바인딩하고 싶다 | ⚠️ MVVM (도구 필요) |
| 빠르게 단순한 구조로 설계하고 싶다  | ✅ MVC or MVP    |

## ✍️ 결론
🧩 MVC / MVP / MVVM은 모두 역할을 분리하여 유지보수를 쉽게 만들기 위한 구조입니다.
유니티에선 바인딩 도구가 없기 때문에, MVP가 가장 실용적이고
MVVM은 디자이너 협업 or 툴 기반 프로젝트에서만 추천됩니다.