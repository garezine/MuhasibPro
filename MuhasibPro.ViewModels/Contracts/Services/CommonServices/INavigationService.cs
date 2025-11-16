namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices
{
    public interface INavigationService
    {
        /// <summary>
        /// Gets a value indicating whether the frame can navigate back
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Navigates back in the frame's navigation history
        /// </summary>
        void GoBack();

        /// <summary>
        /// Initializes the navigation service with a frame
        /// </summary>
        /// <param name="frame">The frame to use for navigation</param>
        void Initialize(object frame);

        /// <summary>
        /// Navigates to a view associated with the specified ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel</typeparam>
        /// <param name="parameter">Optional parameter to pass to the view</param>
        /// <returns>True if navigation was successful</returns>
        bool Navigate<TViewModel>(object parameter = null);

        /// <summary>
        /// Navigates to a view associated with the specified ViewModel type
        /// </summary>
        /// <param name="viewModelType">The type of ViewModel</param>
        /// <param name="parameter">Optional parameter to pass to the view</param>
        /// <returns>True if navigation was successful</returns>
        bool Navigate(Type viewModelType, object parameter = null);

        /// <summary>
        /// Creates a new window and navigates to the specified ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel</typeparam>
        /// <param name="parameter">Optional parameter to pass to the view</param>
        /// <returns>The newly created Window</returns>
        Task<int> CreateNewViewAsync<TViewModel>(object parameter = null, string customTitle = null);

        /// <summary>
        /// Creates a new window and navigates to the specified ViewModel type
        /// </summary>
        /// <param name="viewModelType">The type of ViewModel</param>
        /// <param name="parameter">Optional parameter to pass to the view</param>
        /// <returns>The newly created Window</returns>
        Task<int> CreateNewViewAsync(Type viewModelType, object parameter = null, string customTitle = null);


        /// <summary>
        /// Closes the current view/window
        /// </summary>
        Task CloseViewAsync();
    }
}


