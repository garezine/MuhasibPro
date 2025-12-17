using FluentValidation;
using Muhasib.Business.Models.SistemModel;

namespace Muhasib.Business.Validations.SistemValidations
{
    public class FirmaValidator : AbstractValidator<FirmaModel>
    {
        public FirmaValidator()
        {
            string mesaj = "Gerekli alan!";
            ClassLevelCascadeMode = CascadeMode.Continue;
            RuleLevelCascadeMode = CascadeMode.Stop;
           
            RuleFor(p => p.KisaUnvani)
               .NotEmpty()
               .WithMessage(mesaj)
               .MinimumLength(10)
               .WithMessage("En az 10 karakter olmalı");
            RuleFor(p => p.TamUnvani)
               .NotEmpty()
               .WithMessage(mesaj)
               .MinimumLength(10)
               .WithMessage("En az 10 karakter olmalı");

            RuleFor(p => p.YetkiliKisi)
                .NotEmpty()
                .WithMessage(mesaj)
                .MinimumLength(10)
                .WithMessage("En az 10 karakter olmalı");

            // Telefon 1 - Zorunlu
            RuleFor(p => p.Telefon1)
                .NotEmpty()
                .WithMessage(mesaj)
                .Matches(@"^0\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}$")
                .WithMessage("Geçerli bir telefon numarası giriniz (11 haneli)");
        }
    }
}
