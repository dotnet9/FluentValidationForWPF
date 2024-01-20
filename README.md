---
title: FluentValidation在C# WPF中的应用
slug: Uses-fluent-validation-in-WPF
description: 介绍FluentValidation的文章不少，其实它也可以用于WPF属性验证，本文主要也是讲解该组件在WPF中的使用
date: 2019-11-19 03:43:13
lastmod: 2024-01-20 10:59:24
author: 沙漠尽头的狼
draft: false
cover: https://img1.dotnet9.com/2019/11/cover_01.png
categories: WPF
copyright: Original
tags: WPF,FluentValiatoin
---

## 1. 简介

介绍FluentValidation的文章不少，[零度编程](https://www.xcode.me/post/5849)(站长注：原文已失连)的介绍我引用下：

>FluentValidation 是一个基于 .NET 开发的验证框架，开源免费，而且优雅，支持链式操作，易于理解，功能完善，还是可与 MVC5、WebApi2 和 ASP.NET CORE 深度集成，组件内提供十几种常用验证器，可扩展性好，支持自定义验证器，支持本地化多语言。

其实它也可以用于WPF属性验证，本文主要也是讲解该组件在WPF中的使用，FluentValidation官网是： https://fluentvalidation.net/ 。

## 2. 本文需要实现的功能

提供WPF界面输入验证，采用MVVM方式，需要以下功能：

1. 能验证ViewModel中定义的基本数据类型属性：int\string等；
2. 能验证ViewModel中定义的复杂属性，比如1）对象属性的子属性，如VM有个学生属性Student，需要验证他的姓名、年龄等，2）集合属性，动态生成表单验证类；
3. 能简单提供两种验证样式；

先看实现效果图：

![](https://img1.dotnet9.com/2019/11/0101.png)

## 3. 调研中遇到的问题

简单属性：验证ViewModel的普通属性比较简单，可以参考[FluentValidation官网](https://fluentvalidation.net/) ，或者国外holymoo大神的代码： [UserValidator.cs](https://gist.github.com/holymoo/11243164) 。

复杂属性：我遇到的问题是，怎么验证ViewModel中对象属性的子属性？见第二个功能描述，[FluentValidation官网](https://fluentvalidation.net/)有[Complex Properties](https://fluentvalidation.net/start#complex-properties)的例子，但是我试了没效果，贴上官方源码截图：

![](https://img1.dotnet9.com/2019/11/0102.jpg)

最后我Google到这篇文章，根据该[链接代码](https://adityaswami89.wordpress.com/category/xaml/wpf/)，ViewModel和子属性都实现IDataErrorInfo接口，即可实现复杂属性验证，文章中没有具体实现，但灵感是从这来的，就不具体说该链接代码了，有兴趣可以点击链接阅读，下面贴上代码。

## 4. 开发步骤

### 4.1. 创建工程、引入库

创建.NET WPF模板解决方案（.Net Framework模板也行）`WpfFluentValidation`，引入Nuget包FluentValidation(属性验证使用)和`Prism.Wpf`(简化MVVM和命令使用)：

```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" Version="11.9.0" />
  <PackageReference Include="Prism.Wpf" Version="9.0.271-pre" />
</ItemGroup>
```

### 4.2. 创建测试实体类

#### 4.2.1. 普通类 - Student

此类用作ViewModel中的对象属性使用，学生类包含3个属性：名字、年龄、邮政编码。此实体需要继承IDataErrorInfo接口，用于触发FluentValidation验证使用。

```C#
/// <summary>
///     学生实体
///     继承BindableBase,即继承属性变化接口INotifyPropertyChanged
///     实现IDataErrorInfo接口，用于FluentValidation验证，必须实现此接口
/// </summary>
public class Student : BindableBase, IDataErrorInfo
{
    private int _age;
    private string? _name;
    private string? _zip;
    private readonly StudentValidator _validator = new();

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public string? Zip
    {
        get => _zip;
        set => SetProperty(ref _zip, value);
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
```

上面关键代码在`public string this[string columnName]`：这里进行输入表单项的数据校验，`FluentValidation`调用就在这里，校验逻辑封装在`StudentValidator`，表单输入时会实时调用该处代码，`columnName`表示表单项的列名，就是ViewModel绑定的属性名。

#### 4.2.2. 集合类 - Field

此类用作ViewModel中的集合项使用，模拟动态表单数据校验，简单包含4个属性：字段名称、字段显示名称、数据类型、数据值，表单主要根据数据类型验证输入的数据值是否合法。同样此实体需要继承IDataErrorInfo接口，用于触发FluentValidation验证使用。

```csharp
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
```

看上面代码，`public string this[string columnName]`代码处写法和`Student`类一样，只是`_validator`变量类型不同，前者为`StudentValidator`，这里是`FieldValidator`，下面我们看看这两个类怎么写。

### 4.3. 创建验证器

验证属性的写法有两种：

1. 可以在实体属性上方添加特性（本文不作特别说明，百度文章介绍很多）；
2. 通过代码的形式添加，如下方，创建一个验证器类，继承自AbstractValidator，在此验证器构造函数中写规则验证属性，方便管理。

本文使用第二种，通过创建`StudentValidator`和`FieldValidator`两个验证器类介绍。

#### 4.3.1. StudentValidator

这是学生验证器`StudentValidator`，需要继承`AbstractValidator`，泛型指定前面需要验证的实体类`Student`：

```C#
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
```

代码简单，使用到数字的大小和范围验证（见Age）、字符串不能为空和长度限制（见Name）、字符串正则表达式验证（见Zip）。

#### 4.3.2. FieldValidator

动态表单数据值校验器，同理需要继承`AbstractValidator`，泛型指定前面需要验证的实体类`Field`：：

```csharp
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
```

这里写的简单了点：

1. 文本数据类型，值不能为空；
2. 数字数据类型，必须是`double`类型；
3. 日期类型，必须能使用`DateTime`转换；

本文只做简单演示，可按实际情况修改。

#### 4.3.3. StudentViewModelValidator

这个验证器用于验证ViewModel（后面会写）：

```csharp
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
```

1. `Title`用于关联验证基本数据类型(string类型)；
2. `CurrentStudent`用于验证对象属性（Student类的实例），设置验证该属性时使用`StudentValidator`验证器；
3. `Fields`用于验证集合属性(`ObservableCollection<Field>`)，设置验证该属性子项时使用`FieldValidator`验证器，注意前面使用的`RuleForEach`表示关联集合中的项验证器。

### 4.4. ViewModel - StudentViewModel

`StudentViewModel`与`Student`实体类结构类似，都需要实现`IDataErrorInfo`接口，该类由一个简单的`string`属性(`Title`)和一个复杂的`Student`对象属性(`CurrentStudent`)、集合属性`ObservableCollection<Field> Fields`组成，代码如下：

```C#
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
        Fields.Add(new Field(DataType.Text, "文本，比如：四川省成都市场", "地址", ""));
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
```

`ViewModel`属性验证和`Student`及`Field`类似，这里我加上了保存(`SaveCommand`)和取消(`CancelCommand`)两个命令，其中保存命令需要所有属性验证通过才可用，通过注册属性的变化事件`PropertyChanged`，在变化事件处理程序中验证：

```csharp
PropertyChanged += Validate;
CurrentStudent.PropertyChanged += Validate;
foreach (var field in Fields)
{
    field.PropertyChanged += Validate;
}
```

```csharp
private void Validate(object sender, PropertyChangedEventArgs e)
{
    _isCanExecuteSaveCommand = _validator.Validate(this).IsValid;
    SaveCommand.RaiseCanExecuteChanged();
}
```

### 4.5. 视图StudentView

用户直接接触的视图文件来了，比较简单，提供简单属性标题(Title)、复杂属性(包括学生姓名(CurrentStudent.Name)、学生年龄( CurrentStudent .Age)、学生邮政编码( CurrentStudent .Zip)）验证、集合属性验证，xaml代码如下：

```xml
<UserControl
    x:Class="WpfFluentValidation.Views.StudentView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:models="clr-namespace:WpfFluentValidation.Models"
    xmlns:vm="clr-namespace:WpfFluentValidation.ViewModels"
    mc:Ignorable="d" Padding="10">
    <UserControl.DataContext>
        <vm:StudentViewModel />
    </UserControl.DataContext>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="50" />
        </Grid.RowDefinitions>
        <ScrollViewer HorizontalScrollBarVisibility="Hidden" VerticalScrollBarVisibility="Auto">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                </Grid.RowDefinitions>

                <GroupBox Header="ViewModel直接属性验证">
                    <StackPanel>
                        <Label Content="标题：" />
                        <TextBox Style="{StaticResource Styles.TextBox.ErrorStyle1}"
                                 Text="{Binding Title, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}" />
                    </StackPanel>
                </GroupBox>

                <GroupBox Grid.Row="1" Header="ViewModel对象属性CurrentStudent的属性验证">
                    <StackPanel>
                        <StackPanel>
                            <Label Content="姓名：" />
                            <TextBox Style="{StaticResource Styles.TextBox.ErrorStyle2}"
                                     Text="{Binding CurrentStudent.Name, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}" />
                        </StackPanel>
                        <StackPanel>
                            <Label Content="年龄：" />
                            <TextBox Style="{StaticResource Styles.TextBox.ErrorStyle2}"
                                     Text="{Binding CurrentStudent.Age, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}" />
                        </StackPanel>
                        <StackPanel>
                            <Label Content="邮编：" />
                            <TextBox Style="{StaticResource Styles.TextBox.ErrorStyle2}"
                                     Text="{Binding CurrentStudent.Zip, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}" />
                        </StackPanel>
                    </StackPanel>
                </GroupBox>

                <GroupBox Grid.Row="2" Header="ViewModel集合属性Fields的属性验证">
                    <ItemsControl ItemsSource="{Binding Fields}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate DataType="{x:Type models:Field}">
                                <Border Padding="10">
                                    <Grid>
                                        <Grid.RowDefinitions>
                                            <RowDefinition Height="Auto" />
                                            <RowDefinition Height="Auto" />
                                        </Grid.RowDefinitions>
                                        <TextBlock Margin="0,0,0,5">
                                            <Run Text="{Binding Name}" />
                                            <Run Text="(" />
                                            <Run Text="{Binding TypeLabel}" />
                                            <Run Text=")" />
                                        </TextBlock>
                                        <TextBox Grid.Row="1" Style="{StaticResource Styles.TextBox.ErrorStyle2}"
                                                 Text="{Binding Value, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}" />
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </GroupBox>
            </Grid>
        </ScrollViewer>

        <StackPanel Grid.Row="1" HorizontalAlignment="Right" Orientation="Horizontal">
            <Button Content="取消" Command="{Binding CancelCommand}" Style="{StaticResource Styles.Button.Common}"
                    Margin="0 3 40 3" />
            <Button Content="提交" Command="{Binding SaveCommand}" Style="{StaticResource Styles.Button.Blue}"
                    Margin="0 3 10 3" />
        </StackPanel>
    </Grid>
</UserControl>
```

8. 错误提示样式

本文提供了两种样式，具体效果见前面的截图，代码如下：

```xml
<Application
    x:Class="WpfFluentValidation.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    StartupUri="MainWindow.xaml">
    <Application.Resources>
        <Style TargetType="StackPanel">
            <Setter Property="Margin" Value="0,5" />
        </Style>
        <!--  第一种错误样式，红色边框  -->
        <Style x:Key="Styles.TextBox.ErrorStyle1" TargetType="{x:Type TextBox}">
            <Setter Property="Width" Value="250" />
            <Setter Property="Height" Value="25" />
            <Setter Property="HorizontalAlignment" Value="Left" />
            <Setter Property="Validation.ErrorTemplate">
                <Setter.Value>
                    <ControlTemplate>
                        <DockPanel>
                            <Grid
                                Width="16"
                                Height="16"
                                Margin="3,0,0,0"
                                VerticalAlignment="Center"
                                DockPanel.Dock="Right">
                                <Ellipse
                                    Width="16"
                                    Height="16"
                                    Fill="Red" />
                                <Ellipse
                                    Width="3"
                                    Height="8"
                                    Margin="0,2,0,0"
                                    HorizontalAlignment="Center"
                                    VerticalAlignment="Top"
                                    Fill="White" />
                                <Ellipse
                                    Width="2"
                                    Height="2"
                                    Margin="0,0,0,2"
                                    HorizontalAlignment="Center"
                                    VerticalAlignment="Bottom"
                                    Fill="White" />
                            </Grid>
                            <Border
                                BorderBrush="Red"
                                BorderThickness="2"
                                CornerRadius="2">
                                <AdornedElementPlaceholder />
                            </Border>
                        </DockPanel>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="Validation.HasError" Value="true">
                    <Setter Property="ToolTip"
                            Value="{Binding RelativeSource={x:Static RelativeSource.Self}, Path=(Validation.Errors)[0].ErrorContent}" />
                </Trigger>
            </Style.Triggers>
        </Style>

        <!--  第二种错误样式，右键文字提示  -->
        <Style x:Key="Styles.TextBox.ErrorStyle2" TargetType="{x:Type TextBox}">
            <Setter Property="Width" Value="250" />
            <Setter Property="Height" Value="25" />
            <Setter Property="VerticalContentAlignment" Value="Center" />
            <Setter Property="Padding" Value="5,0" />
            <Setter Property="HorizontalAlignment" Value="Left" />
            <Setter Property="Validation.ErrorTemplate">
                <Setter.Value>
                    <ControlTemplate>
                        <StackPanel Orientation="Horizontal">
                            <AdornedElementPlaceholder x:Name="textBox" />
                            <Grid>
                                <TextBlock Margin="10 0 0 0" Width="130"
                                           Foreground="Red" TextWrapping="Wrap"
                                           Text="{Binding [0].ErrorContent}" />
                            </Grid>
                        </StackPanel>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="Validation.HasError" Value="true">
                    <Setter Property="ToolTip"
                            Value="{Binding RelativeSource={x:Static RelativeSource.Self}, Path=(Validation.Errors)[0].ErrorContent}" />
                    <Setter Property="Background" Value="LightPink" />
                    <Setter Property="BorderBrush" Value="Red" />
                    <Setter Property="Foreground" Value="White" />
                </Trigger>
            </Style.Triggers>
        </Style>

        <Style TargetType="GroupBox">
            <Setter Property="Margin" Value="5" />
            <Setter Property="Padding" Value="2" />
            <Setter Property="BorderBrush" Value="#FF0078D7" />
            <Setter Property="BorderThickness" Value="2" />
            <Setter Property="Background" Value="#FFF0F0F0" />
            <Setter Property="Foreground" Value="#FF0078D7" />
            <Setter Property="FontWeight" Value="Bold" />
        </Style>

        <Style x:Key="Styles.Button.Common" TargetType="{x:Type Button}">
            <Setter Property="MinWidth" Value="75" />
            <Setter Property="MinHeight" Value="25" />
            <Setter Property="Background" Value="White" />
            <Setter Property="Foreground" Value="Black" />
        </Style>

        <Style
            x:Key="Styles.Button.Blue"
            BasedOn="{StaticResource ResourceKey=Styles.Button.Common}"
            TargetType="{x:Type Button}">
            <Setter Property="Background" Value="Green" />
            <Setter Property="Foreground" Value="White" />
        </Style>
    </Application.Resources>
</Application>
```

## 5. 介绍完毕

代码贴完，我们看看效果：

![](https://img1.dotnet9.com/2019/11/0103.gif)

## 6. 源码同步

本文代码已同步：

- gitee: https://gitee.com/dotnet9/FluentValidationForWpf
- github： https://github.com/dotnet9/FluentValidationForWPF