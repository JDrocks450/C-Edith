using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CEdith.Frontend.Controls
{
    /// <summary>
    /// Interaction logic for CodeObjectControl.xaml
    /// </summary>
    public partial class CodeObjectControl : UserControl
    {
        private bool _entryPoint;
        private bool _falseenabled;

        public bool IsEntryPoint
        {
            get
            {
                return _entryPoint;
            }
            set
            {
                _entryPoint = value;
                InvalidateMe();
            }
        }

        public CodeObjectControl? TrueNode { get; set; }
        public bool HasTrue => TrueNode != null;
        public CodeObjectControl? FalseNode { get; set; }
        public bool HasFalse => FalseNode != null;
        public bool FalseEligible { 
            get => _falseenabled; 
            set
            {
                _falseenabled = value;
                InvalidateMe();
            }
        }

        public CodeObjectControl()
        {
            InitializeComponent();
            InvalidateMe();
        }

        public void InvalidateMe()
        {
            entryPointRect.BorderThickness = new Thickness(0, 0, 0, 0);
            if (IsEntryPoint)            
                entryPointRect.BorderThickness = new Thickness(4);            
            FalseButton.Content = FalseEligible ? "F" : "";
        }

        internal static CodeObjectControl Create(string Title, string Description)
        {
            var control = new CodeObjectControl();
            control.TitleBlock.Text = Title;
            control.DescriptionBlock.Text = Description;
            return control;            
        }

        internal void SetPrimaryContent(object Text)
        {
            TrueButton.Content = Text;
        }
    }
}
