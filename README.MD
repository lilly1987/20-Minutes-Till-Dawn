# 플러그인 사용 방법
 
 ## BepInEx 설치 필요 

https://github.com/BepInEx/BepInEx/releases  
exe있는게임폴더에 압축 해제 

아래 스샷처럼 넣기  
![2022-10-10 17 15 20](https://user-images.githubusercontent.com/20321215/194823924-fc419cab-27b1-4d11-a5e3-8e55c4b472e2.png)


## 플러그인 dll 넣는 위치

exe있는게임폴더\BepInEx\plugins

아래 스샷처럼 넣기  
![2022-10-10 17 15 53](https://user-images.githubusercontent.com/20321215/194823986-a4ec5024-bd38-4ca1-81a8-335041293b4e.png)



# 기능

- 포인트 10만 추가
- 새로고침 무한대
- 무조건 생존
- 체력 변화 없음
- 최대 체력 0이 안됨
- 탄약 사용 없음
- 경험치 배율
- 경험치 줍기 거리 보너스
- 시야 배율(파워업엔 적용 안됨. 사실상 )
- 자동 파워업
- 파워업 메뉴
- 모든 파워 1개식 추가
- 파워별 추가(3개 제한 무시)
- 적 소환 시작 시간 반으로(60초 소환 시작이 30초에 시작)
- 적 소환 갯수 배율(1배 2배 등)
- 적 체력 배율
- 적 이동 속도 배율


# 비주얼 스튜디오로 플러그인 직접 수정할경우

## 비주얼 스튜디오 패키지 관리자 설정

"도구 - 패키지 관리자 - 설정 - 패키지 소스"에서 아래 링크 추가  
https://nuget.bepinex.dev/v3/index.json  


## dll 참조 추가

게임명_Data\Managed\Assembly-CSharp.dll  
게임명_Data\Managed\UnityEngine.dll  

- 버전에 따라 아래 dll 도 추가해야 컴파일 가능  
  이건 직접 dnspy같은걸로 "게임명_Data\Managed"안의 dll 에서 어딧는지 찿아보는 수밖에

UnityEngine.CoreModule.dll  
UnityEngine.IMGUIModule.dll  
