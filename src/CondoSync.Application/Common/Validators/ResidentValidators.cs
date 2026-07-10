using FluentValidation;
using CondoSync.Application.Features.Residents.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateResidentRequestValidator : AbstractValidator<CreateResidentRequest>
{
    public CreateResidentRequestValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ResidentType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Cpf).MaximumLength(14);
        RuleFor(x => x.Rg).MaximumLength(20);
        RuleFor(x => x.Profession).MaximumLength(100);
        RuleForEach(x => x.Vehicles).SetValidator(new VehicleDTOValidator()).When(x => x.Vehicles != null);
        RuleForEach(x => x.Pets).SetValidator(new PetDTOValidator()).When(x => x.Pets != null);
    }
}

public class UpdateResidentRequestValidator : AbstractValidator<UpdateResidentRequest>
{
    public UpdateResidentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Profession).MaximumLength(100);
    }
}

public class ToggleAccessRequestValidator : AbstractValidator<ToggleAccessRequest>
{
    public ToggleAccessRequestValidator()
    {
        RuleFor(x => x.GrantAccess).NotNull();
    }
}

public class VehicleDTOValidator : AbstractValidator<VehicleDTO>
{
    public VehicleDTOValidator()
    {
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.Brand).MaximumLength(100);
    }
}

public class PetDTOValidator : AbstractValidator<PetDTO>
{
    public PetDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Species).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Breed).MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
    }
}
