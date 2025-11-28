// 📂 NavigationHelper.cs

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace gatchapon
{
    public static class NavigationHelper
    {
        // Flag to prevent concurrent navigation attempts
        public static bool IsNavigating { get; private set; } = false;

        public static async Task SafeGoToAsync(string route, bool animate = true)
        {
            if (IsNavigating) return; // Ignore if already navigating

            IsNavigating = true;

            try
            {
                // Execute the Shell navigation command
                await Shell.Current.GoToAsync(route, animate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeGoToAsync Error on route {route}: {ex.Message}");
            }
            finally
            {
                // Add a small delay to ensure the page has visually changed/transitioned
                // before re-enabling navigation, preventing double-tap issues.
                await Task.Delay(250);
                IsNavigating = false;
            }
        }
    }
}