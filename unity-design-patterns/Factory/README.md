# 🏭 Factory Pattern in Unity

## 📖 개요

**Factory 패턴**은 객체 생성 로직을 별도의 클래스(또는 메서드)에 위임하여  
`new`나 `Instantiate()` 호출을 분리하고, **유연하고 테스트 가능한 구조**를 만드는 디자인 패턴입니다.

---

## ✅ 유니티에서의 활용 예

| 예시                | 설명 |
|---------------------|------|
| 적/무기/아이템 생성  | 타입에 따라 오브젝트 생성
| 프리팹 종류에 따라 인스턴스화 | `WeaponFactory.CreateWeapon(type)`
| 풀링 + 생성 분리     | ObjectPool과 결합도 가능

---

## 🧱 구조

Player.cs → WeaponFactory.cs → Gun / Bow (IWeapon 인터페이스)

Player: 무기 사용 (생성 책임 없음)
WeaponFactory: 무기 생성
Gun/Bow: 실제 무기 구현체

## ✅ 장점

생성 책임 분리: Player는 무기 생성 세부 사항을 몰라도 됨
확장 쉬움: 무기 추가 시 enum과 switch만 수정
테스트 유리: 팩토리만 바꿔서 모의(Mock) 객체 주입 가능

## ❗ 단점
클래스/구조가 조금 늘어남
너무 단순한 오브젝트는 Factory까지 필요 없을 수도 있음