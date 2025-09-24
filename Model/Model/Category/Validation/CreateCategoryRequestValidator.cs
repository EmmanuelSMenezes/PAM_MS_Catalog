using FluentValidation;

namespace Domain.Model
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(s => s.Description)
              .NotEmpty().WithMessage("Description é obrigatório.")
              .NotNull().WithMessage("Description é obrigatório.");

        }
    }
}
