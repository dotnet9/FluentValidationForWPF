using FluentValidation;
using System.Text.RegularExpressions;
using WpfFluentValidation.Models;

namespace WpfFluentValidation.Validators
{
    public class StudentValidator : AbstractValidator<Student>
    {
        public StudentValidator()
        {
            RuleFor(vm => vm.Name)
                    .NotEmpty()
                    .WithMessage("请输入学生姓名！")
                .Length(5, 30)
                .WithMessage("学生姓名长度限制在5到30个字符之间！");

            RuleFor(vm => vm.Age)
                .GreaterThanOrEqualTo(0)
                .WithMessage("学生年龄为整数！")
                .ExclusiveBetween(10, 150)
                .WithMessage($"请正确输入学生年龄(10-150)");

            RuleFor(vm => vm.Zip)
                .NotEmpty()
                .WithMessage("邮政编码不能为空！")
                .Must(BeAValidZip)
                .WithMessage("邮政编码由六位数字组成。");
        }

        private static bool BeAValidZip(string zip)
        {
            if (!string.IsNullOrEmpty(zip))
            {
                var regex = new Regex(@"\d{6}");
                return regex.IsMatch(zip);
            }
            return false;
        }
    }
}
