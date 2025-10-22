using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class AsyncBasicsManager : MonoBehaviour
{
    public Button synceDownloadButton;
    public Button asyncDownloadButton;

    public TextMeshProUGUI section1StatusText;
    public TextMeshProUGUI section2StatusText;
    public TextMeshProUGUI section3StatusText;
    public TextMeshProUGUI section4StatusText;
    public TextMeshProUGUI section5StatusText;
    public TextMeshProUGUI section6StatusText;
    public TextMeshProUGUI timeOutValue;

    public Slider progressBar;
    public Slider progressBar2;
    public Slider progressBar3;
    public Slider timeOutSlider;
    public Slider progressBar4;

    public Transform CubeObjTr;

    #region Section1
    //Section 1

    public void onSynceDownloadButtonClicked()
    {
        UpdateSection1Text("Starting synchronous download...");

        Thread.Sleep(3000);

        UpdateSection1Text("Synchronous download complete.");
    }

    public async void onAsyncDownloadButtonClicked()
    {
        UpdateSection1Text("Starting asynchronous download...");
        await Task.Delay(3000);
        UpdateSection1Text("Asynchronous download complete.");
        //Task.Run(async () =>
        //{
        //    await Task.Delay(3000);
        //    UpdateSection1Text("Asynchronous download complete.");
        //});
    }

    private void UpdateSection1Text(string newText)
    {
        section1StatusText.text = newText;
        Debug.Log($"[Section1] {newText}");
    }
    #endregion

    #region Section2
    //Section 2

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public async void OnDelayClicked(int sec)
    {
        UpdateSection2Text($"Starting asynchronous download... {sec}");
        for (int i = sec; i > 0; i--)
        {
            UpdateSection2Text($"Downloading... {i} seconds remaining");
            await Task.Delay(1000);
        }
        UpdateSection2Text("Asynchronous download complete.");
    }

    public async void OnCancellableDelayClicked()
    {
        try
        {
            UpdateSection2Text("Starting asynchronous download with cancellation...");
            for (int i = 10; i > 0; i--)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested(); // 외부에서 캔슬 요청 들어오면 예외처리
                UpdateSection2Text($"Downloading... {i} seconds remaining");
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
            UpdateSection2Text("Asynchronous download complete. [10s]");
        }
        catch (OperationCanceledException)
        {
            UpdateSection2Text("Download cancelled.");
        }
    }

    public void OnCancelDelayClicked()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            UpdateSection2Text("Download cancelled.");
        }
    }

    private void UpdateSection2Text(string newText)
    {
        section2StatusText.text = newText;
        Debug.Log($"[Section2] {newText}");
    }
    #endregion

    #region Section3
    //Section 3

    private void ResetProgressBars()
    {
        progressBar.value = 0;
        progressBar2.value = 0;
        progressBar3.value = 0;
    }

    public async void OnSequentialDownloadsClicked()
    {
        ResetProgressBars();
        UpdateSection3Text("Starting sequential downloads...");
        float startTime = Time.time;

        await FakeDownloadAsync(progressBar, 1, 2000, UpdateSection3Text);
        await FakeDownloadAsync(progressBar2, 2, 2000, UpdateSection3Text);
        await FakeDownloadAsync(progressBar3, 3, 2000, UpdateSection3Text);

        float endTime = Time.time - startTime;
        UpdateSection3Text($"All sequential downloads complete. : {endTime}s");
    }

    public async void OnParallelDownloadsClicked()
    {
        ResetProgressBars();
        UpdateSection3Text("Starting parallel downloads...");
        float startTime = Time.time;

        Task download1 = FakeDownloadAsync(progressBar, 1, 2000, UpdateSection3Text);
        Task download2 = FakeDownloadAsync(progressBar2, 2, 2000, UpdateSection3Text);
        Task download3 = FakeDownloadAsync(progressBar3, 3, 2000, UpdateSection3Text);

        await Task.WhenAll(download1, download2, download3);
        float endTime = Time.time - startTime;
        UpdateSection3Text($"All parallel downloads complete. : {endTime}");
    }

    private async Task FakeDownloadAsync(Slider p, int i, int durMs, Action<string> act)
    {
        int steps = 20;
        int delay = durMs / steps;
        var token = cts.Token;

        //for (int step = 1; step <= steps; step++)
        //{
        //    token.ThrowIfCancellationRequested();
        //    p.value = (float)step / steps;
        //    await Task.Delay(delay);
        //}

        try
        {
            for (int step = 1; step <= steps; step++)
            {
                token.ThrowIfCancellationRequested();
                p.value = (float)step / steps;
                await Task.Delay(delay);
            }
        }
        catch (OperationCanceledException)
        {
            act("Cancellable download cancelled.");
            //UpdateSection6Text("Cancellable download cancelled.");
        }

        if (p.value >= 1f)
          Debug.Log($"Download {i} complete.");
    }

    private void UpdateSection3Text(string newText)
    {
        section3StatusText.text = newText;
        Debug.Log($"[Section3] {newText}");
    }
    #endregion

    #region Section4
    //Section 4

    private void Start()
    {
        InitTimeOutSlide();
    }

    private void InitTimeOutSlide()
    {
        timeOutSlider.minValue = 1f;
        timeOutSlider.maxValue = 5f;
        timeOutSlider.value = 3f;
        OnTimeOutSliderChanged(timeOutSlider.value);

        timeOutSlider.onValueChanged.AddListener(OnTimeOutSliderChanged);
    }

    private void OnTimeOutSliderChanged(float value)
    {
        timeOutValue.text = value.ToString("F1") + "s";
    }

    private void UpdateSection4Text(string newText)
    {
        section4StatusText.text = newText;
        Debug.Log($"[Section4] {newText}");
    }

    public async void OnTimeOutDownloadClicked()
    {
        Task downloadTast = Task.Delay(4000);
        Task timeOutTask = Task.Delay((int)(timeOutSlider.value * 1000));

        UpdateSection4Text("Starting download with timeout...");
        var completedTask = await Task.WhenAny(downloadTast, timeOutTask);
        if (completedTask == downloadTast)
        {
            UpdateSection4Text("Download completed before timeout.");
        }
        else
        {
            UpdateSection4Text("Download timed out.");
        }
    }
    #endregion

    #region Section5
    //Section 5

    private void UpdateSection5Text(string newText)
    {
        section5StatusText.text = newText;
        Debug.Log($"[Section5] {newText}");
    }

    public async void OnSafeCodeClicked()
    {
        UpdateSection5Text("Starting safe code execution...");

        await Task.Delay(1000);

        CubeObjTr.position += Vector3.up * 0.5f;

        UpdateSection5Text("Safe code execution complete.");
    }

    public async void OnUnSafeCodeClicked()
    {
        UpdateSection5Text("Starting unsafe code execution...");
        await Task.Run(() =>
        {
            Thread.Sleep(1000);
            CubeObjTr.position += Vector3.up * 0.5f;
        });
        UpdateSection5Text("Unsafe code execution complete.");
    }
    #endregion

    #region Section6
    //Section 6
    private void UpdateSection6Text(string newText)
    {
        section6StatusText.text = newText;
        Debug.Log($"[Section6] {newText}");
    }

    private CancellationTokenSource cts = new CancellationTokenSource();
    public async void OnCancellableSingleTask()
    {
        OnCancelSingleTask();
        progressBar4.value = 0;
        UpdateSection6Text("Starting cancellable download...");
        await FakeDownloadAsync(progressBar4, 1, 5000, UpdateSection6Text);
        if (progressBar4.value >= 1f)
            UpdateSection6Text("Cancellable download complete.");
        //CancellationToken token = cts.Token;
        //UpdateSection6Text("Starting cancellable download...");
        //try
        //{
        //    for (int i = 5; i > 0; i--)
        //    {
        //        token.ThrowIfCancellationRequested();
        //        UpdateSection6Text($"Downloading... {i} seconds remaining");
        //        progressBar4.value = (5 - i + 1) / 5f;
        //        await Task.Delay(1000, token);
        //    }
        //    UpdateSection6Text("Cancellable download complete.");
        //}
        //catch (OperationCanceledException)
        //{ 
        //    UpdateSection6Text("Cancellable download cancelled.");
        //}
    }

    public void OnCancelSingleTask()
    {
        cts?.Cancel();
        cts?.Dispose();
        //progressBar4.value = 0;
        cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cts?.Cancel();
        cts?.Dispose();
        progressBar4.value = 0;
    }

    #endregion
}
