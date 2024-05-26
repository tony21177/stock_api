using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateOwnerProcessValidator : AbstractValidator<UpdateOwnerProcessRequest>
    {
        

        public UpdateOwnerProcessValidator()
        {
            RuleFor(request => request.OwnerProcess)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(status => CommonConstants.PurchaseMainOwnerProcessStatus.GetAllValues().Contains(status))
                    .WithMessage($"ownerProcess必須為{string.Join(",", CommonConstants.PurchaseMainOwnerProcessStatus.GetAllValues())}");
            

        }

    }
}
