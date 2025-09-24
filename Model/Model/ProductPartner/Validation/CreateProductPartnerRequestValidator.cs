using FluentValidation;

namespace Domain.Model
{
    public class CreateProductPartnerRequestValidator : AbstractValidator<CreateProductPartnerRequest>
    {
        public CreateProductPartnerRequestValidator()
        {
            RuleFor(s => s.Base_product_id)
              .NotEmpty().WithMessage("Base_product_id é obrigatório.")
              .NotNull().WithMessage("Base_product_id é obrigatório.");
           
            RuleFor(s => s.Created_by)
              .NotEmpty().WithMessage("Created_by é obrigatório.")
              .NotNull().WithMessage("Created_by é obrigatório.");

        }
    }
}
