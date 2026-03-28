using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 가족사진 액자 — 플레이어가 [E]를 누르면 이미지 업로드 다이얼로그 열림.
///
/// ══════════════════════════════════════════════════
///  동작 원리 (WebGL 빌드)
/// ══════════════════════════════════════════════════
///
///   [E] 누름
///     ↓
///   PhotoUpload.jslib → OpenFilePicker()
///     ↓ (브라우저 파일 선택창)
///   FileReader.readAsDataURL(file)
///     ↓ PNG/JPEG → Base64 변환
///   SendMessage("PhotoFrame", "OnImageUploaded", base64)
///     ↓
///   OnImageUploaded() → Texture2D.LoadImage(bytes)
///     ↓
///   PhotoCanvas 메시 렌더러에 텍스처 적용
///
/// ══════════════════════════════════════════════════
///  주의사항
/// ══════════════════════════════════════════════════
///   - jslib의 SendMessage 첫 인자가 "PhotoFrame" (오브젝트 이름) 과 일치해야 함
///   - Editor 모드에서는 파일 업로드 불가 (테스트 격자 텍스처 적용)
///   - iOS Safari: 사용자 제스처 컨텍스트 내에서만 파일 선택창 열림 → [E] 키 입력이 트리거 역할
/// </summary>
public class PhotoFrame : Interactable
{
    [Header("사진 액자")]
    [Tooltip("사진이 표시될 메시 렌더러 — PhotoCanvas 오브젝트를 연결하세요")]
    public Renderer photoRenderer;

    bool _isLoading;

    // ── WebGL 전용: jslib 함수 선언 ──────────────────────────────
    // 왜 DllImport("__Internal")?
    //   Unity WebGL은 C# → JavaScript 호출 시 "__Internal" 라이브러리명을 사용.
    //   .jslib 파일에 정의된 함수를 C# 쪽에서 extern으로 선언하면 자동 연결됨.
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void OpenFilePicker();
#endif

    // ── Interactable 오버라이드 ──────────────────────────────────

    public override string GetPrompt()
        => _isLoading ? "사진 불러오는 중..." : "[E] 가족사진 바꾸기";

    public override void Interact(PlayerHand hand)
    {
        if (_isLoading) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        OpenFilePicker();
#else
        // Editor / 비-WebGL 테스트용: 격자 패턴 텍스처 적용
        Debug.Log("[PhotoFrame] WebGL 빌드에서만 실제 업로드가 가능합니다. 테스트 텍스처를 적용합니다.");
        ApplyTestTexture();
#endif
    }

    // ── JavaScript → Unity 콜백 ──────────────────────────────────

    /// <summary>
    /// jslib의 SendMessage("PhotoFrame", "OnImageUploaded", base64) 로 호출됨.
    /// GameObject 이름이 "PhotoFrame" 이어야 함 — 이름 변경 금지.
    /// </summary>
    public void OnImageUploaded(string base64)
    {
        if (string.IsNullOrEmpty(base64))
        {
            Debug.LogWarning("[PhotoFrame] 빈 Base64 데이터를 받았습니다. 파일 선택이 취소됐거나 읽기 오류입니다.");
            return;
        }
        StartCoroutine(ApplyBase64Image(base64));
    }

    // ── 내부 메서드 ──────────────────────────────────────────────

    IEnumerator ApplyBase64Image(string base64)
    {
        _isLoading = true;
        yield return null; // 한 프레임 대기 → "불러오는 중..." 프롬프트 표시

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PhotoFrame] Base64 디코딩 실패: {e.Message}");
            _isLoading = false;
            yield break;
        }

        // Texture2D.LoadImage: PNG / JPEG 자동 감지, 크기 자동 조절
        // 왜 RGBA32? → 알파 채널 포함 PNG도 손실 없이 처리 가능
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(bytes))
        {
            Debug.LogError("[PhotoFrame] 이미지 디코딩 실패 — 지원 형식: PNG, JPEG");
            Destroy(tex);
            _isLoading = false;
            yield break;
        }

        tex.wrapMode   = TextureWrapMode.Clamp;   // 테두리 반복 방지
        tex.filterMode = FilterMode.Bilinear;      // 부드러운 스케일링

        ApplyTexture(tex);
        _isLoading = false;
        Debug.Log($"[PhotoFrame] ✅ 가족사진 적용 완료! ({tex.width}×{tex.height}px)");
    }

    void ApplyTexture(Texture2D tex)
    {
        if (photoRenderer == null)
        {
            Debug.LogWarning("[PhotoFrame] photoRenderer가 연결되어 있지 않습니다.");
            return;
        }
        var mat = photoRenderer.material;
        // URP: _BaseMap / Standard: _MainTex 모두 대응
        mat.SetTexture("_BaseMap", tex);
        mat.SetTexture("_MainTex", tex);
    }

    void ApplyTestTexture()
    {
        // Editor 테스트: 16×16 격자 패턴 (따뜻한 크림/갈색)
        var tex = new Texture2D(64, 64);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                bool edge = (x % 16 == 0 || y % 16 == 0);
                tex.SetPixel(x, y, edge
                    ? new Color(0.72f, 0.60f, 0.44f)
                    : new Color(0.96f, 0.91f, 0.82f));
            }
        }
        tex.Apply();
        ApplyTexture(tex);
        Debug.Log("[PhotoFrame] 테스트 텍스처 적용됨.");
    }
}
