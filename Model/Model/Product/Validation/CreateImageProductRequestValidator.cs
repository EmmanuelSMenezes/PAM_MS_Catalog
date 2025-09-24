using Domain.Model.Product.Image.Request;
using Domain.Model.Product.Request;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Product.Validation
{
    public class CreateImageProductRequestValidator : AbstractValidator<CreateImageProductRequest>
    {
        public CreateImageProductRequestValidator()
        {
            RuleFor(s => s.product_id)
              .NotEmpty().WithMessage("product_id é obrigatório.")
              .NotNull().WithMessage("product_id é obrigatório.");
            RuleFor(s => s.Image)
              .NotEmpty().WithMessage("Image é obrigatório.")
              .NotNull().WithMessage("Image é obrigatório.");
            RuleFor(s => s.Image)
              .NotEmpty().WithMessage("Image é obrigatório.")
              .NotNull().WithMessage("Image é obrigatório.");
            RuleFor(x => x.Image).SetValidator(new FileValidator());

        }
    }
    public class FileValidator : AbstractValidator<IFormFile>
    {
        public FileValidator()
        {
            
            RuleFor(x => x.ContentType).NotNull().Must(x => x.Equals("image/jpeg") || x.Equals("image/jpg") || x.Equals("image/png"))
                .WithMessage("Tipo de imagem inválido.");
        }
    }
}
