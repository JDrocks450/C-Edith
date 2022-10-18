using CEdith.SharpAntics.Code.Definitions;
using CEdith.SharpAntics.Roslyn;
using ICSharpCode.NRefactory.CSharp;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace CEdith.Frontend.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Dispatcher.InvokeAsync(delegate
            {
                if (!PromptOpenFile())
                {
                    Application.Current.MainWindow.Content = new WelcomePage();
                    return;
                }             
                TabViewer.Items.Clear();
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            Current = this;
        }

        private string currentFilePath;
        private FileSystemWatcher currentWatcher;
        private HashSet<CodePage> Tabs = new();

        public static MainPage Current { get; internal set; }

        internal void OpenTab(string Name, CodePage Page)
        {
            var tab = new TabItem()
            {
                Header = Name,
                Background = Brushes.White,
                Content = new Frame()
                {
                    ClipToBounds = true,
                    Content = Page
                }
            };
            TabViewer.Items.Add(tab);
            Tabs.Add(Page);
            TabViewer.SelectedItem = tab;
        }

        internal void OpenTab(TypeMemberDefinition Definition)
        {
            TypeMemberDefinition function = Definition;
            var codePage = new CodePage();
            codePage.OpenBlock(function);
            OpenTab(Definition.Name, codePage);
        }

        internal void OpenFile(string FilePath)
        {
            if (currentWatcher != null)
            {
                currentWatcher.Dispose();
                currentWatcher = null;
            }
            currentWatcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(FilePath));
            var project = CodeImporter.ImportProject(FilePath);
            currentFilePath = FilePath;
            void AddFunctions()
            {
                var functionsTree = new TreeViewItem()
                {
                    Header = "Macro: Functions",
                    IsExpanded = true
                };
                TreeControl.Items.Add(functionsTree);
                foreach (var namespec in project.Namespaces)
                {
                    if (project.TryGetNamespaceTypes(namespec, out var content))
                    {
                        foreach (var type in content)
                        {
                            var classTree = new TreeViewItem()
                            {
                                Header = type.Description,
                                IsExpanded = true
                            };
                            functionsTree.Items.Add(classTree);
                            if (project.TryGetTypeMemberContents(type, out var functions))
                                foreach (var function in functions)
                                {
                                    var functionTree = new TreeViewItem()
                                    {
                                        Header = function.Name,

                                    };
                                    functionTree.Selected += delegate
                                    {
                                        FunctionSelected(function);
                                    };
                                    classTree.Items.Add(functionTree);
                                }
                        }
                    }
                }
            }
            TreeControl.Items.Clear();
            void AddNode(CodeObjectTreeNode Node, in TreeViewItem CurrentNode)
            {
                var AddedNode = new TreeViewItem() { Header = Node.ToString(), ToolTip = Node.CSharpCode, Opacity = Node.Hidden ? .5 : 1 };
                if (Node.BaseDefinition != null)
                {
                    if (Node.BaseDefinition.SpecialToken is SharpAntics.Code.CSharpSpecialTokens.BLOCK)
                    {
                        AddedNode.Tag = ((CodeBlockDefinition)Node.BaseDefinition).Reference;
                        AddedNode.Selected += BlockSelected;
                    }
                }
                CurrentNode.Items.Add(AddedNode);
                foreach (var child in Node.Children)
                    AddNode(child, AddedNode);
            }
            var parentNode = new TreeViewItem() { Header = System.IO.Path.GetFileName(project.Path) };
            TreeControl.Items.Add(parentNode);
            AddNode(project.Root, parentNode);
            AddFunctions();
            currentWatcher.Changed += delegate (object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == currentFilePath)
                    if (MessageBox.Show("The current file has been modified outside of this environment, would you" +
                        " like to refresh all codemaps to reflect new changes?",
                        "File Changed Externally", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        ReloadAll();
            };
        }

        private void ReloadAll()
        {
            OpenFile(currentFilePath);
            foreach (var page in Tabs)
                page.Reload();
        }

        private void FunctionSelected(TypeMemberDefinition function)
        {
            OpenTab(function);
        }

        private void BlockSelected(object sender, RoutedEventArgs e)
        {
            var block = (BlockStatement)((sender as FrameworkElement).Tag);
            var codePage = new CodePage();
            codePage.OpenBlock("Function Body", block);
            OpenTab("Function Body", codePage);
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            PromptOpenFile();
        }

        private bool PromptOpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Open a C# Source File",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                AddExtension = true,
                Filter = "C# Source Files|*.cs"
            };
            if (dialog.ShowDialog() ?? false)
            {
                OpenFile(dialog.FileName);
                return true;
            }
            return false;
        }
    }
}
