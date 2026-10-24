# 코드 서명 설정

이 저장소는 공개 릴리스의 `SpeakerWidget.exe`를 SignPath Foundation을 통해 무료로 서명할 수 있도록 준비되어 있습니다.

## 최초 설정

1. [SignPath Foundation 신청 페이지](https://signpath.org/apply.html)에서 이 저장소를 신청합니다.
2. SignPath에서 GitHub.com을 Trusted Build System으로 연결하고 이 저장소에 SignPath GitHub App을 설치합니다.
3. SignPath 프로젝트에서 ZIP 안의 `SpeakerWidget.exe`에 Authenticode 서명을 적용하는 Artifact Configuration과 Signing Policy를 만듭니다.
4. GitHub 저장소의 **Settings → Secrets and variables → Actions**에 다음 값을 등록합니다.

| 종류 | 이름 | 값 |
| --- | --- | --- |
| Secret | `SIGNPATH_API_TOKEN` | SignPath 제출자 API 토큰 |
| Variable | `SIGNPATH_ORGANIZATION_ID` | SignPath 조직 ID |
| Variable | `SIGNPATH_PROJECT_SLUG` | SignPath 프로젝트 slug |
| Variable | `SIGNPATH_SIGNING_POLICY_SLUG` | SignPath 서명 정책 slug |

API 토큰은 저장소에 직접 기록하지 않습니다.

## 서명 릴리스 만들기

1. 릴리스용 커밋에 `v1.0.1` 같은 태그와 GitHub Release를 만듭니다.
2. GitHub의 **Actions → Sign Windows release → Run workflow**에서 해당 태그를 선택합니다.
3. SignPath에서 요청을 확인하고 승인합니다.
4. 워크플로가 서명을 검증한 뒤 서명된 ZIP을 해당 GitHub Release에 올립니다.

서명 전에 빌드 결과가 GitHub Actions artifact로 등록되므로 SignPath가 소스와 빌드 출처를 확인할 수 있습니다. 모든 릴리스는 SignPath 정책에 따라 수동 승인을 거칩니다.
