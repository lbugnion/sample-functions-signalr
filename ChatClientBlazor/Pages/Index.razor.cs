using ChatClientBlazor.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Data.Common;
using System.Text.Json;

namespace ChatClientBlazor.Pages
{
    public partial class Index
    {
#if DEBUG
        public const string ApiBaseUrl = "http://localhost:7071";
        private static HttpClient _http;
#else
        public const string ApiBaseUrl = "";
#endif

        public static HttpClient Http
        {
            get
            {
                if (_http == null)
                {
                    _http = new HttpClient();
                }

                return _http;
            }
        }

        public bool Ready { get; set; }

        public string UserName { get; set; } = string.Empty;

        public List<Message> Messages { get; set; } = new List<Message>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                UserName = await JSRuntime.InvokeAsync<string>("askUserName");

                await Connect();

                Ready = true;
                StateHasChanged();
            }
        }

        private void FormKeyPressed(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                SendNewMessage();
            }
        }

        public EditContext CurrentEditContext { get; private set; }

        public FormModel Model { get; set; } = new FormModel();

        public string NewMessage { get; set; } 

        private async void SendNewMessage()
        {
            var message = new Message
            {
                Name = UserName,
                Text = Model.NewMessage
            };

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json);

            await Http.PostAsync($"{ApiBaseUrl}/api/talk", content);

            Model.NewMessage = string.Empty;
        }

        protected override async Task OnInitializedAsync()
        {
            CurrentEditContext = new EditContext(Model);
        }

        private HubConnection? _connection;

        private async Task Connect()
        {
            var connectionInfoJson = await Http.GetStringAsync($"{ApiBaseUrl}/api/negotiate");
            var connectionInfo = JsonConvert.DeserializeObject<ConnectionInfo>(connectionInfoJson);

            _connection = new HubConnectionBuilder()
                .WithUrl(connectionInfo.Url, options =>
                {
                    options.AccessTokenProvider = async () => connectionInfo.AccessToken;
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<JsonDocument>("newMessage", ReceiveNewMessage);

            _connection.Closed += ConnectionClosed;

            await _connection.StartAsync();
        }

        private async Task ConnectionClosed(Exception arg)
        {

        }

        private void ReceiveNewMessage(JsonDocument message)
        {
            var content = message.Deserialize<Message>();
            Messages.Insert(0, content);
            StateHasChanged();
        }
    }
}
