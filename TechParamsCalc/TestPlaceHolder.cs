using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TechParamsCalc
{
    public class TestPlaceHolder : DependencyObject
    {

        public static readonly DependencyProperty ButtonWidthProperty;
        public static readonly DependencyProperty GridWidthProperty;
        public static readonly DependencyProperty LabelOpacityProperty;

        static TestPlaceHolder()
        {
            ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(double), typeof(TestPlaceHolder));
            GridWidthProperty = DependencyProperty.Register("GridWidth", typeof(double), typeof(TestPlaceHolder));
            LabelOpacityProperty = DependencyProperty.Register("LabelOpacity", typeof(double), typeof(TestPlaceHolder));

        }
       
        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }

        public double GridWidth
        {
            get { return (double)GetValue(GridWidthProperty); }
            set { SetValue(GridWidthProperty, value); }
        }
        public double LabelOpacity
        {
            get { return (double)GetValue(LabelOpacityProperty); }
            set { SetValue(LabelOpacityProperty, value); }
        }

    }
}
