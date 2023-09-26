using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WpfFluentValidation.Models;
using WpfFluentValidation.Validators;

namespace WpfFluentValidation.ViewModels
{
    /// <summary>
    /// 视图ViewModel
    /// 继承BaseClasss,即继承属性变化接口INotifyPropertyChanged
    /// 实现IDataErrorInfo接口，用于FluentValidation验证，必须实现此接口
    /// </summary>
    public class StudentViewModel : BaseClass, IDataErrorInfo
    {
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private Student currentStudent;
        public Student CurrentStudent
        {
            get { return currentStudent; }
            set
            {
                if (value != currentStudent)
                {
                    currentStudent = value;
                    OnPropertyChanged(nameof(CurrentStudent));
                }
            }
        }
        public ObservableCollection<Field> Fields { get; }= new ObservableCollection<Field>();

        public StudentViewModel()
        {
            CurrentStudent = new Student()
            {
                Name = "李刚的儿",
                Age = 23
            };
            Fields.Add(new Field(DataType.Text, "文本，比如：四川省成都市场", "地址", ""));
            Fields.Add(new Field(DataType.Number, "数字，比如：12", "工龄", ""));
            Fields.Add(new Field(DataType.Date, "时间，比如：2023-09-26 05:13:23", "培训时间", ""));
        }


        public string this[string columnName]
        {
            get
            {
                if (validator == null)
                {
                    validator = new ViewModelValidator();
                }
                var firstOrDefault = validator.Validate(this)
                    .Errors.FirstOrDefault(lol => lol.PropertyName == columnName);
                return firstOrDefault?.ErrorMessage;
            }
        }
        public string Error
        {
            get
            {
                var results = validator.Validate(this);
                if (results != null && results.Errors.Any())
                {
                    var errors = string.Join(Environment.NewLine, results.Errors.Select(x => x.ErrorMessage).ToArray());
                    return errors;
                }

                return string.Empty;
            }
        }

        private ViewModelValidator validator;
    }
}