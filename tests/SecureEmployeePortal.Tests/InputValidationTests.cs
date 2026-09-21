// Verifies server-side data-annotation validation rules used by portal forms.
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureEmployeePortal.Models;

namespace SecureEmployeePortal.Tests;

[TestClass]
public sealed class InputValidationTests
{
    [TestMethod]
    public void AccessRequest_RejectsShortReason()
    {
        var model = new AccessRequestInputModel
        {
            RequestedResource = "Finance reporting workspace",
            Reason = "Need it"
        };

        Assert.IsFalse(IsValid(model));
    }

    [TestMethod]
    public void AccessRequest_AcceptsCompleteReason()
    {
        var model = new AccessRequestInputModel
        {
            RequestedResource = "Finance reporting workspace",
            Reason = "Required to prepare the monthly reporting pack for my assigned role."
        };

        Assert.IsTrue(IsValid(model));
    }

    [TestMethod]
    public void Register_RequiresValidEmailMatchingPasswordsAndPolicyAcceptance()
    {
        var model = new RegisterInputModel
        {
            FirstName = "Test",
            LastName = "Employee",
            WorkEmail = "not-an-email",
            Password = "SecureTest1!",
            ConfirmPassword = "DifferentTest1!",
            AcceptTerms = false
        };

        var errors = Validate(model);

        Assert.IsTrue(errors.Count >= 3);
        Assert.IsTrue(errors.Any(error => error.MemberNames.Contains(nameof(RegisterInputModel.WorkEmail))));
        Assert.IsTrue(errors.Any(error => error.MemberNames.Contains(nameof(RegisterInputModel.ConfirmPassword))));
        Assert.IsTrue(errors.Any(error => error.MemberNames.Contains(nameof(RegisterInputModel.AcceptTerms))));
    }

    [TestMethod]
    public void Register_AcceptsCompleteFormInput()
    {
        var model = new RegisterInputModel
        {
            FirstName = "Test",
            LastName = "Employee",
            WorkEmail = "test.employee@example.com",
            Password = "SecureTest1!",
            ConfirmPassword = "SecureTest1!",
            AcceptTerms = true
        };

        Assert.IsTrue(IsValid(model));
    }

    [TestMethod]
    public void ChangePassword_RejectsMismatchedConfirmation()
    {
        var model = new ChangePasswordInputModel
        {
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!",
            ConfirmPassword = "DifferentPassword1!"
        };

        Assert.IsFalse(IsValid(model));
    }

    [TestMethod]
    public void ResetPassword_RejectsMismatchedConfirmation()
    {
        var model = new ResetPasswordInputModel
        {
            Email = "test.employee@example.com",
            Code = "encoded-token",
            Password = "NewPassword1!",
            ConfirmPassword = "DifferentPassword1!"
        };

        Assert.IsFalse(IsValid(model));
    }

    [TestMethod]
    public void Profile_RejectsOverlongName()
    {
        var model = new ProfileInputModel
        {
            FirstName = new string('A', 101),
            LastName = "Employee"
        };

        Assert.IsFalse(IsValid(model));
    }

    [TestMethod]
    public void CreateEmployee_RequiresValidEmailAndMatchingTemporaryPassword()
    {
        var model = new CreateEmployeeInputModel
        {
            FirstName = "Test",
            LastName = "Employee",
            Email = "invalid",
            EmployeeNumber = "EMP-0001",
            Department = "Operations",
            JobTitle = "Employee",
            Role = "Employee",
            TemporaryPassword = "SecureTest1!",
            ConfirmTemporaryPassword = "DifferentTest1!"
        };

        Assert.IsFalse(IsValid(model));
    }

    private static bool IsValid(object model) => Validate(model).Count == 0;

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
