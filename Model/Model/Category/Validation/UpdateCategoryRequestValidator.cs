using FluentValidation;

namespace Domain.Model
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(s => s.Description)
              .NotEmpty().WithMessage("Description é obrigatório.")
              .NotNull().WithMessage("Description é obrigatório.");

            RuleFor(s => s.Active)
             .NotEmpty().WithMessage("Active é obrigatório.")
             .NotNull().WithMessage("Active é obrigatório.");

        }
    }
}
