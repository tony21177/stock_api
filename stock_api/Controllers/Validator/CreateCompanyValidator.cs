﻿using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class CreateManufacturerValidator : AbstractValidator<CreateManufacturerRequest>
    {
        public CreateManufacturerValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("code為必須");
            RuleFor(x => x.Name).NotEmpty().WithMessage("name為必須");
            RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
        }
    }
}
