using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

// Animation khủng long bằng DOTween, xoay các bone (cha) đã đặt pivot ở khớp.
// Góc z: âm = xoay theo chiều kim đồng hồ (đầu quay sang phải -> âm là cúi xuống / hàm há ra)
//  - Bite : thân chúi + cúi cổ + há miệng nhanh -> đớp cắm miệng xuống đất -> giữ nhịp gặm -> về
//  - Roar : ngửa hẳn ra sau lấy đà -> thân lao tới, đầu hất lên, há to gầm -> nhiều nhịp sóng xung kích từ miệng -> về
//  - Move : 2 chân đung đưa nhẹ, loop
public class DinoAnimator : MonoBehaviour
{
    [Title("Bones")]
    public Transform body;
    public Transform tail;
    public Transform neck;
    public Transform head;
    public Transform jaw;
    public Transform legFront;
    public Transform legBack;
    [Tooltip("Điểm chóp miệng (con của Head) - nơi phát sóng gầm")]
    public Transform mouthPoint;

    [Title("Bite")]
    [Tooltip("Lúc đớp: thân -16 / cổ -35.5 / đầu -20 thì miệng chạm đúng mặt đất (đáy chân)")]
    public float biteBodyAngle = -16f;
    public float biteNeckAngle = -35.5f;
    public float biteHeadAngle = -20f;
    [Tooltip("Lúc há: cổ còn cao hơn lúc đớp bao nhiêu độ (đớp sẽ nện từ đây xuống đất)")]
    public float biteOpenNeckOffset = 10f;
    public float biteJawOpen = -32f;
    [Tooltip("Hàm khép quá 1 chút lúc đớp cho có lực")]
    public float biteJawClose = 3f;
    public float biteTailAngle = 8f;
    public float biteOpenTime = 0.14f;
    public float biteChompTime = 0.07f;
    public float biteHoldTime = 0.32f;
    [Tooltip("Đầu giật khi gặm (độ)")]
    public float biteHoldShake = 5f;
    public float biteReturnTime = 0.28f;

    [Title("Roar - lấy đà")]
    public float roarWindupBody = 8f;
    public float roarWindupNeck = 14f;
    public float roarWindupHead = 12f;
    public float roarWindupTail = -8f;
    public float roarWindupTime = 0.32f;
    [Tooltip("Thân lùi ra sau lúc lấy đà (local X)")]
    public float roarWindupBack = -0.25f;

    [Title("Roar - gầm")]
    public float roarBodyAngle = -5f;
    public float roarNeckAngle = -4f;
    [Tooltip("Đầu hất lên (dương) cho miệng mở về phía trước")]
    public float roarHeadAngle = 24f;
    [Tooltip("Hàm há so với đầu")]
    public float roarJawOpen = -46f;
    public float roarTailAngle = 10f;
    [Tooltip("Thân lao tới lúc gầm (local X)")]
    public float roarLunge = 0.45f;
    public float roarThrustTime = 0.09f;
    public float roarHoldTime = 0.75f;
    [Tooltip("Rung đầu + thân khi đang gầm")]
    public float roarHeadShake = 4f;
    public float roarBodyShake = 0.06f;
    public float roarReturnTime = 0.35f;

    [Title("Shockwave")]
    [Tooltip("Object chứa Glow + Ring, sẽ được đặt tại mouthPoint lúc gầm")]
    public Transform shockwave;
    public SpriteRenderer shockwaveGlow;   // fx_glow: phần tròn mờ
    public SpriteRenderer shockwaveRing;   // Circle_Outline_Thick: viền sóng
    [Tooltip("Bán kính sóng lúc toả hết (world unit) = bán kính AOE")]
    public float shockwaveRadius = 3.2f;
    [Tooltip("Đẩy tâm sóng ra trước miệng (world X) để vùng AOE nằm phía trước, ít đè lên thân")]
    public float shockwaveForward = 1.2f;
    public float shockwaveTime = 0.45f;
    [Tooltip("Số vòng sóng trong lúc gầm")]
    public int shockwavePulses = 3;
    public float shockwavePulseGap = 0.16f;
    [Range(0f, 1f)] public float glowAlpha = 0.5f;
    [Range(0f, 1f)] public float ringAlpha = 0.85f;

    [Title("Sound")]
    public AudioClip biteSfx;
    public AudioClip roarSfx;
    [Tooltip("Lệch cao độ ngẫu nhiên mỗi lần phát cho đỡ nhàm")]
    public float pitchJitter = 0.08f;

    [Title("Move")]
    public float legSwing = 12f;
    [Tooltip("Thời gian 1 nhịp bước (chân ra -> chân về)")]
    public float stepTime = 1f;

    // Hook cho gameplay sau này
    public event Action OnBiteHit;                    // lúc miệng cắm xuống đất
    public event Action<Vector3, float> OnShockwave;  // tâm, bán kính AOE

    // Cho gameplay chỉnh lại tâm sóng (vd kéo xuống làn đánh cho trúng lính). Null = trước miệng.
    public Func<Vector3, Vector3> shockwaveCenterResolver;

    // Kích thước gốc của sprite khi scale = 1 (world unit, đường kính) để quy ra bán kính
    const float GLOW_DIAMETER = 10.24f;   // fx_glow 1024px / ppu 100
    const float RING_DIAMETER = 5.12f;    // Circle_Outline_Thick 256px / ppu 50

    private Sequence _attack;
    private Sequence _move;
    private Sequence _wave;
    private bool _moving;
    private Vector3 _bodyBasePos;

    public bool IsMoving => _moving;
    public bool IsAttacking => _attack != null && _attack.IsActive() && !_attack.IsComplete();

    // Pause game (popup) -> đóng băng animation đang chạy
    public void Pause()
    {
        _attack?.Pause();
        _move?.Pause();
        _wave?.Pause();
    }

    public void Resume()
    {
        _attack?.Play();
        _move?.Play();
        _wave?.Play();
    }

    void Awake()
    {
        if (body != null) _bodyBasePos = body.localPosition;
        HideShockwave();
    }

    void OnDestroy() => KillAll();

    // ================= BITE =================
    [Button(ButtonSizes.Large), GUIColor(1f, 0.6f, 0.6f), DisableInEditorMode]
    public void PlayBite()
    {
        ResetPose();
        _attack = DOTween.Sequence().SetLink(gameObject);

        // chúi thân + cúi cổ + há miệng nhanh
        _attack.Append(Rot(body, biteBodyAngle * 0.8f, biteOpenTime, Ease.OutQuad));
        _attack.Join(Rot(neck, biteNeckAngle + biteOpenNeckOffset, biteOpenTime, Ease.OutQuad));
        _attack.Join(Rot(head, biteHeadAngle, biteOpenTime, Ease.OutQuad));
        _attack.Join(Rot(jaw, biteJawOpen, biteOpenTime, Ease.OutQuad));
        _attack.Join(Rot(tail, biteTailAngle, biteOpenTime, Ease.OutQuad));

        // đớp: nện miệng xuống chạm đất + khép hàm
        _attack.Append(Rot(body, biteBodyAngle, biteChompTime, Ease.InQuad));
        _attack.Join(Rot(neck, biteNeckAngle, biteChompTime, Ease.InQuad));
        _attack.Join(Rot(jaw, biteJawClose, biteChompTime, Ease.InQuad));
        _attack.AppendCallback(() =>
        {
            PlaySfx(biteSfx);
            OnBiteHit?.Invoke();
        });

        // giữ nhịp gặm: đầu giật, thân nảy nhẹ
        _attack.Append(head.DOPunchRotation(new Vector3(0f, 0f, biteHoldShake), biteHoldTime, 8, 0.5f));
        _attack.Join(body.DOPunchPosition(new Vector3(0f, -0.05f, 0f), biteHoldTime * 0.5f, 6, 0.5f));

        AppendReturn(_attack, biteReturnTime);
    }

    // ================= ROAR =================
    [Button(ButtonSizes.Large), GUIColor(1f, 0.85f, 0.4f), DisableInEditorMode]
    public void PlayRoar()
    {
        ResetPose();
        _attack = DOTween.Sequence().SetLink(gameObject);

        // lấy đà: ngửa hẳn ra sau, thân lùi, miệng hé
        _attack.Append(Rot(body, roarWindupBody, roarWindupTime, Ease.OutCubic));
        _attack.Join(body.DOLocalMoveX(_bodyBasePos.x + roarWindupBack, roarWindupTime).SetEase(Ease.OutCubic));
        _attack.Join(Rot(neck, roarWindupNeck, roarWindupTime, Ease.OutCubic));
        _attack.Join(Rot(head, roarWindupHead, roarWindupTime, Ease.OutCubic));
        _attack.Join(Rot(jaw, roarJawOpen * 0.25f, roarWindupTime, Ease.OutCubic));
        _attack.Join(Rot(tail, roarWindupTail, roarWindupTime, Ease.OutCubic));
        // tiếng gầm bắt đầu từ cuối nhịp lấy đà để đỉnh âm rơi đúng lúc bung ra
        _attack.InsertCallback(roarWindupTime * 0.6f, () => PlaySfx(roarSfx));

        // bung ra: thân lao tới, cổ vươn, đầu hất lên, hàm há hết cỡ
        _attack.Append(Rot(body, roarBodyAngle, roarThrustTime, Ease.OutQuad));
        _attack.Join(body.DOLocalMoveX(_bodyBasePos.x + roarLunge, roarThrustTime).SetEase(Ease.OutQuad));
        _attack.Join(Rot(neck, roarNeckAngle, roarThrustTime, Ease.OutQuad));
        _attack.Join(Rot(head, roarHeadAngle, roarThrustTime, Ease.OutQuad));
        _attack.Join(Rot(jaw, roarJawOpen, roarThrustTime, Ease.OutQuad));
        _attack.Join(Rot(tail, roarTailAngle, roarThrustTime, Ease.OutQuad));
        _attack.AppendCallback(PlayShockwave);

        // gầm: đầu + thân rung
        _attack.Append(head.DOShakeRotation(roarHoldTime, new Vector3(0f, 0f, roarHeadShake), 22, 0f, false));
        _attack.Join(body.DOShakePosition(roarHoldTime, new Vector3(roarBodyShake, roarBodyShake, 0f), 22, 90f, false, false));

        AppendReturn(_attack, roarReturnTime);
    }

    [Button, DisableInEditorMode]
    public void PlayShockwave()
    {
        if (shockwave == null) return;

        _wave?.Kill();
        HideShockwave();

        // Tâm sóng: trước chóp miệng (lấy theo tư thế lúc gầm, không phải tư thế đứng yên)
        Vector3 mouth = mouthPoint != null ? mouthPoint.position : shockwave.position;
        Vector3 center = mouth + Vector3.right * Mathf.Sign(transform.lossyScale.x) * shockwaveForward;
        if (shockwaveCenterResolver != null) center = shockwaveCenterResolver(center);
        shockwave.position = center;
        shockwave.gameObject.SetActive(true);

        // shockwaveRadius tính theo world -> chia scale của cha (dino có thể bị thu nhỏ / lật)
        float parentScale = Mathf.Max(0.0001f, Mathf.Abs(shockwave.parent != null ? shockwave.parent.lossyScale.y : 1f));
        float glowScale = shockwaveRadius * 2f / GLOW_DIAMETER * 1.25f / parentScale;   // glow mờ dần ra rìa nên to hơn chút
        float ringScale = shockwaveRadius * 2f / RING_DIAMETER / parentScale;

        _wave = DOTween.Sequence().SetLink(gameObject);

        // phần mờ: bung 1 lần
        if (shockwaveGlow != null)
        {
            var t = shockwaveGlow.transform;
            t.localScale = Vector3.one * glowScale * 0.2f;
            SetAlpha(shockwaveGlow, glowAlpha);
            _wave.Insert(0f, t.DOScale(glowScale, shockwaveTime).SetEase(Ease.OutCubic));
            _wave.Insert(0f, shockwaveGlow.DOFade(0f, shockwaveTime * 1.2f).SetEase(Ease.InQuad));
        }

        // viền sóng: nhiều nhịp, nhịp sau nhỏ + mờ hơn
        if (shockwaveRing != null)
        {
            for (int i = 0; i < Mathf.Max(1, shockwavePulses); i++)
            {
                var ring = i == 0 ? shockwaveRing : Instantiate(shockwaveRing, shockwave);
                var t = ring.transform;
                float k = 1f - i * 0.18f;
                float at = i * shockwavePulseGap;

                t.localPosition = Vector3.zero;
                t.localScale = Vector3.zero;
                SetAlpha(ring, 0f);
                _wave.InsertCallback(at, () =>
                {
                    t.localScale = Vector3.one * ringScale * 0.1f;
                    SetAlpha(ring, ringAlpha * k);
                });
                _wave.Insert(at, t.DOScale(ringScale * k, shockwaveTime).SetEase(Ease.OutCubic));
                _wave.Insert(at, ring.DOFade(0f, shockwaveTime).SetEase(Ease.InCubic));
                if (i > 0) _wave.InsertCallback(at + shockwaveTime, () => Destroy(ring.gameObject));
            }
        }
        _wave.OnComplete(HideShockwave);

        OnShockwave?.Invoke(center, shockwaveRadius);
    }

    // ================= MOVE =================
    [Button(ButtonSizes.Large), GUIColor(0.6f, 0.85f, 1f), DisableInEditorMode]
    public void ToggleMove() => SetMove(!_moving);

    public void SetMove(bool moving)
    {
        _moving = moving;
        _move?.Kill();
        legFront.DOKill();
        legBack.DOKill();
        if (!moving)
        {
            // dừng: chân thu về thẳng
            Rot(legFront, 0f, 0.15f, Ease.OutQuad).SetLink(gameObject);
            Rot(legBack, 0f, 0.15f, Ease.OutQuad).SetLink(gameObject);
            return;
        }

        // 2 chân lệch pha: chân trước ra thì chân sau về
        _move = DOTween.Sequence().SetLink(gameObject).SetLoops(-1, LoopType.Restart);
        _move.Append(Rot(legFront, legSwing, stepTime * 0.5f, Ease.InOutSine));
        _move.Join(Rot(legBack, -legSwing, stepTime * 0.5f, Ease.InOutSine));
        _move.Append(Rot(legFront, -legSwing, stepTime, Ease.InOutSine));
        _move.Join(Rot(legBack, legSwing, stepTime, Ease.InOutSine));
        _move.Append(Rot(legFront, 0f, stepTime * 0.5f, Ease.InOutSine));
        _move.Join(Rot(legBack, 0f, stepTime * 0.5f, Ease.InOutSine));
    }

    // ================= HELPERS =================
    void AppendReturn(Sequence seq, float time)
    {
        seq.Append(Rot(body, 0f, time, Ease.OutQuad));
        seq.Join(body.DOLocalMove(_bodyBasePos, time).SetEase(Ease.OutQuad));
        seq.Join(Rot(neck, 0f, time, Ease.OutQuad));
        seq.Join(Rot(head, 0f, time, Ease.OutQuad));
        seq.Join(Rot(jaw, 0f, time, Ease.OutQuad));
        seq.Join(Rot(tail, 0f, time, Ease.OutQuad));
    }

    // Tư thế gốc của các bone là rotation 0 (pivot đã dựng sẵn)
    static Tween Rot(Transform t, float z, float time, Ease ease) =>
        t.DOLocalRotate(new Vector3(0f, 0f, z), time).SetEase(ease);

    [Button, DisableInEditorMode]
    public void ResetPose()
    {
        _attack?.Kill();
        foreach (var t in new[] { body, neck, head, jaw, tail })
        {
            if (t == null) continue;
            t.DOKill();
            t.localRotation = Quaternion.identity;
        }
        if (body != null) body.localPosition = _bodyBasePos;
    }

    void HideShockwave()
    {
        if (shockwave == null) return;
        // xoá các ring clone còn sót (khi gầm liên tục)
        for (int i = shockwave.childCount - 1; i >= 0; i--)
        {
            var c = shockwave.GetChild(i);
            if (c != shockwaveGlow?.transform && c != shockwaveRing?.transform) Destroy(c.gameObject);
        }
        shockwave.gameObject.SetActive(false);
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySfx(clip, 1f + UnityEngine.Random.Range(-pitchJitter, pitchJitter));
    }

    static void SetAlpha(SpriteRenderer sr, float a)
    {
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }

    void KillAll()
    {
        _attack?.Kill();
        _move?.Kill();
        _wave?.Kill();
    }

    void OnDrawGizmosSelected()
    {
        if (mouthPoint == null) return;
        Vector3 center = mouthPoint.position + Vector3.right * Mathf.Sign(transform.lossyScale.x) * shockwaveForward;
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(center, shockwaveRadius);
        Gizmos.DrawLine(mouthPoint.position, center);
    }
}
