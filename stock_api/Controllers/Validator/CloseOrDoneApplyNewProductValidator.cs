using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class CloseOrDoneApplyNewProductValidator : AbstractValidator<CloseOrDoneApplyNewProductRequest>
    {
        

        public CloseOrDoneApplyNewProductValidator()
        {
            RuleFor(request => request.CurrentStatus)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(status => new List<string> { "DONE", "CLOSE" }.Contains(status))
                    .WithMessage($"currentStatus必須為{string.Join(",", new List<string> { "DONE", "CLOSE" })}");
            

        }

    }
}
