﻿using FluentValidation;
using NLayer.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayer.Service.Validation
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {

            RuleFor(X => X.Name)
                .NotNull().WithMessage("{PropertyName} is required")
                .NotEmpty().WithMessage("{PropertyName} is required");

            RuleFor(X => X.Price)
                .InclusiveBetween(1,int.MaxValue).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(X => X.Stock)
                .InclusiveBetween(1, int.MaxValue).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(X => X.CategoryId)
                .InclusiveBetween(1, int.MaxValue).WithMessage("{PropertyName} must be greater than 0.");
        }
    }
}
