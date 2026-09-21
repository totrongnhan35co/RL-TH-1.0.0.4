using BarcodeVerificationSystem.Controller;
using BarcodeVerificationSystem.Services.CaoSuDongNai;
using BarcodeVerificationSystem.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms; // Add this for MessageBox

public sealed class TokenRefresher
{
    private static readonly Lazy<TokenRefresher> _instance = new Lazy<TokenRefresher>(() => new TokenRefresher());
    public static TokenRefresher Instance => _instance.Value;

    private readonly CaoSuApiService _apiService;
    private CancellationTokenSource _cts;
    private Task _refreshTask;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly TimeSpan _refreshBefore = TimeSpan.FromHours(2); // Refresh 2 hours early

    private TokenRefresher()
    {
        _apiService = new CaoSuApiService();
    }

    /// <summary>
    /// Call this after successful login
    /// </summary>
    public void StartAutoRefresh(int expiresInSeconds)
    {
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresInSeconds);

        // Stop any previous task
        Stop();

        _cts = new CancellationTokenSource();
        _refreshTask = Task.Run(() => RefreshLoop(_cts.Token));
    }

    /// <summary>
    /// Stop auto refresh (e.g., on logout or app close)
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    private async Task RefreshLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var timeUntilRefresh = _tokenExpiry - now - _refreshBefore;

            if (timeUntilRefresh <= TimeSpan.Zero)
            {
                bool success = await TryRefreshTokenAsync();

                if (!success)
                {
                    Logger.LogError("Failed to refresh token!");
                }

                // Wait 5 minutes before next attempt (whether success or fail)
                await Task.Delay(TimeSpan.FromMinutes(5), token);
            }
            else
            {
                // Still good, check again in 1 minute
                var checkIn = TimeSpan.FromMinutes(1);
                if (timeUntilRefresh < checkIn) checkIn = timeUntilRefresh;

                await Task.Delay(checkIn, token);
            }
        }
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            Console.WriteLine("Refreshing token...");
            var response = await _apiService.PostRefreshTokenAsync();

            if (response?.success == true)
            {
                var data = response.data;

                Shared.Settings.AccessToken = data.accessToken;
                Shared.Settings.RefreshToken = data.refreshToken;
                // Update expiry time
                _tokenExpiry = DateTime.UtcNow.AddSeconds(data.expiresIn);

                Shared.SaveSettings();

                Console.WriteLine($"Token refreshed! New expiry: {_tokenExpiry:yyyy-MM-dd HH:mm:ss} UTC");
                return true;
            }
            else
            {
                Console.WriteLine("Refresh token API returned failure.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Refresh failed: {ex.Message}");
            return false;
        }
    }

    // Optional: Resume on app start (conservative)
    public void TryResumeFromSavedToken()
    {
        if (string.IsNullOrEmpty(Shared.Settings.AccessToken) ||
            string.IsNullOrEmpty(Shared.Settings.RefreshToken))
            return;

        // Assume token lasts at least 1 more hour if we have refresh token
        var assumedExpiry = DateTime.UtcNow.AddHours(1);
        var seconds = (int)(assumedExpiry - DateTime.UtcNow).TotalSeconds;
        if (seconds > 0)
        {
            //MessageBox.Show("Resuming token auto-refresh from saved login...", "Token Refresher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            StartAutoRefresh(seconds);
        }
    }
}