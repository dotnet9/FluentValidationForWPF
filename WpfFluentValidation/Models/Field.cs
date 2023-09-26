using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WpfFluentValidation.Validators;

namespace WpfFluentValidation.Models
{
    /// <summary>
    /// 扩展字段
    /// </summary>
    public class Field : BaseClass, IDataErrorInfo
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType Type { get; set; }
        /// <summary>
        /// 数据类型名称
        /// </summary>
        public string TypeLabel { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        private string _value;
        /// <summary>
        /// 值
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }



        public Field(DataType type, string typeLabel, string name, string value)
        {
            Type = type;
            TypeLabel = typeLabel;
            Name = name;
            Value = value;
        }
        public string Error { get; set; }

        public string this[string columnName]
        {
            get
            {
                if(columnName == nameof(Value))
                  {
                    switch(Type)
                    {
                        case DataType.Text:
                            if(string.IsNullOrWhiteSpace(Value))
                            {
                                return "值不能为空";
                            }
                            break;
                            case DataType.Number:
                            if(!double.TryParse(Value, out var number)) {

                                return "值为数字";
                            }
                            break;
                        case DataType.Date:
                            if (!DateTime.TryParse(Value, out var date))
                            {
                                return "值为日期";
                            }
                            break;
                    }
                }
                return string.Empty;

            }
        }
    }

    public enum DataType
    {
        Text,
        Number,
        Date
    }
}
