using Domain.Model.Product.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Product.Validation
{
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator() 
        {
            RuleFor(s => s.Name)
             .NotEmpty().WithMessage("Description é obrigatório.")
             .NotNull().WithMessage("Description é obrigatório.");
            RuleFor(s => s.Type)
           .NotEmpty().WithMessage("Type é obrigatório.")
           .NotNull().WithMessage("Type é obrigatório.");
            RuleFor(s => s.Updated_by)
           .NotEmpty().WithMessage("Updated_by é obrigatório.")
           .NotNull().WithMessage("Updated_by é obrigatório.");
        }
    }
}
