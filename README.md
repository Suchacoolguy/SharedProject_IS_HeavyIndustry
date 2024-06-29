## 주요 업데이트 ##
- WorkManager를 없애고 다른 ViewModel로 모조리 옮김.

### 시바끗 파트배치 최적화 성공!!! ### (5월 27일 04:00AM)
```diff
- 가능한 조합들을 찾고 그 중 최적의 조합을 고르므로 시간이 제법 소요됨.
- 시간 제한을 거는 방식으로 해결이 가넝함. 현재 코드는 10초로 설정해 둠.
- 매번 조합이 달라짐.
- 5초로 설정하면 판타네스트 선배님들이랑 비슷한 정도 성능
- 10초로 설정하면 판타네스트 선배님들보다 더 나은 성능 (스크랩 5000 mm 정도 더 적게 나오는 듯? 평균적으로)
```
## 추후 수정사항 ##
- 분리 체크된 파트 따로 처리하는 것 구현해야 함.
- 드랍할 때 정확히 빈 공간을 타게팅하지 않더라도 드랍되게 바꿔야 함.

## 해야할 것 ##
- 원자재 종류 적게 쓰는 파트배치 알고리즘 추가
- 엑셀에서 데이터 가져올 때 White Space 포함되는 거 수정해야 됨.
- 분리길이 Enter 입력 안 해도 설정되게 바꿔야할 듯
- 드래그앤드랍 -> 드래깅할 때 막대 모양 표시되게 수정
- 드래그앤드랍 -> 새로 생성된 애들 표시하기
- 파트배치 -> 길이초과된 애들 색깔 다르게 표시
- 프로젝트 저장 및 불러오기 기능
- 환경설정 - 이것도 구현할 거 생각보다 많음.

## 현재 진행중인 파트: 상헌 ##
- 파트 임시저장소 구현
- 분리길이 입력 안 했을 때 메시지창 띄우는 것

## 현재 진행중인 파트: 진용 ##
- 테이블 필터링80% 완료
- 필터링된 데이터들에 대해서 제외/분리 설정(완료)
- 엑셀 접근 라이브러리 다른거 시도해봄
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
