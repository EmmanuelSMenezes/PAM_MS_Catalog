using Domain.Model.Product.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Product.Validation
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(s => s.Name)
              .NotEmpty().WithMessage("Description é obrigatório.")
              .NotNull().WithMessage("Description é obrigatório.");
            RuleFor(s => s.Category)
              .NotEmpty().WithMessage("Category é obrigatório.")
              .NotNull().WithMessage("Category é obrigatório.");
            RuleFor(s => s.Created_by)
             .NotEmpty().WithMessage("Created_by é obrigatório.")
             .NotNull().WithMessage("Created_by é obrigatório.");
            RuleFor(s => s.Admin_id)
             .NotEmpty().WithMessage("Admin_id é obrigatório.")
             .NotNull().WithMessage("Admin_id é obrigatório.");
            RuleFor(s => s.Type)
            .NotEmpty().WithMessage("Type é obrigatório.")
            .NotNull().WithMessage("Type é obrigatório.");

        }
    }
}
