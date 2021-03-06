using System.Threading.Tasks;
using System.Windows.Input;
using PandaTechEShop.Services;
using PandaTechEShop.Services.Preferences;
using PandaTechEShop.ViewModels.Base;
using Prism.Commands;
using Prism.Navigation;
using Rg.Plugins.Popup.Contracts;
using Xamarin.CommunityToolkit.ObjectModel;

namespace PandaTechEShop.ViewModels
{
    public class TestMainPageViewModel : BaseViewModel
    {
        private IPreferences _preferences;

        public TestMainPageViewModel(IBaseService baseService, IPreferences preferences)
            : base(baseService)
        {
            _preferences = preferences;
            Title = "Welcome to Xamarin.Forms with PRISM!";
            SaveCommand = new AsyncCommand(ExecuteSaveCommandAsync, allowsMultipleExecutions: false);
            RetriveCommand = new AsyncCommand(ExecuteRetrieveCommandAsync, allowsMultipleExecutions: false);
        }

        public string Username { get; set; }

        // TODO - Make AsyncCommand using Xamarin Community Toolkit
        // TOOD - "IsProcessing" checks
        public IAsyncCommand SaveCommand { get; }

        public IAsyncCommand RetriveCommand { get; }

        private async Task ExecuteSaveCommandAsync()
        {
            Username = Username?.Trim();

            if (!string.IsNullOrEmpty(Username))
            {
                _preferences.Set("Username", Username);
            }

            await Task.Delay(1);

            // await base.NavigationService.NavigateAsync("AboutPage", useModalNavigation: true);
        }

        private async Task ExecuteRetrieveCommandAsync()
        {
            Username = _preferences.Get("Username", string.Empty);

            await Task.Delay(1);
        }
    }
}
