// -----------------------------------------------------------------------
// <copyright file="AsyncExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MinIO.ApiService;

/// <summary>
/// Async extensions.
/// </summary>
public static class AsyncExtensions
{
    /// <summary>
    /// Allows a cancellation token to be awaited.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The awaiter.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static CancellationTokenAwaiter GetAwaiter(this CancellationToken ct) => new() { CancellationToken = ct };

    /// <summary>
    /// The awaiter for cancellation tokens.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public struct CancellationTokenAwaiter(CancellationToken cancellationToken) : System.Runtime.CompilerServices.ICriticalNotifyCompletion
    {
        /// <summary>
        /// The cancellation token.
        /// </summary>
        internal CancellationToken CancellationToken = cancellationToken;

        /// <inheritdoc cref="System.Runtime.CompilerServices.TaskAwaiter.IsCompleted" />
#pragma warning disable IDE0251, MA0102
        public bool IsCompleted => this.CancellationToken.IsCancellationRequested;
#pragma warning restore IDE0251, MA0102

        /// <inheritdoc cref="System.Runtime.CompilerServices.TaskAwaiter.GetResult" />
        public object GetResult()
        {
            // this is called by compiler generated methods when the task has completed.
            // Instead of returning a result, we just throw an exception.
            if (this.IsCompleted)
            {
                throw new OperationCanceledException();
            }

            throw new InvalidOperationException("The cancellation token has not yet been cancelled.");
        }

        /// <inheritdoc cref="System.Runtime.CompilerServices.TaskAwaiter.OnCompleted(Action)" />
        public readonly void OnCompleted(Action continuation) => this.CancellationToken.Register(continuation);

        /// <inheritdoc cref="System.Runtime.CompilerServices.TaskAwaiter.UnsafeOnCompleted(Action)" />
        public readonly void UnsafeOnCompleted(Action continuation) => this.CancellationToken.Register(continuation);
    }
}