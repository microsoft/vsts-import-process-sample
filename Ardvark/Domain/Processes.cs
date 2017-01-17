using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Aardvark.ViewModels;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Formatting;
using Newtonsoft.Json;

namespace Aardvark.Domain
{
    public class Processes
    {        
        private IAppConfig _appConfig;
        private string _apiurl;
        private string _login;

        public Processes(IAppConfig appConfig)
        {
            _appConfig = appConfig;

            _apiurl = _appConfig.ReadSetting("account");
            _login = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", _appConfig.ReadSetting("login"), _appConfig.ReadSetting("password"))));
        }

        public StandardResponseViewModel Export(string processId, string path, string name)
        {
            //processId = ADCC42AB-9882-485E-A3ED-7678F01F66BC

            Byte[] bytes = this.GetProcessExportDataRESTCall(processId);
            StandardResponseViewModel response = new StandardResponseViewModel();

            if (bytes != null)
            {
                //Byte[] bytes = Convert.FromBase64String(vm.data);

                File.WriteAllBytes(@path + @"\" + name + ".zip", bytes);

                response.Success = true;
                response.Message = name + ".zip file was successfull created";
            }
            else
            {
                response.Success = false;
                response.Message = "error getting data from rest api for '" + processId + "'";
            }

            return response;
        }

        private Byte[] GetProcessExportDataRESTCall(string processId)
        {
            Byte[] bytes = null; 

            using (var client = new HttpClient())
            {
                // New code:
                client.BaseAddress = new Uri(_apiurl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _login);

                HttpResponseMessage response = client.GetAsync("_apis/work/processAdmin/processes/export/" + processId + "?api-version=2.2-preview").Result;

                if (response.IsSuccessStatusCode)
                {
                    bytes = response.Content.ReadAsByteArrayAsync().Result; //ReadAsAsync<ExportProcessViewModel>().Result; 
                }

                response.Dispose();

                return bytes;           
            }
        }
                
        public ProcessesListViewModel GetListOfProcessessRESTCall()
        {
            ProcessesListViewModel vm = null;

            using (var client = new HttpClient())
            {
                // New code:
                client.BaseAddress = new Uri(_apiurl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _login);

                HttpResponseMessage response = client.GetAsync("_apis/process/processes?api-version=1.0").Result;

                if (response.IsSuccessStatusCode)
                {
                    vm = response.Content.ReadAsAsync<ProcessesListViewModel>().Result;
                }

                response.Dispose();

                return vm;
            }
        }

        public ImportViewModel ImportSingleProcessRESTCall(string zipPath)
        {
            ImportViewModel importViewModel = new ImportViewModel();

            if (! File.Exists(zipPath))
            {
                importViewModel.Success = false;
                importViewModel.Message = "Import Failed: zip file '" + zipPath + "' not found";

                return importViewModel;        
            }

            Byte[] bytes = File.ReadAllBytes(zipPath);
            ImportResponseViewModel result;

            using (var client = new HttpClient())
            {                
                client.BaseAddress = new Uri(_apiurl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/zip"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _login);

                ByteArrayContent content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                HttpResponseMessage response = client.PostAsync("_apis/work/processAdmin/processes/import?ignoreWarnings=true&api-version=2.2-preview", content).Result;

                result = response.Content.ReadAsAsync<ImportResponseViewModel>().Result;

                if (response.IsSuccessStatusCode)
                {
                    importViewModel.ImportResponseViewModel = result;
                    importViewModel.Success = true;
                    importViewModel.Message = "Import succeeded for '" + zipPath + "'";                                       
                }
                else
                {
                    importViewModel.ImportResponseViewModel = null;
                    importViewModel.Success = false;
                    importViewModel.Message = response.ReasonPhrase;
                    importViewModel.validationResults = result.validationResults;
                }               
                               
                bytes = null;               
                result = null;
                response.Dispose();

                return importViewModel;
            }            
        }

        public StandardResponseViewModel BugsBehaviorRESTCall(string project, TeamSettings vm)
        {
            var response = new StandardResponseViewModel();

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_apiurl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _login);

                var patchValue = new StringContent(JsonConvert.SerializeObject(vm), Encoding.UTF8, "application/json");

                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, _apiurl + project + "/_apis/work/teamsettings?api-version=2.0-preview.1") { Content = patchValue, };
                var result = client.SendAsync(request).Result;
                              
                if (result.IsSuccessStatusCode)
                {
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Error during rest call: " + result.ReasonPhrase;
                }

                return response;
            }
        }
    }
}
