﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using WpfFluentValidation.Models;
using WpfFluentValidation.Validators;

namespace WpfFluentValidation.ViewModels;

/// <summary>
///     视图ViewModel
///     继承BindableBase,即继承属性变化接口INotifyPropertyChanged
///     实现IDataErrorInfo接口，用于FluentValidation验证，必须实现此接口
/// </summary>
public class StudentViewModel : BindableBase, IDataErrorInfo
{
    private Student _currentStudent;
    private string _title;

    private readonly StudentViewModelValidator _validator;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public Student CurrentStudent
    {
        get => _currentStudent;
        set => SetProperty(ref _currentStudent, value);
    }

    public ObservableCollection<Field> Fields { get; } = new();

    private DelegateCommand _saveCommand;

    public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(HandleSaveCommand,
        HandleCanExecuteSaveCommand);

    private DelegateCommand _cancelCommand;

    public DelegateCommand CancelCommand =>
        _cancelCommand ??= new DelegateCommand(HandleCancelCommand, () => true);

    public StudentViewModel()
    {
        _validator = new StudentViewModelValidator();
        CurrentStudent = new Student
        {
            Name = "李刚的儿",
            Age = 23
        };
        Fields.Add(new Field(DataType.Text, "文本，比如：四川省成都市", "地址", ""));
        Fields.Add(new Field(DataType.Number, "数字，比如：12", "工龄", ""));
        Fields.Add(new Field(DataType.Date, "时间，比如：2023-09-26 05:13:23", "培训时间", ""));

        PropertyChanged += Validate;
        CurrentStudent.PropertyChanged += Validate;
        foreach (var field in Fields)
        {
            field.PropertyChanged += Validate;
        }
    }

    ~StudentViewModel()
    {
        PropertyChanged -= Validate;
        CurrentStudent.PropertyChanged -= Validate;
        foreach (var field in Fields)
        {
            field.PropertyChanged -= Validate;
        }
    }

    private void Validate(object sender, PropertyChangedEventArgs e)
    {
        _isCanExecuteSaveCommand = _validator.Validate(this).IsValid;
        SaveCommand.RaiseCanExecuteChanged();
    }

    private void HandleSaveCommand()
    {
        var validateResult = _validator.Validate(this);
        if (validateResult.IsValid)
        {
            MessageBox.Show("看到我说明验证成功！");
        }
        else
        {
            var errorMsg = string.Join(Environment.NewLine,
                validateResult.Errors.Select(x => x.ErrorMessage).ToArray());
            MessageBox.Show($"慌啥子嘛，你再检查下输入噻：\r\n{errorMsg}");
        }
    }

    private bool _isCanExecuteSaveCommand;

    private bool HandleCanExecuteSaveCommand()
    {
        return _isCanExecuteSaveCommand;
    }

    private void HandleCancelCommand()
    {
        MessageBox.Show("我啥都不做，退休了");
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