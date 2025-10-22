using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAlpha : MonoBehaviour
{
    public static ChangeAlpha I { get; private set; }
    public Image targetImage;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetAlpha(float a)
    {
        if (!targetImage) return;
        var c = targetImage.color;
        c.a = Mathf.Clamp01(a);
        targetImage.color = c;
    }

    public async UniTask FadeAsync(float fr, float to, float s)
    {
        float elapsed = 0f;
        var c = targetImage.color;
        c.a = fr;
        targetImage.color = c;
        while (elapsed < s)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(fr, to, Mathf.Clamp01(elapsed / s));
            targetImage.color = c;
            await UniTask.Yield();
        }
        c.a = to;
        targetImage.color = c;
    }
}
