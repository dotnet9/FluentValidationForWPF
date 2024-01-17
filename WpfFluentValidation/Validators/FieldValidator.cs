using System;
using FluentValidation;
using WpfFluentValidation.Models;

namespace WpfFluentValidation.Validators;

public class FieldValidator : AbstractValidator<Field>
{
    public FieldValidator()
    {
        RuleFor(field => field.Value)
            .Must((field, value) => (field.Type == DataType.Text && !string.IsNullOrWhiteSpace(field.Value))
                                    || (field.Type == DataType.Number && double.TryParse(field.Value, out var _))
                                    || (field.Type == DataType.Date && DateTime.TryParse(field.Value, out var _)))
            .WithMessage("1.文本不能为空；2.数字类型请填写数字；3.日志类型请填写日期类型");
    }
}