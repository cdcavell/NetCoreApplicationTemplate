using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Template.Web.Constants;

namespace Template.Web.Tests.TestControllers;

/// <summary>
/// Provides test endpoints for demonstrating and verifying rate limiting policies in the application.
/// </summary>
/// <remarks>This controller exposes endpoints that are protected by different rate limiting policies, such as
/// 'Fixed' and 'Concurrency', to facilitate testing and validation of rate limiting behavior. Intended for use in
/// development and testing scenarios.</remarks>
[ApiController]
[Route("test/rate-limiting")]
public sealed class RateLimitingTestController : ControllerBase
{
    private static TaskCompletionSource _concurrencyRequestStarted = CreateSignal();

    /// <summary>
    /// Handles HTTP GET requests for the 'fixed' endpoint and returns a fixed result response.
    /// </summary>
    /// <remarks>This endpoint is rate-limited according to the 'Fixed' policy defined in <see
    /// cref="TemplateRateLimitingPolicyNames.Fixed"/>.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing a JSON object with a 'result' property set to "fixed".</returns>
    [HttpGet("fixed")]
    [EnableRateLimiting(TemplateRateLimitingPolicyNames.Fixed)]
    public IActionResult Fixed()
    {
        return Ok(new { result = "fixed" });
    }

    /// <summary>
    /// Handles a GET request to the 'concurrency' endpoint, responding after a fixed delay to simulate concurrent
    /// request handling.
    /// </summary>
    /// <remarks>This endpoint is subject to concurrency rate limiting as defined by the applied policy. The
    /// response is delayed by 750 milliseconds to simulate processing time.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation before the delay completes.</param>
    /// <returns>An <see cref="IActionResult"/> containing the result of the concurrency operation.</returns>
    [HttpGet("concurrency")]
    [EnableRateLimiting(TemplateRateLimitingPolicyNames.Concurrency)]
    public async Task<IActionResult> Concurrency(CancellationToken cancellationToken)
    {
        _concurrencyRequestStarted.TrySetResult();

        await Task.Delay(TimeSpan.FromMilliseconds(750), cancellationToken);

        return Ok(new { result = "concurrency" });
    }

    /// <summary>
    /// Resets the concurrency signal to its initial state, allowing new concurrency operations to be tracked.
    /// </summary>
    /// <remarks>Call this method to reinitialize the concurrency signal before starting a new set of
    /// concurrent operations. This is typically used in scenarios where concurrency tracking needs to be restarted or
    /// cleared.</remarks>
    public static void ResetConcurrencySignal()
    {
        _concurrencyRequestStarted = CreateSignal();
    }

    /// <summary>
    /// Waits asynchronously until a concurrency request has started or the operation times out after five seconds.
    /// </summary>
    /// <remarks>If the concurrency request does not start within five seconds, the returned task will
    /// complete in a faulted state with a timeout exception.</remarks>
    /// <returns>A task that completes when a concurrency request has started or the five-second timeout elapses.</returns>
    public static Task WaitForConcurrencyRequestStartedAsync()
    {
        return _concurrencyRequestStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Creates a new TaskCompletionSource that runs continuations asynchronously.
    /// </summary>
    /// <remarks>Running continuations asynchronously helps prevent potential deadlocks and improves thread
    /// safety when signaling task completion from multiple threads.</remarks>
    /// <returns>A TaskCompletionSource instance configured to run continuations asynchronously.</returns>
    private static TaskCompletionSource CreateSignal()
    {
        return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
