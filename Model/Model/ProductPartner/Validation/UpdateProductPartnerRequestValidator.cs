using FluentValidation;

namespace Domain.Model
{
    public class UpdateProductPartnerRequestValidator : AbstractValidator<UpdateProductPartnerRequest>
    {
        public UpdateProductPartnerRequestValidator()
        {

            RuleFor(s => s.Name)
             .NotEmpty().WithMessage("Name é obrigatório.")
             .NotNull().WithMessage("Name é obrigatório.");
            RuleFor(s => s.Updated_by)
              .NotEmpty().WithMessage("Updated_by é obrigatório.")
              .NotNull().WithMessage("Updated_by é obrigatório.");

        }
    }
}
