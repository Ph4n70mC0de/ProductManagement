namespace ProductManagement.Features.Services.Implementations;

public class ConfirmationService
{
    public event Action<string, Action<bool>>? OnConfirm;

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        OnConfirm?.Invoke(message, result => tcs.SetResult(result));
        return await tcs.Task;
    }
}