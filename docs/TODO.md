좋아! 지금까지 이야기한 내용을 바탕으로 **Discord Personal Assistant 개발 Todo List**를 정리해 봤어.

단순 기능 목록이 아니라 개발 순서와 시스템 구조까지 고려했어. 아직 확정하지 않은 기능은 선택 항목으로 분리했어.

# Discord Personal Assistant — Todo List

## 1. Core System

- [v] 프로젝트 기본 구조 설계
- [v] Discord Bot Client 구현
- [v] Discord API 연결 및 인증
- [ ] Slash Command Handler 구현
- [v] Event Handler 구현
- [ ] Permission Manager 구현
- [ ] Configuration Manager 구현
- [ ] Logger 구현
- [ ] Database 연결
- [ ] 데이터 저장 및 불러오기
- [ ] 예외 처리 및 오류 복구
- [ ] 자동 재연결 시스템

## 2. Discord Monitor

Discord를 확인하지 못하는 동안 발생한 일을 수집하는 기능.

- [ ] 메시지 수집
- [ ] 멘션 감지
- [ ] 키워드 감지
- [ ] 질문 감지
- [ ] 미답변 질문 감지
- [ ] 중요 메시지 분류
- [ ] 부재중 활동 기록
- [ ] 활동 요약 생성
- [ ] 활동 보고서 전송
- [ ] 긴급 알림 시스템

## 3. Assistant

실제 비서 역할을 담당하는 기능.

### 질문 관리
- [ ] 질문 접수
- [ ] 질문 분류
- [ ] 질문 목록 조회
- [ ] 질문 상태 관리
- [ ] 중복 질문 감지
- [ ] 질문 일괄 보고서 생성
- [ ] 질문 답변 작성
- [ ] 답변 전달
- [ ] 질문 종료 처리

### 일정 및 업무 관리
- [ ] 할 일 등록
- [ ] 할 일 목록 조회
- [ ] 할 일 완료 처리
- [ ] 마감일 설정
- [ ] 반복 일정 등록
- [ ] 일정 알림
- [ ] 업무 상태 관리

### 자동 응답
- [ ] FAQ 관리
- [ ] FAQ 자동 응답
- [ ] 자동 안내 메시지
- [ ] 문의 자동 접수
- [ ] 답변 불가능한 질문 관리자 전달

## 4. Utility

일상적인 Discord 사용을 보조하는 도구.

### 기본 도구
- [ ] Discord Timestamp 변환기
- [ ] 알림 예약
- [ ] 계산기

### 메시지 도구
- [ ] Embed 생성기
- [ ] Embed 편집기
- [ ] 메시지 작성 도구
- [ ] 메시지 템플릿 관리
- [ ] 메시지 미리보기

### URL 도구
- [ ] URL 정보 조회
- [ ] URL 제목 및 설명 추출
- [ ] URL 연결 상태 검사
- [ ] URL 리디렉션 확인
- [ ] 의심스러운 URL 검사

### 파일 도구
- [ ] 파일 업로드 및 공유
- [ ] 파일 목록 관리
- [ ] 파일 다운로드
- [ ] 파일 삭제
- [ ] 파일 공유 기록

## 5. Integration

외부 서비스와 연결하는 기능.

### Webhook Manager
- [ ] Webhook 생성
- [ ] Webhook 삭제
- [ ] Webhook 목록 조회
- [ ] Webhook 정보 수정
- [ ] Webhook 테스트 전송

### External API
- [ ] 외부 API 요청 시스템
- [ ] API 응답 처리
- [ ] API 오류 처리
- [ ] API 연결 설정 관리

## 6. AI Assistant (선택)

- [ ] AI API 연결
- [ ] 메시지 요약
- [ ] 질문 자동 분류
- [ ] 메시지 작성 보조
- [ ] 답변 초안 생성
- [ ] 중요도 판단
- [ ] FAQ 기반 답변
- [ ] 대화 맥락 관리

## 7. Local Environment

내 컴퓨터에서 안정적으로 실행하기 위한 시스템.

- [ ] 로컬 실행 환경 구축
- [ ] Windows 시작 시 자동 실행
- [ ] 비정상 종료 감지
- [ ] 자동 재시작
- [ ] 프로세스 상태 확인
- [ ] 데이터 백업
- [ ] 데이터 복구
- [ ] 설정 파일 관리
- [ ] 프로그램 업데이트 구조

## 8. Dashboard (선택)

나중에 Discord 외부에서도 관리할 수 있도록 하는 기능.

- [ ] 로컬 웹 서버 구축
- [ ] 대시보드 UI 제작
- [ ] 봇 상태 확인
- [ ] 질문 목록 조회
- [ ] 질문 답변
- [ ] 활동 보고서 조회
- [ ] 알림 설정
- [ ] 모듈 설정
- [ ] 로그 조회

---

# 9. 개발 단계

위 Todo를 실제 개발 순서로 정리하면 다음과 같아.

| 단계 | 목표 | 주요 작업 |
|---|---|---|
| Phase 1 | 기반 구축 | Core, Discord 연결, Database |
| Phase 2 | 기본 비서 | Timestamp, Reminder, Embed |
| Phase 3 | 정보 수집 | Monitor, Mention, Question |
| Phase 4 | 업무 관리 | 질문 관리, 일정, 할 일 |
| Phase 5 | 편의 기능 | URL, File, Webhook |
| Phase 6 | 지능화 | AI 요약, 분류, 답변 |
| Phase 7 | 안정화 | 자동 실행, 복구, 백업 |
| Phase 8 | 확장 | Local Dashboard |

# 10. 추가 설계 원칙

개발에 들어가기 전에 다음 원칙도 정해 두는 걸 추천해.

| 항목 | 방향 |
|---|---|
| 실행 환경 | Windows Local |
| 통신 | Discord API / Gateway |
| 데이터베이스 | SQLite 권장 |
| 구조 | 모듈 기반 |
| 명령어 | Slash Command |
| 설정 | JSON 또는 Database |
| 로그 | 파일 기반 |
| AI | 외부 API 연동 |
| 확장성 | 서버 배포 가능하도록 설계 |

### ⭐ 개인적으로 가장 중요한 개발 원칙

**기능별 독립성을 보장할 것.**

예를 들어 URL Inspector가 고장 나더라도 Reminder나 Question Manager는 정상적으로 작동해야 해.

또한 Monitor와 Assistant는 서로 연결하되 독립적으로 동작하도록 만드는 게 좋아.

---

이 Todo List를 프로젝트의 초기 개발 명세로 사용해도 좋겠어.

처음부터 모든 기능을 만들기보다는 Phase 1부터 차례대로 구현하는 걸 추천해. 특히 이번 프로젝트는 단순한 봇이 아니라 네 Discord 생활을 보조하는 프로그램이니까, 기능의 개수보다는 실제로 네가 Discord를 확인하는 시간을 얼마나 줄여 주는지가 중요해!