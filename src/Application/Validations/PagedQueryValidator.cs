using DevKit.Domain.Models;
using FluentValidation;

namespace DevKit.Application.Validations;

public class PagedQueryValidator : AbstractValidator<PagedQuery>
{
    public PagedQueryValidator() {
        When(p => p.PageNumber.HasValue, () => RuleFor(e => e.PageNumber).GreaterThanOrEqualTo(1));
        When(p => p.PageSize.HasValue, () => RuleFor(e => e.PageSize).GreaterThanOrEqualTo(1));
    }
}
