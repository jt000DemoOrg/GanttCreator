using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GanttCreator.AdoModels;
using Mechavian.GanttControls.Models;
using Mechavian.WpfHelpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GanttCreator
{
    public class MainWindowViewModel : ViewModel
    {
        private GanttDescriptor ganttDescriptor;
        public DelegateCommand OpenFileCommand { get; private set; }
        public DelegateCommand ExportFileCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }

        public GanttDescriptor GanttDescriptor
        {
            get => this.ganttDescriptor;
            set
            {
                if (this.ganttDescriptor != value)
                {
                    this.ganttDescriptor = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            this.OpenFileCommand = new DelegateCommand(this.OnOpenFile);
            this.ExportFileCommand = new DelegateCommand<FrameworkElement>(OnExportFile);
            this.ExitCommand = new DelegateCommand(OnExit);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (!string.IsNullOrEmpty(UserSettings.Default.LastOpenFile))
            {
                OpenGanttFile(UserSettings.Default.LastOpenFile).ContinueWith((task) =>
                {
                    if (!task.Result)
                    {
                        UserSettings.Default.LastOpenFile = string.Empty;
                        UserSettings.Default.Save();
                    }
                });
            }
        }

        private void OnExportFile(FrameworkElement frameworkElement)
        {
            var sfd = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "png",
                RestoreDirectory = true,
                OverwritePrompt = true,
                Filter = "png files (*.png)|*.png|All files (*.*)|*.*",
                Title = "Save File"
            };

            if (sfd.ShowDialog(ParentWindow).GetValueOrDefault(false))
            {
                try
                {
                    var targetWidth = (int)frameworkElement.ActualWidth;
                    var targetHeight = (int)frameworkElement.ActualHeight;

                    // Exit if there's no 'area' to render
                    if (targetWidth == 0 || targetHeight == 0)
                        throw new Exception("Framework Element has no area");

                    // Prepare the rendering target
                    var targetBitmap = new RenderTargetBitmap(targetWidth, targetHeight, 96, 96, PixelFormats.Pbgra32);

                    // Render the framework element into the target
                    targetBitmap.Render(frameworkElement);

                    var bitmapFrame = BitmapFrame.Create(targetBitmap);
                    var bitmapEncoder = new PngBitmapEncoder();
                    bitmapEncoder.Frames.Add(bitmapFrame);

                    using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        bitmapEncoder.Save(fileStream);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(ParentWindow, e.Message, "Save File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task<GanttDescriptor> LoadGanttDescriptor(GanttFile ganttFile)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(ganttFile.AzureDevOpsUri),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ganttFile.User}:{ganttFile.PersonalAccessToken}"))),
                    UserAgent = { new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString()) },
                    Accept = { new MediaTypeWithQualityHeaderValue("*/*") },
                    Connection = { "keep-alive" }
                }
            };
            var response = await httpClient.GetAsync($"{ganttFile.Organization}/{ganttFile.Project}/{ganttFile.Team}/_apis/work/teamsettings/iterations");
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var iterations = JObject.Parse(json).ToObject<TeamIterationCollection>() ?? new TeamIterationCollection();
            return new GanttDescriptor()
            {
                Ranges = iterations.Value.Select((i) => new GanttRange() { Name = i.Name }).ToArray(),
                Work = Array.Empty<GanttWork>()
            };
        }

        private void OnExit(object obj)
        {
            this.ParentWindow?.Close();
        }

        private void OnOpenFile(object obj)
        {
            var ofd = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "json",
                RestoreDirectory = true,
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
                Multiselect = false,
                Title = "Open File"
            };

            if (ofd.ShowDialog(ParentWindow).GetValueOrDefault(false))
            {
                var fileName = ofd.FileName;
                this.OpenGanttFile(fileName).ContinueWith((task) =>
                {
                    if (task.Result)
                    {
                        UserSettings.Default.LastOpenFile = fileName;
                        UserSettings.Default.Save();
                    }
                });
            }
        }

        private async Task<bool> OpenGanttFile(string fileName)
        {
            try
            {
                var loadSettings = new JsonLoadSettings()
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
                };
                JsonSerializer jsonSerializer = new JsonSerializer()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var ganttFile = JObject.Parse(File.ReadAllText(fileName), loadSettings).ToObject<GanttFile>(jsonSerializer);
                var descriptor = await LoadGanttDescriptor(ganttFile);

                Dispatcher.Invoke(() => GanttDescriptor = descriptor);
                return true;
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() => MessageBox.Show(this.ParentWindow, e.Message, "Open File Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return false;
            }
        }
    }
}