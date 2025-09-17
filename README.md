# Dayvive
라이트 캐주얼 생존 게임 (Unity)

---

## 🎯 현재 진행 상황 (Current Progress)
- **Player Cursor 이동**
- **Range 시각화 (CombatAimGuide)**
- **Mining System (Tick 기반)**
- **Stage Spawner (리소스 자동 배치)**
- **Projectile 발사 (CombatShooter / Projectile)**
- **Hit Effect & QuickFade 적용**

---

## 🛠️ Next 목표 (Next Tasks)
- **데미지 분기 (Enemy vs Mineable)**
- **총기 데이터 분리 (WeaponData 구조화)**
- **Dummy Enemy Prefab 제작 및 전투 테스트**
- **Outgame 루프 설계 (소비/업그레이드)**

---

## 🔮 장기 계획 (Future Plans)
- 다양한 무기 및 사거리/속도/데미지 튜닝
- Range vs Sensor 디버깅
- Day Timer 시스템
- Outgame 확장 (퀘스트, 업그레이드, 자원 흐름)

---

## 📌 기술 스택
- **엔진**: Unity 6 (6000.0 LTS) 2D
- **버전 관리**: GitHub / Git
- **언어**: C#

---

## 📂 주요 스크립트
- `CombatShooter.cs` : 전투 모드에서의 발사 로직
- `Projectile.cs` : 탄환 이동, 사거리 판정, 히트 처리
- `CombatAimGuide.cs` : 가이드 라인 시각화
- `QuickFade.cs` : 히트 이펙트 자연스러운 페이드아웃
- `CombatAmmo.cs` : 탄약 관리
- `PlayerMode.cs` : 플레이어 상태 관리 (Mining/Combat)

---
