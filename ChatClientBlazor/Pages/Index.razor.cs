using ChatClientBlazor.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Text.Json;

namespace ChatClientBlazor.Pages
{
    public partial class Index
    {
#if DEBUG
        public const string ApiBaseUrl = "http://localhost:7071";
#else
        public const string ApiBaseUrl = "https://lemon-pond-0169aab03.2.azurestaticapps.net";
#endif

        private static HttpClient _http;
        private HubConnection _connection;

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

        public EditContext CurrentEditContext { get; private set; }

        public List<Message> Messages { get; set; } = new List<Message>();

        public FormModel Model { get; set; } = new FormModel();

        public string NewMessage { get; set; }

        public bool Ready { get; set; }

        public string UserName { get; set; } = string.Empty;

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

            await _connection.StartAsync();
        }

        private void FormKeyPressed(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                SendNewMessage();
            }
        }

        private void ReceiveNewMessage(JsonDocument message)
        {
            var content = message.Deserialize<Message>();
            Messages.Insert(0, content);
            StateHasChanged();
        }

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
            StateHasChanged();
        }

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

        protected override void OnInitialized()
        {
            CurrentEditContext = new EditContext(Model);
        }
    }
}