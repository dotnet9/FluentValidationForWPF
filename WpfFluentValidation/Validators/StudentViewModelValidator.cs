using FluentValidation;
using WpfFluentValidation.ViewModels;

namespace WpfFluentValidation.Validators;

public class StudentViewModelValidator : AbstractValidator<StudentViewModel>
{
    public StudentViewModelValidator()
    {
        RuleFor(vm => vm.Title)
            .NotEmpty()
            .WithMessage("标题长度不能为空！")
            .Length(5, 30)
            .WithMessage("标题长度限制在5到30个字符之间！");

        RuleFor(vm => vm.CurrentStudent).SetValidator(new StudentValidator());

        RuleForEach(vm => vm.Fields).SetValidator(new FieldValidator());
    }
}