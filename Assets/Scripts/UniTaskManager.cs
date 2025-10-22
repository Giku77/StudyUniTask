using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class UniTaskManager : MonoBehaviour
{
    public Button delayButton;
    public Button delayFrameButton;
    public Button yieldButton;
    public Button nextFrameButton;

    public Button sequentialButton;
    public Button whenAllButton;
    public Button whenAnyButton;

    public Button loadResourceButton;
    public Button loadingWithProgressBarButton;
    public Button cancelButton;
    public Slider progressloadingBar;

    public Button updateButton;
    public Button fixedUpdateButton;
    public Button lateUpdateButton;

    public Button destroyTokenButton;
    public Button timeOutTokenButton;
    public Button linkedTokenButton;
    public Button cancelSection5Button;
    public Slider section5ProgressBar;

    private CancellationTokenSource section5Cts;

    public Slider progressSlider;
    public Slider progressSlider2;
    public Slider progressSlider3;

    public TextMeshProUGUI[] section1Text;

    public Button fadeInButton;
    public Button fadeOutButton;
    public Button AnimationButton;
    public Button waitForInputButton;
    public CanvasGroup fadePanel;
    public Transform animatedObject;

    private void Start()
    {
        delayButton.onClick.AddListener(() => OnDelayClicked().Forget());
        delayFrameButton.onClick.AddListener(() => OnDelayFrameClicked().Forget());
        yieldButton.onClick.AddListener(() => OnYieldClicked().Forget());
        nextFrameButton.onClick.AddListener(() => OnNextFrameClicked().Forget());

        sequentialButton.onClick.AddListener(() => OnSequentialClicked().Forget());
        whenAllButton.onClick.AddListener(() => OnWhenAllClicked().Forget());
        whenAnyButton.onClick.AddListener(() => OnWhenAnyClicked().Forget());

        loadResourceButton.onClick.AddListener(() => OnLoadResourceClicked().Forget());
        loadingWithProgressBarButton.onClick.AddListener(() => OnLoadingWithProgressBarClicked().Forget());
        cancelButton.onClick.AddListener(() =>
        {
            loadCts?.Cancel();
            loadCts?.Dispose();
            loadCts = new CancellationTokenSource();
        });

        updateButton.onClick.AddListener(() => OnUpdateClicked().Forget());
        fixedUpdateButton.onClick.AddListener(() => OnFixedUpdateClicked().Forget());
        lateUpdateButton.onClick.AddListener(() => OnLateUpdateClicked().Forget());

        destroyTokenButton.onClick.AddListener(() => OnDestroyTokenClicked().Forget());
        timeOutTokenButton.onClick.AddListener(() => OnTimeOutTokenClicked().Forget());
        linkedTokenButton.onClick.AddListener(() => OnLinkedTokenClicked().Forget());
        cancelSection5Button.onClick.AddListener(OnCancelSection5Clicked);

        fadeInButton.onClick.AddListener(() => OnFadeInClicked().Forget());
        fadeOutButton.onClick.AddListener(() => OnFadeOutClicked().Forget());
        AnimationButton.onClick.AddListener(() => OnAnimationClicked().Forget());
        waitForInputButton.onClick.AddListener(() => OnWaitForInputClicked().Forget());


        ResetProgressBars();
    }

    private void ResetProgressBars()
    {
        progressSlider.value = 0f;
        progressSlider2.value = 0f;
        progressSlider3.value = 0f;
    }

    private async UniTask FakeLoadAsync(Slider p, int ms)
    {
        int steps = 20;
        int delayPerStep = ms / steps;

        for (int i = 0; i < steps; i++)
        {
            await UniTask.Delay(delayPerStep);
            p.value = (i + 1) / (float)steps;
        }
    }

    private async UniTaskVoid OnSequentialClicked()
    {
        UpdateSectionText(1, "Sequential started");
        ResetProgressBars();
        float startTime = Time.time;
        await FakeLoadAsync(progressSlider, 2000);
        await FakeLoadAsync(progressSlider2, 2000);
        await FakeLoadAsync(progressSlider3, 2000);
        float endTime = Time.time;
        UpdateSectionText(1, $"Sequential ended in {(endTime - startTime):F2} seconds");
    }

    private async UniTaskVoid OnWhenAllClicked()
    {
        UpdateSectionText(1, "WhenAll started");
        ResetProgressBars();
        float startTime = Time.time;
        var task1 = FakeLoadAsync(progressSlider, 2000);
        var task2 = FakeLoadAsync(progressSlider2, 3000);
        var task3 = FakeLoadAsync(progressSlider3, 4000);
        await UniTask.WhenAll(task1, task2, task3);
        float endTime = Time.time;
        UpdateSectionText(1, $"WhenAll ended in {(endTime - startTime):F2} seconds");
    }

    private async UniTaskVoid OnWhenAnyClicked()
    {
        UpdateSectionText(1, "WhenAny started");
        ResetProgressBars();
        float startTime = Time.time;
        var task1 = FakeLoadAsync(progressSlider, 2000);
        var task2 = FakeLoadAsync(progressSlider2, 3000);
        var task3 = FakeLoadAsync(progressSlider3, 4000);
        await UniTask.WhenAny(task1, task2, task3);
        float endTime = Time.time;
        UpdateSectionText(1, $"WhenAny ended in {(endTime - startTime):F2} seconds");
    }

    private void UpdateSectionText(int section, string str)
    {
        var log = $"[Section {section + 1}]  {str}";
        section1Text[section].text = log;
        Debug.Log(log);
    }

    private async UniTaskVoid OnDelayClicked()
    {
        UpdateSectionText(0, "Delay started");
        await UniTask.Delay(2000);
        UpdateSectionText(0, "Delay ended after 2 seconds");
    }

    private async UniTaskVoid OnDelayFrameClicked()
    {
        UpdateSectionText(0, "DelayFrame started");
        int startFrame = Time.frameCount;
        await UniTask.DelayFrame(60);
        int endFrame = Time.frameCount;
        UpdateSectionText(0, $"DelayFrame ended after {endFrame - startFrame} frames");
    }

    private async UniTaskVoid OnYieldClicked()
    {
        UpdateSectionText(0, "Yield started");
        int startFrame = Time.frameCount;
        await UniTask.Yield();
        int endFrame = Time.frameCount;
        UpdateSectionText(0, $"Yield ended after {endFrame - startFrame} frames");
    }

    private async UniTaskVoid OnNextFrameClicked()
    {
        UpdateSectionText(0, "NextFrame started");
        int startFrame = Time.frameCount;
        await UniTask.NextFrame();
        int endFrame = Time.frameCount;
        UpdateSectionText(0, $"NextFrame ended after {endFrame - startFrame} frames");
    }

    private async UniTaskVoid OnLoadResourceClicked()
    {
        UpdateSectionText(2, "LoadResource started");
        var pr = await Resources.LoadAsync<GameObject>("RotatingCube").ToUniTask();
        UpdateSectionText(2, "LoadResource ended");
        Instantiate(pr);
    }

    private void OnDestroy()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
    }

    private CancellationTokenSource loadCts;

    private async UniTaskVoid OnLoadingWithProgressBarClicked()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();

        try
        {
            UpdateSectionText(2, "LoadingWithProgressBar started");
            progressloadingBar.value = 0f;

            for (int i = 0; i <= 100; i++)
            {
                loadCts.Token.ThrowIfCancellationRequested();
                progressloadingBar.value = i / 100f;
                UpdateSectionText(2, $"LoadingWithProgressBar progress: {i}%");
                await UniTask.Delay(50, cancellationToken: loadCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            UpdateSectionText(2, "LoadingWithProgressBar canceled");
            progressloadingBar.value = 0f;
        }
        UpdateSectionText(2, "LoadingWithProgressBar ended");
    }

    private async UniTaskVoid OnUpdateClicked()
    {
        UpdateSectionText(3, "Update started");
        for (int count = 0; count < 100; count++)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            UpdateSectionText(3, $"Update count: {Time.frameCount}");
        }
        UpdateSectionText(3, "Update ended");
    }

    private async UniTaskVoid OnFixedUpdateClicked()
    {
        UpdateSectionText(3, "FixedUpdate started");
        for (int count = 0; count < 100; count++)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            UpdateSectionText(3, $"FixedUpdate count: {Time.time}");
        }
        UpdateSectionText(3, "FixedUpdate ended");
    }

    private async UniTaskVoid OnLateUpdateClicked()
    {
        UpdateSectionText(3, "LateUpdate started");
        for (int count = 0; count < 100; count++)
        {
            await UniTask.Yield(PlayerLoopTiming.LastTimeUpdate);
            UpdateSectionText(3, $"LateUpdate count: {Time.frameCount}");
        }
        UpdateSectionText(3, "LateUpdate ended");
    }

    public async UniTask LongRunningTaskAsync(CancellationToken ct)
    {
        UpdateSectionText(4, "LongRunningTask started");
        section5ProgressBar.value = 0f;

        for (int i = 0; i <= 100; i++)
        {
            ct.ThrowIfCancellationRequested();
            section5ProgressBar.value = i / 100f;
            UpdateSectionText(4, $"LongRunningTask progress: {i}%");
            await UniTask.Delay(100, cancellationToken: ct);
        }
        UpdateSectionText(4, "LongRunningTask ended");
    }

    private async UniTaskVoid OnDestroyTokenClicked()
    {
        UpdateSectionText(4, "DestroyToken LongRunningTask started");

        try
        {
            await LongRunningTaskAsync(this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            UpdateSectionText(4, "LongRunningTask canceled by DestroyToken");
        }
        UpdateSectionText(4, "DestroyToken LongRunningTask ended");
    }

    private async UniTaskVoid OnTimeOutTokenClicked()
    {
        UpdateSectionText(4, "TimeOutToken LongRunningTask started");

        try
        {
            using (var cts = new CancellationTokenSource(3000))
            {
                await LongRunningTaskAsync(cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            UpdateSectionText(4, "LongRunningTask canceled by TimeOutToken");
        }
        UpdateSectionText(4, "TimeOutToken LongRunningTask ended");
    }

    private async UniTaskVoid OnLinkedTokenClicked()
    {
        section5Cts?.Cancel();
        section5Cts?.Dispose();
        section5Cts = new CancellationTokenSource();
        UpdateSectionText(4, "LinkedToken LongRunningTask started");
        try
        {
            using (var cts1 = new CancellationTokenSource(5000))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, section5Cts.Token, this.GetCancellationTokenOnDestroy()))
            {
                await LongRunningTaskAsync(linkedCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            //UpdateSectionText(4, "LongRunningTask canceled by LinkedToken");

            if (section5Cts.IsCancellationRequested)
            {
                UpdateSectionText(4, "Cancellation requested by user");
            }
            else
            {
                UpdateSectionText(4, "Cancellation requested by timeout or destroy");
            }
        }
        UpdateSectionText(4, "LinkedToken LongRunningTask ended");
    }

    private void OnCancelSection5Clicked()
    {
        section5Cts?.Cancel();
    }

    private async UniTask FadeAsync(CanvasGroup canvasGroup, float fr, float to, float s)
    {
        float elapsed = 0f;
        canvasGroup.alpha = fr;
        while (elapsed < s)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(fr, to, Mathf.Clamp01(elapsed / s));
            await UniTask.Yield();
        }
        canvasGroup.alpha = to;
    }

    private async UniTask MoveToAsync(RectTransform rectTransform, Vector2 to, float s)
    {
        float elapsed = 0f;
        Vector2 from = rectTransform.anchoredPosition;
        while (elapsed < s)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(from, to, Mathf.Clamp01(elapsed / s));
            await UniTask.Yield();
        }
        rectTransform.anchoredPosition = to;
    }

    private async UniTask RotateToAsync(Transform transform, Quaternion to, float s)
    {
        float elapsed = 0f;
        Quaternion from = transform.rotation;
        while (elapsed < s)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(from, to, Mathf.Clamp01(elapsed / s));
            await UniTask.Yield();
        }
        transform.rotation = to;
    }
    private async UniTask RotateToAsync2(Transform transform, float degr, float s)
    {
        float from = transform.eulerAngles.z;
        float elapsed = 0f;
        while (elapsed < s)
        {
            elapsed += Time.deltaTime;
            float z = Mathf.Lerp(from, degr, Mathf.Clamp01(elapsed / s));
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
            await UniTask.Yield();
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, degr);
    }
    private async UniTaskVoid OnFadeInClicked()
    {
        UpdateSectionText(5, "FadeIn started");
        await FadeAsync(fadePanel, 0f, 1f, 2f);
        UpdateSectionText(5, "FadeIn ended");
    }

    private async UniTaskVoid OnFadeOutClicked()
    {
        UpdateSectionText(5, "FadeOut started");
        await FadeAsync(fadePanel, 1f, 0f, 2f);
        UpdateSectionText(5, "FadeOut ended");
    }

    private Vector2 originalPosition;
    private float originalRotationZ;
    private async UniTaskVoid OnAnimationClicked()
    {
        UpdateSectionText(5, "Animation started");
        RectTransform rectTransform = animatedObject.GetComponent<RectTransform>();
        //var originalRotation = animatedObject.rotation;
        if (originalPosition == Vector2.zero)
        {
            originalPosition = rectTransform.anchoredPosition;
            originalRotationZ = animatedObject.eulerAngles.z;
        }
        await MoveToAsync(rectTransform, originalPosition + Vector2.up * 50f, 0.5f);
        UpdateSectionText(5, "Moving up completed");
        await RotateToAsync2(animatedObject, 360f, 0.5f);
        UpdateSectionText(5, "Rotation completed");
        await MoveToAsync(rectTransform, originalPosition, 0.5f);
        UpdateSectionText(5, "Moving down completed");
        await RotateToAsync2(animatedObject, originalRotationZ, 0.5f);
        UpdateSectionText(5, "Rotation reset completed");
        UpdateSectionText(5, "Animation ended");
    }

    private async UniTaskVoid OnWaitForInputClicked()
    {
        UpdateSectionText(5, "WaitForInput started");
        UpdateSectionText(5, "Press Space key to continue...");
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        UpdateSectionText(5, "WaitForInput ended");
    }
}