﻿using System.Text.RegularExpressions;
using FluentValidation;
using WpfFluentValidation.Models;

namespace WpfFluentValidation.Validators;

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
            .WithMessage("请正确输入学生年龄(10-150)");

        _ = RuleFor(vm => vm.Zip)
            .NotEmpty()
            .WithMessage("邮政编码不能为空！")
            .Must(BeAValidZip)
            .WithMessage("邮政编码由六位数字组成。");

        RuleFor(model => model.MinValue).Must((model, minValue) => minValue < model.MaxValue).WithMessage("最小值应该小于最大值");

        RuleFor(model => model.MaxValue).Must((model, maxValue) => maxValue > model.MinValue).WithMessage("最大值应该大于最小值");
    }

    private static bool BeAValidZip(string? zip)
    {
        if (string.IsNullOrEmpty(zip))
        {
            return false;
        }

        var regex = new Regex(@"\d{6}");
        return regex.IsMatch(zip);
    }
}