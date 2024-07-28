## 주요 업데이트 ##
- WorkManager를 없애고 다른 ViewModel로 모조리 옮김.

### 시바끗 파트배치 최적화 성공!!! ### (5월 27일 04:00AM)
```diff
- 가능한 조합들을 찾고 그 중 최적의 조합을 고르므로 시간이 제법 소요됨.
- 시간 제한을 거는 방식으로 해결이 가넝함.
- 찾은 솔루션(조합) 개수로 제한을 걸 수도 있음. (50개 정도 찾으면 그만두게 설정해서 너무 오래 걸리는 것 방지)
- 매번 조합이 달라짐.
```

## 7월 12일 아이에스 방문 피드백 (완료된 것은 ~~이렇게~~ 표기) ##

- ~~로스최적화 알고리즘으로 다시 복구 && 배치 안 되는 자재 발생하는 문제 해결~~
- 파트배치 탭에서 규격 선택하지 않은 애들 있을 때 레포트 출력하면 알림창 띄우기
    - #### ~~레포트 출력 버튼 클릭시 파트배치 안한 규격들 배치하는 작업 구현 완료~~
- ~~파트배치에서 기존 원자재에 파트 추가할 때 원자재 길이 늘려서 넣을 수 있으면 원자재 길이 변경~~
- ~~파트 빠질 때에도 원자재 길이 줄일 수 있으면 줄이기~~
- 형강목록에 있는데 규격 목록에 없는 애가 있으면 ‘신규 규격이 있음. 추가하겠음?’ 알림창 띄우기
- ~~분리필요 버튼 한 번 더 클릭했을 때는 전체 목록으로 돌아오게 수정~~
- ~~‘제외, 분리’ 체크된 애들도 따로 볼 수 있어야 함~~
- ~~분리길이 일괄적용 기능 추가~~
- ~~절단 손실분 스크랩에 적용하기 (양 끝단 손실분은 하지말고 중간에 절단 손실분만)~~
    - 절단 손실분 설정하는 버튼 하나 추가해야할 듯 (SettingsViewModel - Cutting Loss 변수)
- ~~총 파트길이 드래그앤드랍시 업데이트되게~~

## 해야할 것 ##
- ~~원자재 종류 적게 쓰는 파트배치 알고리즘 추가~~
- ~~엑셀에서 데이터 가져올 때 White Space 포함되는 거 수정해야 됨.~~
- ~~분리길이 Enter 입력 안 해도 설정되게 바꿔야할 듯~~
- ~~드래그앤드랍 -> 드래깅할 때 막대 모양 표시되게 수정~~
- ~~드래그앤드랍 -> 새로 생성된 애들 표시하기~~
- ~~파트배치 -> 길이초과된 애들 색깔 다르게 표시~~
- 프로젝트 저장 및 불러오기 기능
- ~~환경설정 - 이것도 구현할 거 생각보다 많음.~~

## 현재 진행중인 파트: 상헌 ##
- 프로젝트 저장/불러오기 DB로 구현
- ~~파트배치에서 자동으로 원자재 크기 변경~~

## 현재 진행중인 파트: 진용 ##
- 테이블 필터링 중첩 완료 
- 테이블에 나머지 필터 구현 
- 
### Converters/DivideByTenConverter.cs ###
Length 값대로 막대 길이 설정하면 넘 길어져서 10으로 나눌려고 컨버터 만든 것. DragAndDropView처럼 파트 시각화해서 디스플레이하는 뷰에서 사용. 

### Services/ ###
데이터 가져오는 애들은 여기다가 따로 빼려고 함.

### FilterMenu.axaml ###
이거 UI 미리보기용으로 만든거라 신경안써도 됨

### 드래그앤드랍 관련 파일 ###
- DragAndDropViewModel.cs
- DragAndDropView.axaml
- DragAndDropView.axaml.cs
- etc)
```
using Squirrel;
using System;
using System.Threading.Tasks;


public async Task CheckForUpdates()
        {
            try
            {
                using (var mgr = new UpdateManager("https://github.com/Suchacoolguy/SharedProject_IS_HeavyIndustry/releases/tag/v0.00"))
                {
                    await mgr.UpdateApp();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions here (log them, show a message to the user, etc.)
                Console.WriteLine("Update failed: " + ex.Message);
            }
        }
```

```
InitializeComponent();
CheckForUpdates().ConfigureAwait(false);
```
