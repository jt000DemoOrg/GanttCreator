using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using GanttCreator.IO;
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
        public DelegateCommand SaveAsFileCommand { get; private set; }
        public DelegateCommand LoadFromADOCommand { get; private set; }
        public DelegateCommand ExportToFileCommand { get; private set; }
        public DelegateCommand ExportToClipboardCommand { get; private set; }
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

        public GanttFile GanttFile { get; set; }

        public MainWindowViewModel()
        {
            this.OpenFileCommand = new DelegateCommand(OnOpenFile);
            this.SaveAsFileCommand = new DelegateCommand(OnSaveAsFile);
            this.LoadFromADOCommand = new DelegateCommand(OnLoadFromADO);
            this.ExportToFileCommand = new DelegateCommand<FrameworkElement>(this.OnExportToFile);
            this.ExportToClipboardCommand = new DelegateCommand<FrameworkElement>(this.OnExportToClipboard);
            this.ExitCommand = new DelegateCommand(OnExit);
        }

        private void OnLoadFromADO(object obj)
        {
            LoadGanttDescriptor(GanttFile)
                .ContinueWith(r =>
                {
                    Dispatcher.Invoke(() => GanttDescriptor = r.Result);
                });
        }

        private void OnSaveAsFile(object obj)
        {
            var sfd = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "json",
                RestoreDirectory = true,
                OverwritePrompt = true,
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save File"
            };

            if (!string.IsNullOrEmpty(UserSettings.Default.LastOpenFile))
            {
                sfd.FileName = UserSettings.Default.LastOpenFile;
            }

            if (sfd.ShowDialog(ParentWindow).GetValueOrDefault(false))
            {
                SaveGanttDescriptor();
                File.WriteAllText(sfd.FileName, JObject.FromObject(GanttFile).ToString(Formatting.Indented));
            }
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

        private void OnExportToFile(FrameworkElement frameworkElement)
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
                    var bitmapFrame = GetFrameworkElementAsBitmap(frameworkElement);
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

        private void OnExportToClipboard(FrameworkElement frameworkElement)
        {
            try
            {
                var bitmapFrame = GetFrameworkElementAsBitmap(frameworkElement);
                var bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(bitmapFrame);

                var tempFilePath = Path.ChangeExtension(Path.GetTempFileName(), "png");
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    bitmapEncoder.Save(fileStream);
                }

                Clipboard.SetFileDropList(new StringCollection() { tempFilePath });
            }
            catch (Exception e)
            {
                MessageBox.Show(ParentWindow, e.Message, "Save to Clipboard Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static BitmapFrame GetFrameworkElementAsBitmap(FrameworkElement frameworkElement)
        {
            var targetWidth = (int)Math.Ceiling(frameworkElement.ActualWidth * 96 / 72d);
            var targetHeight = (int)Math.Ceiling(frameworkElement.ActualHeight * 96 / 72d);

            // Exit if there's no 'area' to render
            if (targetWidth == 0 || targetHeight == 0)
                throw new Exception("Framework Element has no area");

            // Prepare the rendering target
            var targetBitmap = new RenderTargetBitmap(targetWidth, targetHeight, 96, 96, PixelFormats.Default);

            // Render the framework element into the target
            targetBitmap.Render(frameworkElement);

            var bitmapFrame = BitmapFrame.Create(targetBitmap);
            return bitmapFrame;
        }

        private async Task<GanttDescriptor> LoadGanttDescriptor(GanttFile ganttFile)
        {
            var rangesTask = GetRanges(ganttFile);
            var workTask = GetWork(ganttFile);
            return new GanttDescriptor()
            {
                Ranges = await rangesTask,
                Work = await workTask
            };
        }
        private void SaveGanttDescriptor()
        {
            if (this.GanttFile == null)
            {
                this.GanttFile = new GanttFile();
            }

            this.GanttFile.FromDescriptor(GanttDescriptor);
        }

        private static async Task<GanttRange[]> GetRanges(GanttFile ganttFile)
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
            return iterations.Value.Select((i) => new GanttRange()
            {
                Name = i.Name,
                StartDate = i.Attributes.StartDate,
                EndDate = i.Attributes.FinishDate
            }).ToArray();
        }

        private static async Task<GanttWork[]> GetWork(GanttFile ganttFile)
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

            var wiql = @"SELECT
    [System.Id],
    [System.Title],
    [System.BoardColumn]
FROM workitems
WHERE
    [System.TeamProject] = @project
    AND [System.WorkItemType] = 'Feature'
    AND [System.AreaPath] UNDER 'OneAgile\PowerApps\Studio\Power Apps Advanced Makers'
    AND [System.State] <> 'Closed'
    AND [System.State] <> 'Cut'
    AND [System.State] <> 'Removed'
ORDER BY [System.BoardColumn] DESC,
    [Microsoft.VSTS.Common.StackRank]";
            var body = new JObject()
            {
                { "query", new JValue(wiql) }
            };
            var response = await httpClient.PostAsync($"{ganttFile.Organization}/{ganttFile.Project}/{ganttFile.Team}/_apis/wit/wiql?timePrecision=false&$top=20&api-version=6.0", 
                new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var wiqlOutput = JObject.Parse(json).ToObject<WiqlOutput>();

            var ganttWork = new List<GanttWork>();
            foreach (var workItem in wiqlOutput.WorkItems)
            {
                ganttWork.Add(await GetWork(httpClient, workItem.Url));
            }

            return ganttWork.ToArray();
        }

        private static async Task<GanttWork> GetWork(HttpClient httpClient, Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Query = "fields=System.Id,System.Title,System.BoardColumn,System.State";
            var jsonResponse = await httpClient.GetStringAsync(uriBuilder.Uri);
            var workItem = JObject.Parse(jsonResponse).ToObject<WorkItem>();

            return new GanttWork()
            {
                Name = workItem.Fields.Title
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
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
                var ganttFile = JObject.Parse(await File.ReadAllTextAsync(fileName), loadSettings).ToObject<GanttFile>(jsonSerializer);
                var descriptor = ganttFile.ToDescriptor();

                Dispatcher.Invoke(() =>
                {
                    this.GanttDescriptor = descriptor;
                    this.GanttFile = ganttFile;
                });
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