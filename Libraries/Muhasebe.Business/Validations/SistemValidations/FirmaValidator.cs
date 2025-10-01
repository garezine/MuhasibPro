using FluentValidation;
using Muhasebe.Business.Models.SistemModel;

namespace Muhasebe.Business.Validations.SistemValidations
{
    public class FirmaValidator : AbstractValidator<FirmaModel>
    {
        public FirmaValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Continue;
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.KisaUnvani)
                .NotEmpty()
                .WithMessage("'Kısa Ünvan' boş geçilemez!")
                .MinimumLength(10)
                .WithMessage("En az 10 karakter olmalı");

            RuleFor(p => p.YetkiliKisi)
                .NotEmpty()
                .WithMessage("'Yetkili Kişi' boş geçilemez!")
                .MinimumLength(10)
                .WithMessage("En az 10 karakter olmalı");

            // Telefon 1 - Zorunlu
            RuleFor(p => p.Telefon1)
                .NotEmpty()
                .WithMessage("Telefon numarası zorunludur")
                .Matches(@"^0\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}$")
                .WithMessage("Geçerli bir telefon numarası giriniz (11 haneli)");
        }
    }
}
