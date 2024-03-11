using FluentValidation;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class ListAnnouncementRequestValidator : AbstractValidator<ListAnnoucementRequest>
    {
        public ListAnnouncementRequestValidator()
        {
            When(request => request.IsPagination, () =>
            {
                RuleFor(request => request.PageIndex)
                    .NotNull().WithMessage("PageIndex is required when IsPagination is true.")
                    .GreaterThan(0).WithMessage("PageIndex should be greater than 0.");

                RuleFor(request => request.PageSize)
                    .GreaterThan(0).WithMessage("PageSize should be greater than 0.");
            });
        }
    }
}
