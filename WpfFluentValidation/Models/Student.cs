using System.ComponentModel;
using System.Linq;
using WpfFluentValidation.Validators;

namespace WpfFluentValidation.Models
{
    /// <summary>
    /// 学生实体
    /// 继承BaseClasss,即继承属性变化接口INotifyPropertyChanged
    /// 实现IDataErrorInfo接口，用于FluentValidation验证，必须实现此接口
    /// </summary>
    public class Student : BaseClass, IDataErrorInfo
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private int age;
        public int Age
        {
            get { return age; }
            set
            {
                if (value != age)
                {
                    age = value;
                    OnPropertyChanged(nameof(Age));
                }
            }
        }
        private string zip;
        public string Zip
        {
            get { return zip; }
            set
            {
                if (value != zip)
                {
                    zip = value;
                    OnPropertyChanged(nameof(Zip));
                }
            }
        }

        public string Error { get; set; }

        public string this[string columnName]
        {
            get
            {
                if (validator == null)
                {
                    validator = new StudentValidator();
                }
                var firstOrDefault = validator.Validate(this)
                    .Errors.FirstOrDefault(lol => lol.PropertyName == columnName);
                return firstOrDefault?.ErrorMessage;
            }
        }

        private StudentValidator validator { get; set; }
    }
}
