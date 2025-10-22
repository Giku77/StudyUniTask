using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class UniTasks : MonoBehaviour
{
    public TextMeshProUGUI[] statusText;

    //1
    public Button startButton;
    public Button stopButton;
    public Button resumeButton;
    public Button resetButton;
    public Slider slider1;
    public TextMeshProUGUI timer1;

    //2
    public Button loadingButton;
    public Slider slider2;

    //3
    public Button loadStartButton;
    public Button loadCancelButton;
    public Slider rslider1;
    public Slider rslider2;
    public Slider rslider3;
    public Slider rslider;

    //4
    public Button animateStartButton;
    public Button animateResetButton;
    public Button animateCancelButton;
    public Transform animateObj;

    //5
    public Button saveDataButton;
    public TMP_InputField dataInputField;

    private CancellationTokenSource cts = new CancellationTokenSource();

    private void Awake()
    {
        startButton.onClick.AddListener(() => StartTask().Forget());
        stopButton.onClick.AddListener(() => StopTask());
        resumeButton.onClick.AddListener(() => ResumeTask().Forget());
        resetButton.onClick.AddListener(() => ResetTimer());
        loadingButton.onClick.AddListener(() => LoadSceneWithUI().Forget());
        loadStartButton.onClick.AddListener(() => OnLoadResourcesClicked().Forget());
        loadCancelButton.onClick.AddListener(() => OnLoadCancelClicked());
        animateStartButton.onClick.AddListener(() => OnAnimateStartClicked().Forget());
        animateCancelButton.onClick.AddListener(() => OnAnimateCancelClicked());
        animateResetButton.onClick.AddListener(() => OnAnimateResetClicked());
        saveDataButton.onClick.AddListener(() => OnSaveDataClicked().Forget());
      
        dataInputField.onValueChanged.AddListener((str) =>
        {
            debounceCts?.Cancel();
            debounceCts?.Dispose();
            debounceCts = new CancellationTokenSource();
            DebouncedLogAsync(str, debounceCts.Token).Forget();
        });
        AutosaveLoopSimple(saveCts.Token).Forget();
    }
    private void Start()
    {
        InitUi();
        var loadStr = PlayerPrefs.GetString("SavedData", "");
        dataInputField.text = loadStr;
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    private void UpdateStatusText(int i, string str)
    {
        statusText[i].text = str;
        Debug.Log($"Q{i + 1} : {str}");
    }

    private void ResetSlider()
    {
        slider1.value = 0;
    }

    private void ResetTimer()
    {
        ResetSlider();
        StopTask();
    }

    private void InitUi()
    {
        ResetSlider();
        slider1.onValueChanged.AddListener((value) =>
        {
            var min = value / 60;
            var sec = value % 60;
            timer1.text = $"{(int)min}m {(int)sec}s";
        });
        slider1.minValue = 0;
        slider1.maxValue = 100;
    }

    private async UniTask StartTask()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        try
        {
            UpdateStatusText(0, "[Q1] Task Started");
            await UpdateCountDownTimer(cts.Token, (int)slider1.value);
            // await UpdateSliderAsync(cts.Token, (int)slider1.value);
            UpdateStatusText(0, "Time's Up!");
        }
        catch (OperationCanceledException)
        {
            UpdateStatusText(0, "[Q1] Task Canceled Exception Caught");
        }
    }

    private async UniTask UpdateCountDownTimer(CancellationToken token, int startTimer)
    {
        for (int i = startTimer - 1; i >= 0; i--)
        {
            token.ThrowIfCancellationRequested();
            await UniTask.Delay(1000, cancellationToken: token);
            slider1.value = i;
            var min = i / 60;
            var sec = i % 60;
            timer1.text = $"{min}m {sec}s";
        }
    }
    private void StopTask()
    {
        cts?.Cancel();
        UpdateStatusText(0, "[Q1] Task Stopped");
    }
    private UniTask ResumeTask()
    {
        return StartTask();
    }

    private async UniTask LoadSceneWithUI()
    {
        await ChangeAlpha.I.FadeAsync(0f, 0.5f, 0.5f);

        var op = SceneManager.LoadSceneAsync("Loading");
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float p = op.progress / 0.9f;
            slider2.value = p;
            UpdateStatusText(1, $"Loading... {(int)(p * 100)}%");
            await UniTask.Yield();
        }

        slider2.value = 1f;
        UpdateStatusText(1, "[Q2] activate the scene.");

        await ChangeAlpha.I.FadeAsync(0.5f, 1f, 0.5f);
        op.allowSceneActivation = true;
        await UniTask.WaitUntil(() => op.isDone);
        await ChangeAlpha.I.FadeAsync(1f, 0f, 1f);
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

    private CancellationTokenSource loadCts;
    private CancellationTokenSource timeOutCts;

    private async UniTaskVoid OnLoadResourcesClicked()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();

        timeOutCts?.Cancel();
        timeOutCts?.Dispose();
        timeOutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(loadCts.Token, timeOutCts.Token);

        UpdateStatusText(2, "LoadResource started");

        var req1 = Resources.LoadAsync<GameObject>("RES1");
        var req2 = Resources.LoadAsync<GameObject>("RES2");
        var req3 = Resources.LoadAsync<GameObject>("RES3");

        try
        {

            while (!(req1.isDone && req2.isDone && req3.isDone))
            {
                loadCts.Token.ThrowIfCancellationRequested();


                float p1 = req1.progress;
                float p2 = req2.progress;
                float p3 = req3.progress;

                float avg = (p1 + p2 + p3) / 3f;

                rslider1.value = p1;
                rslider2.value = p2;
                rslider3.value = p3;

                rslider.value = avg;
                UpdateStatusText(2, $"Loading... {(int)(avg * 100)}%");

                await UniTask.Yield(PlayerLoopTiming.Update, linkedCts.Token);
            }

            await UniTask.WhenAll(
                req1.ToUniTask(cancellationToken: linkedCts.Token),
                req2.ToUniTask(cancellationToken: linkedCts.Token),
                req3.ToUniTask(cancellationToken: linkedCts.Token)
            );


            Instantiate((GameObject)req1.asset);
            Instantiate((GameObject)req2.asset);
            Instantiate((GameObject)req3.asset);

            rslider1.value = 1f;
            rslider2.value = 1f;
            rslider3.value = 1f;
            rslider.value = 1f;
            UpdateStatusText(2, "All resources loaded!");
        }
        catch (OperationCanceledException)
        {
            UpdateStatusText(2, "Loading cancelled");

            rslider.value = 0f;
        }
    }

    private void OnLoadCancelClicked()
    {
        loadCts?.Cancel();
        UpdateStatusText(2, "LoadResource cancel requested");
    }

    private async UniTask FadeAsync(float fr, float to, float s, CancellationToken token)
    {
        float elapsed = 0f;
        var c = animateObj.gameObject.GetComponent<Image>().color;
        c.a = fr;
        animateObj.gameObject.GetComponent<Image>().color = c;
        while (elapsed < s)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(fr, to, Mathf.Clamp01(elapsed / s));
            animateObj.gameObject.GetComponent<Image>().color = c;
            await UniTask.Yield();
        }
        c.a = to;
        animateObj.gameObject.GetComponent<Image>().color = c;
    }

    private async UniTask ScaleToAsync(Transform transform, Vector3 to, float s, CancellationToken token)
    {
        float elapsed = 0f;
        Vector3 from = transform.localScale;
        while (elapsed < s)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / s));
            await UniTask.Yield();
        }
        transform.localScale = to;
    }

    private async UniTask MoveToAsync(RectTransform rectTransform, Vector2 to, float s, CancellationToken token)
    {
        float elapsed = 0f;
        Vector2 from = rectTransform.anchoredPosition;
        while (elapsed < s)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(from, to, Mathf.Clamp01(elapsed / s));
            await UniTask.Yield();
        }
        rectTransform.anchoredPosition = to;
    }

    private async UniTask RotateToAsync(Transform transform, float degr, float s, CancellationToken token)
    {
        float from = transform.eulerAngles.z;
        float elapsed = 0f;
        while (elapsed < s)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float z = Mathf.Lerp(from, degr, Mathf.Clamp01(elapsed / s));
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
            await UniTask.Yield();
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, degr);
    }

    private CancellationTokenSource animateCts;
    private Vector2 originPos;
    private Vector3 originScale;

    private async UniTask RunAnimationSequence(CancellationToken token)
    {
        await FadeAsync(1f, 0f, 1f, token);
        UpdateStatusText(3, "Faded Out");
        await FadeAsync(0f, 1f, 1f, token);
        UpdateStatusText(3, "Faded In");
        await MoveToAsync(animateObj.GetComponent<RectTransform>(), new Vector2(originPos.x + 200, originPos.y), 0.5f, token);
        UpdateStatusText(3, "Moved Right");
        await ScaleToAsync(animateObj, new Vector3(1.5f, 1.5f, 1.5f), 0.5f, token);
        UpdateStatusText(3, "Scaled Up");
        await RotateToAsync(animateObj, 360f, 1f, token);
        UpdateStatusText(3, "Rotated");
    }
    private async UniTaskVoid OnAnimateStartClicked()
    {
        if (originPos == Vector2.zero)
        {
            originPos = animateObj.GetComponent<RectTransform>().anchoredPosition;
            originScale = animateObj.localScale;
        }
        animateCts?.Cancel();
        animateCts?.Dispose();
        animateCts = new CancellationTokenSource();
        try
        {
            UpdateStatusText(3, "Animation Started");
            await RunAnimationSequence(animateCts.Token);
            UpdateStatusText(3, "Animation Completed");
        }
        catch (OperationCanceledException)
        {
            UpdateStatusText(3, "Animation Canceled");
        }
    }
    private void OnAnimateCancelClicked()
    {
        animateCts?.Cancel();
        UpdateStatusText(3, "Animation Cancel Requested");
    }
    private void OnAnimateResetClicked()
    {
        animateCts?.Cancel();
        animateObj.GetComponent<RectTransform>().anchoredPosition = originPos;
        animateObj.localScale = originScale;
        animateObj.eulerAngles = Vector3.zero;
        UpdateStatusText(3, "Animation Reset");
    }


    private CancellationTokenSource saveCts = new CancellationTokenSource();
    private CancellationTokenSource debounceCts;
    private async UniTaskVoid OnSaveDataClicked()
    {
        saveCts?.Cancel();
        saveCts?.Dispose();
        saveCts = new CancellationTokenSource();
        debounceCts?.Cancel();
        string data = dataInputField.text;
        try
        {
            await SaveDataAsync(data, saveCts.Token);
            UpdateStatusText(4, "Data saved successfully.");
        }
        catch (OperationCanceledException)
        {
            UpdateStatusText(4, "Data save canceled.");
        }
    }
    private bool isSaving = false;
    private async UniTask SaveDataAsync(string data, CancellationToken token)
    {
        //await UniTask.Delay(1000, cancellationToken: token);
        isSaving = true;
        PlayerPrefs.SetString("SavedData", data);
        PlayerPrefs.Save();
        await UniTask.Yield(token);
        isSaving = false;
    }
    private async UniTaskVoid AutosaveLoopSimple(CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            token, this.GetCancellationTokenOnDestroy());

        try
        {
            while (!linked.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: linked.Token);

                if (isSaving) continue;

                isSaving = true;
                try
                {
                    await SaveDataAsync(dataInputField.text, linked.Token);
                    var time = DateTime.Now.ToString("HH:mm:ss");
                    UpdateStatusText(4, $"Auto saved at [{time}]");
                }
                finally { isSaving = false; }
            }
        }
        catch (OperationCanceledException) 
        {
            UpdateStatusText(4, "Autosave loop canceled.");
        }
    }

    private async UniTaskVoid DebouncedLogAsync(string snapshot, CancellationToken token)
    {
        await UniTask.Delay(3000, cancellationToken: token);
        UpdateStatusText(4, "Debounced save triggered.");
        SaveDataAsync(snapshot, token).Forget();
    }
}