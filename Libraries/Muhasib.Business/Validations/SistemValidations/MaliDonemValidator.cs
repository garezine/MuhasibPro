using FluentValidation;
using Muhasib.Business.Models.SistemModel;

namespace Muhasib.Business.Validations.SistemValidations
{
    public class MaliDonemValidator : AbstractValidator<MaliDonemModel>
    {
        public MaliDonemValidator() 
        {
            string mesaj = "Gerekli alan!";
            ClassLevelCascadeMode = CascadeMode.Continue;
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(p => p.FirmaId)
                .NotEmpty()
                .WithMessage(mesaj);
            RuleFor(p=> p.MaliYil)
                .NotEmpty()
                .WithMessage(mesaj);
        }
    }
}
