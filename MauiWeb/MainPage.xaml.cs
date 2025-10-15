using Microsoft.Extensions.Logging;

namespace MauiWeb;

public partial class MainPage : ContentPage
{
    readonly ILogger<MainPage> _logger;

    public MainPage(ILogger<MainPage> logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    private void HybridWebView_RawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
    {
        if (!_logger.IsEnabled(LogLevel.Information)) return;
        _logger.LogInformation("message: {message}", e.Message);
    }

    private readonly Uri _localUri = new("https://0.0.0.1");
    private readonly string _spaRoot = "index.html";
    private async void HybridWebView_WebResourceRequested(object sender, WebViewWebResourceRequestedEventArgs e)
    {

        #region リクエストが処理対象かの判定
        if (_logger.IsEnabled(LogLevel.Information)) 
            _logger.LogInformation("request: {uri}", e.Uri);

        var relativeUri = _localUri.MakeRelativeUri(e.Uri);
        if (relativeUri.IsAbsoluteUri) return;
        #endregion

        // 相対URL
        var uri = $"{relativeUri}";

        // 処理します
        e.Handled = true;

        var exists = await FileSystem.AppPackageFileExistsAsync(uri);

        if (!exists) {
            var spaRoot = _spaRoot;
            exists = await FileSystem.AppPackageFileExistsAsync(spaRoot);
            if (!exists)
            {
                e.SetResponse(404, "NOT FOUND");
                _logger.LogInformation("response: 404: {uri}", uri);
                return;
            }
            uri = spaRoot;
        }
        var contentType = ToMimeType(uri);
        Dictionary<string, string> headers = new()
        {
            { "Content-Type", contentType },
        };
        e.SetResponse(200, "OK", headers, await FileSystem.OpenAppPackageFileAsync(uri));
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("response: contentType:{contentType} {uri}", contentType, uri);
    }
    string ToMimeType(string? path)
        => Path.GetExtension(path) switch
        {
            ".svg" => "image/svg+xml",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            _ => "application/octet-stream",
        };
}
