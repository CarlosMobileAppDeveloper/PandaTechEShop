using System;
using System.Threading.Tasks;
using Moq;
using PandaTechEShop.Services;
using PandaTechEShop.Services.Account;
using PandaTechEShop.ViewModels.Account;
using Xunit;
using XF.Material.Forms.UI.Dialogs;
using Prism.Navigation;
using PandaTechEShop.Validations;
using System.Collections.Generic;
using System.Collections;

namespace PandaTechEShop.UnitTests.ViewModels.Account
{
    public class SignupPageViewModelTests
    {
        // https://enterprisecraftsmanship.com/posts/you-naming-tests-wrong/

        SignupPageViewModel _sut;
        Mock<IAccountService> _mockAccountService;
        Mock<IBaseService> _mockBaseService;
        Mock<IMaterialDialog> _mockMaterialDialog;

        public SignupPageViewModelTests()
        {
            var mockLoadingDialog = new Mock<IMaterialModalPage>();
            var mockLoadingSnackbar = new Mock<IMaterialModalPage>();
            _mockMaterialDialog = new Mock<IMaterialDialog>();
            _mockMaterialDialog.Setup(x => x.LoadingDialogAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(mockLoadingDialog.Object));
            _mockMaterialDialog.Setup(x => x.SnackbarAsync(It.IsAny<string>(), MaterialSnackbar.DurationLong, null)).Returns(Task.FromResult(mockLoadingSnackbar.Object));

            _mockBaseService = new Mock<IBaseService>();
            var mockNavigationService = new Mock<INavigationService>();
            _mockBaseService.Setup(x => x.NavigationService).Returns(mockNavigationService.Object);

            _mockAccountService = new Mock<IAccountService>();
            
            _sut = new SignupPageViewModel(_mockBaseService.Object, _mockAccountService.Object, _mockMaterialDialog.Object);
        }

        [Fact]
        public void On_ViewModel_Creation_Commands_Are_Setup()
        {
            Assert.NotNull(_sut.SignUpCommand);
            Assert.NotNull(_sut.NavigateToSignInPageCommand);
            Assert.NotNull(_sut.ValidateEmailCommand);
            Assert.NotNull(_sut.ForceValidateEmailCommand);
            Assert.NotNull(_sut.ValidatePasswordCommand);
            Assert.NotNull(_sut.ForceValidatePasswordCommand);
            Assert.NotNull(_sut.ValidatePasswordMatchCommand);
            Assert.NotNull(_sut.ForceValidatePasswordMatchCommand);
        }

        [Fact]
        public void On_ViewModel_Creation_Validation_Rules_Are_Setup()
        {
            Assert.NotNull(_sut.EmailAddress.Validations);
            Assert.NotEmpty(_sut.EmailAddress.Validations);
            Assert.NotNull(_sut.Password.Validations);
            Assert.NotEmpty(_sut.Password.Validations);
            Assert.NotNull(_sut.ConfirmedPassword.Validations);
            Assert.NotEmpty(_sut.ConfirmedPassword.Validations);
        }

        [Fact]
        public async Task On_Signup_Submission_User_Created_If_Fields_Are_Valid()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockMaterialDialog.Verify(x => x.LoadingDialogAsync(It.IsAny<string>(), null), Times.Once);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [ClassData(typeof(InvalidNewUserData))]
        public async Task On_Signup_Submission_User_Not_Created_If_Any_Field_Is_InValid(string emailAddress, string password, string confirmedPassword)
        {
            _sut.EmailAddress.Value = emailAddress;
            _sut.Password.Value = password;
            _sut.ConfirmedPassword.Value = confirmedPassword;
            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.False(isFormValid);
            _mockMaterialDialog.Verify(x => x.LoadingDialogAsync(It.IsAny<string>(), null), Times.Never);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockAccountService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task On_User_Created_User_Is_Logged_In_And_Navigated_To_Home_Page()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));
            _mockAccountService.Setup(x => x.LoginAsync(_sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockMaterialDialog.Verify(x => x.LoadingDialogAsync(It.IsAny<string>(), null), Times.Once);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockAccountService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockBaseService.Verify(x => x.NavigationService.NavigateAsync("/NavigationPage/HomePage"), Times.Once);
        }

        [Fact]
        public async Task On_User_Created_User_Is_Logged_In_And_Snackbar_Displayed()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));
            _mockAccountService.Setup(x => x.LoginAsync(_sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockMaterialDialog.Verify(x => x.LoadingDialogAsync(It.IsAny<string>(), null), Times.Once);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockAccountService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockBaseService.Verify(x => x.NavigationService.NavigateAsync("/NavigationPage/HomePage"), Times.Once);
            _mockMaterialDialog.Verify(x => x.SnackbarAsync(It.IsAny<string>(), MaterialSnackbar.DurationLong, null), Times.Once);
        }

        [Fact]
        public async Task On_User_Created_User_Form_Fields_And_Validations_Reset()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(string.IsNullOrEmpty(_sut.EmailAddress.Value));
            Assert.True(string.IsNullOrEmpty(_sut.Password.Value));
            Assert.True(string.IsNullOrEmpty(_sut.ConfirmedPassword.Value));
            Assert.NotNull(_sut.EmailAddress.Validations);
            Assert.NotEmpty(_sut.EmailAddress.Validations);
            Assert.NotNull(_sut.Password.Validations);
            Assert.NotEmpty(_sut.Password.Validations);
            Assert.NotNull(_sut.ConfirmedPassword.Validations);
            Assert.NotEmpty(_sut.ConfirmedPassword.Validations);
            Assert.False(_sut.IsFormValid);
        }

        [Fact]
        public async Task On_User_Creation_Failure_User_Not_Logged_In()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(false));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockAccountService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task On_User_Creation_Failure_Snackbar_Displayed()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(false));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockMaterialDialog.Verify(x => x.SnackbarAsync(It.IsAny<string>(), MaterialSnackbar.DurationLong, null), Times.Once);
        }        

        [Fact]
        public async Task On_User_Created_Login_Fails_And_User_Navigated_To_Login_Page()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));
            _mockAccountService.Setup(x => x.LoginAsync(_sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(false));

            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockAccountService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockBaseService.Verify(x => x.NavigationService.NavigateAsync("LoginPage", null, true, true), Times.Once);
        }

        [Fact]
        public async Task On_User_Created_Login_Fails_And_Snackbar_Displayed()
        {
            SetupValidNewUser();
            _mockAccountService.Setup(x => x.RegisterUserAsync(string.Empty, _sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(true));
            _mockAccountService.Setup(x => x.LoginAsync(_sut.EmailAddress.Value, _sut.Password.Value)).Returns(Task.FromResult(false));

            
            var isFormValid = _sut.IsFormValid;
            await _sut.SignUpCommand.ExecuteAsync();

            Assert.True(isFormValid);
            _mockAccountService.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockAccountService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockBaseService.Verify(x => x.NavigationService.NavigateAsync("LoginPage", null, true, true), Times.Once);
            _mockMaterialDialog.Verify(x => x.SnackbarAsync(It.IsAny<string>(), MaterialSnackbar.DurationLong, null), Times.Once);
        }

        [Fact]
        public void ValidateEmailCommand_Should_Be_Executed_And_Email_Address_Validated()
        {
            var mockEmailAddress = new Mock<IValidatableObject<string>>();
            _sut.EmailAddress = mockEmailAddress.Object;

            _sut.ForceValidateEmailCommand.Execute(null);
            _sut.ValidateEmailCommand.Execute(null);

            mockEmailAddress.Verify(x => x.Validate(), Times.Exactly(2));
        }

        [Fact]
        public void ValidateEmailCommand_Should_Be_Executed_But_Email_Address_Not_Validated()
        {
            var mockEmailAddress = new Mock<IValidatableObject<string>>();
            _sut.EmailAddress = mockEmailAddress.Object;

            _sut.ValidateEmailCommand.Execute(null);

            mockEmailAddress.Verify(x => x.Validate(), Times.Never);
        }

        [Fact]
        public void ValidatePasswordCommand_Should_Be_Executed_And_Password_Validated()
        {
            var mockPassword = new Mock<IValidatableObject<string>>();
            _sut.Password = mockPassword.Object;

            _sut.ForceValidatePasswordCommand.Execute(null);
            _sut.ValidatePasswordCommand.Execute(null);

            mockPassword.Verify(x => x.Validate(), Times.Exactly(2));
        }

        [Fact]
        public void ValidatePasswordCommand_Should_Be_Executed_But_Password_Not_Validated()
        {
            var mockPassword = new Mock<IValidatableObject<string>>();
            _sut.Password = mockPassword.Object;

            _sut.ValidatePasswordCommand.Execute(null);

            mockPassword.Verify(x => x.Validate(), Times.Never);
        }

        [Fact]
        public void ValidatePasswordMatchCommand_Should_Be_Executed_And_Password_Validated()
        {
            var mockConfirmedPassword = new Mock<IValidatableObject<string>>();
            _sut.ConfirmedPassword = mockConfirmedPassword.Object;

            _sut.ForceValidatePasswordMatchCommand.Execute(null);
            _sut.ValidatePasswordMatchCommand.Execute(null);

            mockConfirmedPassword.Verify(x => x.Validate(), Times.Exactly(2));
        }

        [Fact]
        public void ValidatePasswordMatchCommand_Should_Be_Executed_But_Password_Not_Validated()
        {
            var mockConfirmedPassword = new Mock<IValidatableObject<string>>();
            _sut.ConfirmedPassword = mockConfirmedPassword.Object;

            _sut.ValidatePasswordMatchCommand.Execute(null);

            mockConfirmedPassword.Verify(x => x.Validate(), Times.Never);
        }

        [Fact]
        public async Task User_Navigated_To_Sign_In_Page_And_Form_Values_Reset()
        {
            SetupValidNewUser();

            await _sut.NavigateToSignInPageCommand.ExecuteAsync();

            _mockBaseService.Verify(x => x.NavigationService.NavigateAsync("LoginPage", null, true, true), Times.Once);
            Assert.True(string.IsNullOrEmpty(_sut.EmailAddress.Value));
            Assert.True(string.IsNullOrEmpty(_sut.Password.Value));
            Assert.True(string.IsNullOrEmpty(_sut.ConfirmedPassword.Value));
            Assert.NotNull(_sut.EmailAddress.Validations);
            Assert.NotEmpty(_sut.EmailAddress.Validations);
            Assert.NotNull(_sut.Password.Validations);
            Assert.NotEmpty(_sut.Password.Validations);
            Assert.NotNull(_sut.ConfirmedPassword.Validations);
            Assert.NotEmpty(_sut.ConfirmedPassword.Validations);
            Assert.False(_sut.IsFormValid);
        }

        // TODO - User not created if email is null
        // TODO - User not created if email is not valid
        // TODO - User not created if password is null
        // TODO - User not created if password is not valid
        // TODO - User not created if confirm password is null
        // TODO - User not created if confirm password is not the same as password

        // TODO - Unit Test each rule? Or Unit Test all validations in one method?


        // TODO - valid emails

        // TODO - valid passwords

        // TODO - not valid emails

        // TODO - not valid passwords

        // TODO - Passwords match

        // TODO - Passwords don't match

        private void SetupValidNewUser()
        {
            _sut.EmailAddress.Value = "test@test.com";
            _sut.Password.Value = "P@ssword1";
            _sut.ConfirmedPassword.Value = "P@ssword1";
        }

        public class InvalidNewUserData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { null, null, null };
                yield return new object[] { string.Empty, string.Empty, string.Empty };
                yield return new object[] { string.Empty, "P@ssword1", "P@ssword1" };
                yield return new object[] { "test@test.com", string.Empty, "P@ssword1" };
                yield return new object[] { "test@test.com", "P@ssword1", string.Empty };
                yield return new object[] { "test@test.com.", "P@ssword1", "P@ssword1" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

    }
}
