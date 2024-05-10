
### Services/BOMDataService.cs ###
BOM 엑셀파일에서 데이터 불러옴.  
git pull 해서 실행하려면 엑셀 파일경로 바꿔야할 것.  
**짐은 현재 사용하지 않고 있음. 가라데이터로 구현하는 중임.**  

### Services/ArrangePartsService.cs ###
'파트 배치 결과' 가라 데이터 만드는 자슥.  드래그 앤 드랍 테스트용으로 사용함.   얘도 액셀 파일경로 바꿔야 할 것.  

### Converters/DivideByTenConverter.cs ###
Length 값대로 막대 길이 설정하면 넘 길어져서 10으로 나눌려고 컨버터 만든 것. 신경 안 써도 됨.

### Services/ ###
데이터 가져오는 애들은 여기다가 따로 빼려고 함.

### 드래그앤드랍 관련 파일 ###
#### 정리가 안돼서 아직 뒤죽박죽쓰.. 곧 정리할 것 ####
- DragAndDropViewModel.cs
- BOMDataView.axaml
- BOMDataView.axaml.cs
- etc)
