using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Linq;
using WpfFluentValidation.Validators;

namespace WpfFluentValidation.Models;

/// <summary>
///     扩展字段，用于生成动态表单
///     继承BindableBase,即继承属性变化接口INotifyPropertyChanged
///     实现IDataErrorInfo接口，用于FluentValidation验证，必须实现此接口
/// </summary>
public class Field : BindableBase, IDataErrorInfo
{
    private string _value;
    private readonly FieldValidator _validator = new();


    public Field(DataType type, string typeLabel, string name, string value)
    {
        Type = type;
        TypeLabel = typeLabel;
        Name = name;
        Value = value;
    }

    /// <summary>
    ///     数据类型
    /// </summary>
    public DataType Type { get; set; }

    /// <summary>
    ///     数据类型名称
    /// </summary>
    public string TypeLabel { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     值
    /// </summary>
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public string this[string columnName]
    {
        get
        {
            var validateResult = _validator.Validate(this);
            if (validateResult.IsValid)
            {
                return string.Empty;
            }

            var firstOrDefault =
                validateResult.Errors.FirstOrDefault(error => error.PropertyName == columnName);
            return firstOrDefault == null ? string.Empty : firstOrDefault.ErrorMessage;
        }
    }

    public string Error
    {
        get
        {
            var validateResult = _validator.Validate(this);
            if (validateResult.IsValid)
            {
                return string.Empty;
            }

            var errors = string.Join(Environment.NewLine, validateResult.Errors.Select(x => x.ErrorMessage).ToArray());
            return errors;
        }
    }
}

public enum DataType
{
    Text,
    Number,
    Date
}