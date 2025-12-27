using FluentValidation;
using SmartRestaurant.Application.Dtos.Requests;

namespace SmartRestaurant.Application.Validations;

public class RefreshTokenRequestModelValidator : AbstractValidator<RefreshTokenRequestModel>
{
    public RefreshTokenRequestModelValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(32)
            .WithMessage("Invalid refresh token");
    }
}