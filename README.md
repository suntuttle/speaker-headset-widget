# Speaker Headset Switcher

Windows 알림 영역에서 스피커와 헤드셋을 전환하고 볼륨을 조절하는 가벼운 도구입니다.

## 주요 기능

- 트레이 아이콘 좌클릭으로 스피커 ↔ 헤드셋 전환
- 아이콘 위에서 마우스 휠로 볼륨 5% 조절
- 가운데 클릭으로 음소거 전환
- 툴팁에 현재 출력 장치와 볼륨 표시
- 장치를 다시 연결해 이름 앞 번호나 ID가 바뀌어도 자동 재인식
- 관리자 권한, 설치 프로그램, NirCmd 불필요

## 설치 및 사용

GitHub Releases에서 ZIP 파일을 내려받아 원하는 폴더에 압축을 풀고 `SpeakerWidget.exe`를 실행하세요.

- 좌클릭: 출력 장치 전환
- 휠: 볼륨 조절
- 가운데 클릭: 음소거
- 우클릭: 장치 지정 및 설정

자세한 설명은 [사용방법.md](사용방법.md)를 참고하세요.

## 지원 환경

- Windows 10/11 64비트
- .NET Framework 4.x

## 빌드

```powershell
.\build.ps1
```

## 개인정보 보호

이 프로그램은 오디오 장치 전환과 볼륨 조절을 로컬 컴퓨터 안에서만 수행합니다. 사용자가 명시적으로 요청하지 않는 한 어떠한 정보도 네트워크로 전송하지 않습니다.

프로그램을 제거하려면 실행 중인 트레이 아이콘을 우클릭해 종료한 다음, 압축을 푼 폴더와 사용자가 직접 만든 시작 프로그램 바로가기를 삭제하면 됩니다.

## Code signing policy

무료 코드 서명은 [SignPath.io](https://about.signpath.io/)에서 제공하며 인증서는 [SignPath Foundation](https://signpath.org/)에서 제공합니다.

- Committer, reviewer, approver: [suntuttle](https://github.com/suntuttle)
- 릴리스 서명은 GitHub Actions에서 공개된 소스로 빌드한 결과에만 적용합니다.
- 모든 서명 요청은 프로젝트 관리자가 SignPath에서 직접 확인하고 승인합니다.
- 개인정보 보호 정책: 이 프로그램은 사용자가 명시적으로 요청하지 않는 한 어떠한 정보도 다른 네트워크 시스템으로 전송하지 않습니다.
- 설정 방법과 릴리스 절차는 [SIGNING.md](SIGNING.md)를 참고하세요.

## 라이선스

[MIT License](LICENSE)

