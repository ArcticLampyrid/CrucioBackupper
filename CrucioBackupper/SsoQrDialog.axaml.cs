using CrucioNetwork;
using CrucioNetwork.Model;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CrucioBackupper;

public partial class SsoQrDialog : Window
{
    private static readonly HttpClient httpClient = new();

    private readonly DispatcherTimer loopValidTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };

    private bool isValidating;

    public SsoQrInfo? QrInfo { get; private set; }
    public ValidSsoInfo? ValidSsoInfo { get; private set; }
    public string? Token { get; private set; }

    public SsoQrDialog()
    {
        InitializeComponent();

        loopValidTimer.Tick += LoopValidTimer_Tick;
        Closed += (_, _) => loopValidTimer.Stop();

        _ = InitializeQrAsync();
    }

    private async Task InitializeQrAsync()
    {
        try
        {
            var qrInfo = await CrucioApi.Default.RequireSsoQrInfo();
            if (qrInfo.HasError || qrInfo.Data == null)
            {
                await MessageBoxManager.GetMessageBoxStandard("扫码登录", $"获取二维码失败：(Code: {qrInfo.Code}) {qrInfo.Msg}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
                Close();
                return;
            }

            QrInfo = qrInfo.Data;
            var bytes = await httpClient.GetByteArrayAsync(QrInfo.ImageUrl);
            using var ms = new MemoryStream(bytes);
            QrImage.Source = new Bitmap(ms);
            loopValidTimer.Start();
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "加载二维码失败");
            await MessageBoxManager.GetMessageBoxStandard("扫码登录", $"加载二维码失败：{exception.Message}", ButtonEnum.Ok).ShowWindowDialogAsync(this);
            Close();
        }
    }

    private async void LoopValidTimer_Tick(object? sender, EventArgs e)
    {
        if (QrInfo == null || isValidating)
        {
            return;
        }

        try
        {
            isValidating = true;
            var (validInfo, token) = await CrucioApi.Default.ValidSsoQrInfo(QrInfo);
            if (!validInfo.HasError)
            {
                loopValidTimer.Stop();
                ValidSsoInfo = validInfo.Data;
                Token = token;
                Close();
            }
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "验证二维码状态失败");
        }
        finally
        {
            isValidating = false;
        }
    }
}
