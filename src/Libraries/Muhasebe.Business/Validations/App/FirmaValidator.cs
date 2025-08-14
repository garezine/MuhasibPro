using FluentValidation;
using Muhasebe.Business.Models.DbModel.AppModel;

namespace Muhasebe.Business.Validations.App
{
    public class FirmaValidator : AbstractValidator<FirmaModel>
    {
        public FirmaValidator()
        {
            RuleFor(p => p.KisaUnvani)
                .NotEmpty().WithMessage("'Kısa Ünvan' boş geçilemez!")
                .MinimumLength(10).WithMessage("En az 10 karakter olmalı");
        }
    }
}
