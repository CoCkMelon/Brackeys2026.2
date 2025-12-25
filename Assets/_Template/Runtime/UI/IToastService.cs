namespace ZXTemplate.UI
{
    /// <summary>
    /// Small non-blocking notifications (toast messages).
    ///
    /// Typical usage:
    /// - Show short feedback after an action:
    ///   "Settings applied", "Rebind saved", "Not enough coins", etc.
    ///
    /// Notes:
    /// - ToastService implementation is expected to be queue-based and use unscaled time
    ///   so it works during pause menus.
    /// </summary>
    public interface IToastService
    {
        /// <summary>
        /// Enqueues a toast message. seconds is the hold duration (not counting fade).
        /// </summary>
        void Show(string message, float seconds = 2f);

        /// <summary>
        /// Clears the queue and hides the current toast immediately.
        /// </summary>
        void Clear();
    }
}
