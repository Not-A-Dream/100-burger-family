/**
 * PhotoUpload.jslib
 *
 * Unity WebGL ↔ JavaScript 브릿지.
 * C# 측 [DllImport("__Internal")] extern void OpenFilePicker() 와 연결됨.
 *
 * 동작 흐름:
 *   1. Unity가 OpenFilePicker() 호출
 *   2. 브라우저 <input type="file"> 생성 → click()
 *   3. 사용자가 이미지 선택
 *   4. FileReader로 Base64 인코딩
 *   5. SendMessage("PhotoFrame", "OnImageUploaded", base64) → Unity로 전달
 *
 * 지원 형식: PNG, JPEG, GIF, WebP
 *
 * 주의:
 *   - iOS Safari는 사용자 제스처(터치/클릭) 컨텍스트 내에서만 input.click() 동작
 *   - 매 호출마다 새 input 요소 생성: 같은 파일 재선택 시 change 이벤트 재발생 보장
 */
mergeInto(LibraryManager.library, {

  OpenFilePicker: function () {
    // 기존 잔여 input 제거 (안전 처리)
    var existingInput = document.getElementById('__photoFrameInput');
    if (existingInput) {
      document.body.removeChild(existingInput);
    }

    var input = document.createElement('input');
    input.id      = '__photoFrameInput';
    input.type    = 'file';
    input.accept  = 'image/png,image/jpeg,image/jpg,image/gif,image/webp';
    input.style.display = 'none';
    document.body.appendChild(input);

    input.addEventListener('change', function (evt) {
      var file = evt.target.files && evt.target.files[0];

      if (!file) {
        // 파일 선택 취소
        if (input.parentNode) document.body.removeChild(input);
        return;
      }

      // 파일 크기 경고 (10MB 초과)
      if (file.size > 10 * 1024 * 1024) {
        console.warn('[PhotoFrame] 파일 크기가 10MB를 초과합니다. 로딩 시간이 길어질 수 있습니다:', file.size);
      }

      var reader = new FileReader();

      reader.onload = function (e) {
        var dataUrl = e.target.result;

        // "data:image/jpeg;base64,XXXXXX..." → "XXXXXX..."
        // indexOf(',') + 1 로 헤더 부분 제거
        var comma = dataUrl.indexOf(',');
        if (comma === -1) {
          console.error('[PhotoFrame] 유효하지 않은 Data URL 형식입니다.');
          if (input.parentNode) document.body.removeChild(input);
          return;
        }
        var base64 = dataUrl.substring(comma + 1);

        // Unity GameObject "PhotoFrame" 의 OnImageUploaded(string) 메서드 호출
        SendMessage('PhotoFrame', 'OnImageUploaded', base64);

        if (input.parentNode) document.body.removeChild(input);
        console.log('[PhotoFrame] 이미지 전송 완료:', file.name, file.size + ' bytes');
      };

      reader.onerror = function (e) {
        console.error('[PhotoFrame] FileReader 오류:', e);
        if (input.parentNode) document.body.removeChild(input);
      };

      reader.readAsDataURL(file);
    });

    // iOS Safari: 사용자 제스처 컨텍스트가 유지된 상태에서 click() 해야 파일창이 열림
    input.click();
  }

});
