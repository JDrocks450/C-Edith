using CEdith.Frontend.Controls;
using CEdith.SharpAntics.Code.Definitions;
using ICSharpCode.NRefactory.CSharp;
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

namespace CEdith.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for CodePage.xaml
    /// </summary>
    public partial class CodePage : Page
    {
        const double Y_STEP = 150, X_STEP = 150, NODE_WIDTH = 250;
        double CurrentX = 0, CurrentY = 0;

        TypeMemberDefinition OpenDefinition;

        Dictionary<AstNode, CodeObjectControl> controlMap = new();

        public CodePage()
        {
            InitializeComponent();
        }

        public void OpenBlock(string Name, BlockStatement Block, params ParameterDeclaration[] Args)
        {
            OpenBlock(Name, Block, Args);
        }

        public void OpenBlock(string Name, IEnumerable<Statement> Block, params ParameterDeclaration[] Args)
        {
            var tempDef = new TypeMemberDefinition(TypeMemberDefinition.TypeMemberSymbols.Method,
                Name, "object", Block, Args);
            OpenBlock(tempDef);
        }

        public void OpenBlock(TypeMemberDefinition Definition)
        {
            NodeCanvas = new WpfPanAndZoom.CustomControls.PanAndZoomCanvas()
            {
                Background = Brushes.Gainsboro,
                LineColor = Colors.Silver
            };
            PanZoomGrid.Children.Clear();
            PanZoomGrid.Children.Add(NodeCanvas);
            bool first = false;
            bool ProcessNode(AstNode node)
            {
                if (first) EmplaceInLineLabel("Entrypoint", Brushes.Green);
                var controls = GetNode(node);
                if (controls == null) return false;
                foreach (var tuple in controls)
                {
                    NodeCanvas.Children.Add(tuple.Visual);
                    if (!controlMap.ContainsKey(tuple.Origin))
                        controlMap.Add(tuple.Origin, tuple.Visual);
                    if (first)
                    {
                        tuple.Visual.IsEntryPoint = true;
                        first = false;
                    }
                }
                CurrentY += Y_STEP;
                return true;
            }
            var Parameters = Definition.Parameters;
            var Block = Definition.Block;
            var Name = Definition.Name;
            OpenDefinition = Definition;
            SourceLabel.Content = Name;
            controlMap.Clear();
            CurrentX = 0;
            CurrentY = Y_STEP;
            CurrentX = NodeCanvas.ActualWidth / 2;
            CurrentX += NODE_WIDTH / 2;
            CurrentX += 50;
            if (Parameters.Any())
                EmplaceInLineLabel("Parameters", Brushes.DarkBlue);
            foreach (var node in Parameters)
            {
                if (!ProcessNode(node)) continue;
                ToolBar.Children.Insert(1,
                    new Button()
                    {
                        Background = Brushes.LightBlue,
                        Content = $"{node.Type} {node.Name}",
                        Padding = new Thickness(2, 2, 2, 2)
                    });
            }
            ToolBar.Children.Insert(0,
                new Button()
                {
                    Background = Brushes.LightCoral,
                    Content = $"{Definition.ReturnType}",
                    Padding = new Thickness(2, 2, 2, 2)
                }
                );
            first = true;
            if (Block != null)
                foreach (var node in Block)
                    if (!ProcessNode(node)) continue;
            if (NodeCanvas.Children.Count == 0)
            {
                NodeCanvas.Children.Add(new TextBlock()
                {
                    Text = "<Empty>"
                });
                return;
            }
            if (NodeCanvas.Height < CurrentY + (Y_STEP * 4))
                NodeCanvas.Height = CurrentY + (Y_STEP * 4);
            Dispatcher.InvokeAsync(PostDraw, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void EmplaceInLineLabel(string v, Brush Foreground)
        {
            var block = new TextBlock()
            {
                Text = v,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Foreground = Foreground
            };
            NodeCanvas.Children.Add(block);
            Canvas.SetLeft(block, CurrentX - 135);
            Canvas.SetTop(block, CurrentY + 25);
        }

        private void TransformAll(Transform Transform)
        {
            foreach (UIElement child in NodeCanvas.Children)
                child.RenderTransform = Transform;
        }

        private void PostDraw()
        {
            MapNodeLogicPath();
            DrawLines();
        }

        private void MapNodeLogicPath()
        {
            int index = -1;
            foreach(var island in controlMap)
            {
                index++;
                if (!island.Value.HasTrue) // FIND TRUE
                {
                    var nextControl = controlMap.ElementAtOrDefault(index + 1).Value;
                    if (nextControl != default)
                        island.Value.TrueNode = nextControl;
                }
                if (island.Value.FalseEligible && !island.Value.HasFalse)
                {
                    var nextControl = controlMap.ElementAtOrDefault(index + 2).Value;
                    if (nextControl != default)
                        island.Value.FalseNode = nextControl;
                }
            }
        }

        private void DrawLines()
        {
            void DrawLine(CodeObjectControl one, CodeObjectControl two, bool trueLine)
            {
                Point startPos = (trueLine ? one.TrueButton : one.FalseButton).TranslatePoint(new Point(60, 11), NodeCanvas);
                var endPoint = two.TranslatePoint(new Point(125, 50), NodeCanvas);
                Vector delta = startPos - endPoint;
                delta = new Vector(Math.Abs(delta.X), delta.Y);
                if (delta.X < 0)
                    delta.X = 0;
                if (delta.X > 200)
                    delta.X = 200;
                if (delta.Y < 0)
                    delta.Y = 0;
                if (delta.Y > 100)
                    delta.Y = 100;
                endPoint = two.TranslatePoint(new Point(125, delta.Y), NodeCanvas);
                NodeCanvas.Children.Add(new Line()
                {
                    X1 = startPos.X,
                    Y1 = startPos.Y,
                    X2 = endPoint.X,
                    Y2 = endPoint.Y,
                    StrokeThickness = 1,
                    Stroke = Brushes.Black
                });
            }
            foreach(var control in controlMap.Values)
            {
                if (control.HasTrue)                
                    DrawLine(control, control.TrueNode, true);
                if (control.HasFalse)
                    DrawLine(control, control.FalseNode, false);
            }
        }

        internal void Reload()
        {
            OpenBlock(OpenDefinition);
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> GetNode(AstNode Node)
        {
            if (Node is SimpleType or Identifier or PreProcessorDirective or MemberType or NewLineNode or CSharpTokenNode)
                return null;
            if (Node is ParameterDeclaration parameter)
                return AddParameter(parameter);
            else if (Node is Statement)
            {
                if (Node is BlockStatement block)
                    return AddBlock(block);
                if (Node is IfElseStatement ifelse)
                    return AddIfElse(ifelse);
                if (Node is ReturnStatement returnStatement)
                    return AddReturn(returnStatement);
                if (Node is VariableDeclarationStatement variableDecl)
                    return AddVariable(variableDecl);
                if (Node is ExpressionStatement expressionDecl)
                    return AddExpressionStatement(expressionDecl);
                if (Node is WhileStatement whileStatement)
                    return AddWhileStatement(whileStatement);
                if (Node is ForeachStatement foreachStatement)
                    return AddForeachStatement(foreachStatement);
                if (Node is ForStatement forStatement)
                    return AddForLoop(forStatement);
                if (Node is SwitchStatement switchStatement)
                    return AddSwitchStatement(switchStatement);
            }
            var control = CodeObjectControl.Create(Node.GetType().Name, "");
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            return new[] { (Node, control) };
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddSwitchStatement(SwitchStatement switchStatement)
        {
            var tuple = ParseExpressionInfo(switchStatement.Expression);
            var control = CodeObjectControl.Create("Switch:", $"{tuple.title} {tuple.message}");
            control.SetPrimaryContent($"-> {switchStatement.SwitchSections.Count} switch cases");
            control.Background = Brushes.PaleVioletRed;
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            CurrentY += Y_STEP;
            CurrentX += 150;
            List<(AstNode, CodeObjectControl)> controls = new()
            {
                (switchStatement, control)
            };
            foreach (var section in switchStatement.SwitchSections)
            {
                var name = string.Join(", ", section.CaseLabels.Select(x => x.Expression));
                if (string.IsNullOrWhiteSpace(name)) name = "default";
                var caseControl = CodeObjectControl.Create("Case:", name);                
                caseControl.SetPrimaryContent("-> next case");
                Canvas.SetTop(caseControl, CurrentY);
                Canvas.SetLeft(caseControl, CurrentX);
                controls.Add((section, caseControl));
                CurrentY += control.Height;
                CreateNestedPageAt(caseControl, section.Statements, $"switch case: {name}");
                CurrentY += Y_STEP;
            }
            CurrentX -= 150;
            return controls;
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddForLoop(ForStatement foreachStatement)
        {
            var control = CodeObjectControl.Create("For:", $"{foreachStatement.Initializers.First()}");
            control.SetPrimaryContent("-> for loop");
            control.Background = Brushes.Orange;
            control.Foreground = Brushes.White;
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            CurrentY += control.Height;
            if (foreachStatement.EmbeddedStatement is BlockStatement block)
                CreateNestedPageAt(control, block, "For Loop Body");
            return new[] { ((AstNode)foreachStatement, control) };
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddForeachStatement(ForeachStatement foreachStatement)
        {
            var control = CodeObjectControl.Create("Foreach:", $"{foreachStatement.VariableType} {foreachStatement.VariableName} in {foreachStatement.InExpression}");
            control.SetPrimaryContent("-> foreach loop");
            control.Background = Brushes.PaleGreen;
            control.Foreground = Brushes.Black;
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            CurrentY += control.Height;
            if (foreachStatement.EmbeddedStatement is BlockStatement block)
                CreateNestedPageAt(control, block, "Foreach Statement Body");
            return new[] { ((AstNode)foreachStatement, control) };
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddWhileStatement(WhileStatement whileStatement)
        {
            var control = CodeObjectControl.Create("While:", $"{whileStatement.Condition}");
            control.SetPrimaryContent("-> while loop");
            control.Background = Brushes.DarkTurquoise;
            control.Foreground = Brushes.White;
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            CurrentY += control.Height;
            if (whileStatement.EmbeddedStatement is BlockStatement block)
                CreateNestedPageAt(control, block, "While Body");
            return new[] { ((AstNode)whileStatement, control) };
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddBlock(BlockStatement block)
        {
            var control = CodeObjectControl.Create("Subroutine: ", $"Flow of control changes to this block until it completes.");
            control.SetPrimaryContent("-> scope");
            control.Background = Brushes.DarkTurquoise;
            control.Foreground = Brushes.White;
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            CurrentY += control.Height;
            CreateNestedPageAt(control, block, "sub: " + OpenDefinition.Name);
            return new[] { ((AstNode)block, control) };
        }

        private void CreateNestedPageAt(CodeObjectControl control, IEnumerable<Statement> block, string Name = "Nested Block")
        {
            var nestedCodePage = new CodePage();
            nestedCodePage.OpenBlock(Name, block);
            nestedCodePage.NodeCanvas.Background = 
                new SolidColorBrush(
                ((SolidColorBrush)control.Background).Color * .5f);           
            nestedCodePage.NodeCanvas.LineColor = Colors.Transparent;
            nestedCodePage.NodeCanvas.ZoomInverseOnce();
            Border parent = new Border()
            {
                BorderThickness = new Thickness(2),
                BorderBrush = control.Background,
                Width = 700,                
                Height = (nestedCodePage.NodeCanvas.Children.OfType<CodeObjectControl>().Count() + 1) * 105,
                Child = new Frame()
                {
                    Content = nestedCodePage
                }
            };
            nestedCodePage.Width = parent.Width;
            nestedCodePage.Height = parent.Height;
            Canvas.SetTop(parent, CurrentY);
            Canvas.SetLeft(parent, (CurrentX + (control.Width / 2)) - (parent.Width / 2));
            CurrentY += parent.Height - 50;
            NodeCanvas.Children.Add(parent);
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddParameter(ParameterDeclaration parameter)
        {            
            var control = CodeObjectControl.Create(
                $"{(parameter.ParameterModifier != ParameterModifier.None ? parameter.ParameterModifier + " " : "")}Parameter:",
                parameter.Type.ToString() + " " + parameter.Name);
            control.SetPrimaryContent(parameter.ParameterModifier != ParameterModifier.None ? parameter.ParameterModifier : "Parameter");
            control.Background = Brushes.LightBlue;
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            return new[] { ((AstNode)parameter, control) };
        }        

        private (string title, string message, string label, Brush color) ParseExpressionInfo(ICSharpCode.NRefactory.CSharp.Expression expression)
        {
            string title = "Do: ", subtext = expression.ToString(), primaryLabel = "then";
            Brush background = Brushes.LightYellow;
            if (expression is MemberReferenceExpression member)
            {
                title = "Reference:";
            }
            if (expression is IdentifierExpression identifier)
            {
                title = "Local:";
            }
            if (expression is AssignmentExpression assignment)
            {
                title = $"{assignment.Operator}:";
            }
            if (expression is InvocationExpression invocation)
            {
                title = "Invoke:";
                subtext = invocation.ToString();
            }
            return (title, subtext, primaryLabel, background);
        }

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddExpressionStatement(ExpressionStatement expressionDecl)
        {
            var tuple = ParseExpressionInfo(expressionDecl.Expression);
            var control = CodeObjectControl.Create(tuple.title, tuple.message);
            control.Background = tuple.color;
            control.SetPrimaryContent(tuple.label);
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            return new[] { ((AstNode)expressionDecl, control) };
        }      

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddVariable(VariableDeclarationStatement variableDecl)
        {
            var control = CodeObjectControl.Create($"Local:", variableDecl.ToString());
            Canvas.SetTop(control, CurrentY);
            Canvas.SetLeft(control, CurrentX);
            return new[] { ((AstNode)variableDecl, control) };
        }        

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddReturn(ReturnStatement returnStatement)
        {
            var tuple = ParseExpressionInfo(returnStatement.Expression);
            var control1 = CodeObjectControl.Create("Return Expression: ", 
                $"{tuple.title} {tuple.message}");
            control1.Background = Brushes.LightCoral;
            control1.SetPrimaryContent("-> return");
            Canvas.SetTop(control1, CurrentY);
            Canvas.SetLeft(control1, CurrentX);
            CurrentY += control1.Height;
            var control2 = CodeObjectControl.Create("Returned: ", $"{tuple.message} ({OpenDefinition.ReturnType})");
            control2.SetPrimaryContent("EXIT");
            Canvas.SetTop(control2, CurrentY);
            Canvas.SetLeft(control2, CurrentX);
            //control1.TrueNode = control2;
            return new[] { ((AstNode)returnStatement, control1), ((AstNode)returnStatement, control2) };
        }        

        private IEnumerable<(AstNode Origin, CodeObjectControl Visual)> AddIfElse(IfElseStatement Statement)
        {
            List<(AstNode Origin, CodeObjectControl Visual)> controls = new();
            var ifstatement = CodeObjectControl.Create("If", Statement.Condition.ToString());
            ifstatement.FalseEligible = true;
            controls.Add((Statement, ifstatement));
            Canvas.SetLeft(ifstatement, CurrentX);            
            Canvas.SetTop(ifstatement, CurrentY);
            CurrentY += Y_STEP;
            CurrentX -= X_STEP;
            if (!Statement.TrueStatement.IsNull)
            {
                var newControls = GetNode(Statement.TrueStatement);
                controls.AddRange(newControls);
                ifstatement.TrueNode = newControls.FirstOrDefault().Visual;
            }         
            CurrentX += X_STEP * 2;
            if (!Statement.FalseStatement.IsNull)
            {
                var newControls = GetNode(Statement.FalseStatement);
                controls.AddRange(newControls);
                ifstatement.FalseNode = newControls.FirstOrDefault().Visual;
            }
            CurrentX -= X_STEP;            
            return controls;
        }

        private void SourceLabel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PopOutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenTabButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.OpenTab(OpenDefinition);
        }
    }
}
