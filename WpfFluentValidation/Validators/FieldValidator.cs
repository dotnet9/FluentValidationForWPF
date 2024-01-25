using System;
using FluentValidation;
using WpfFluentValidation.Models;

namespace WpfFluentValidation.Validators;

public class FieldValidator : AbstractValidator<Field>
{
    public FieldValidator()
    {
        RuleFor(field => field.Value)
            .Must((field, value) => (field.Type == DataType.Text && !string.IsNullOrWhiteSpace(value))
                                    || (field.Type == DataType.Number && double.TryParse(value, out _))
                                    || (field.Type == DataType.Date && DateTime.TryParse(value, out _)))
            .WithMessage("1.文本不能为空；2.数字类型请填写数字；3.日志类型请填写日期类型");
    }
}