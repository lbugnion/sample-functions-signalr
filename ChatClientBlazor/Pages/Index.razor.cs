using ChatClientBlazor.Model;
using Microsoft.JSInterop;

namespace ChatClientBlazor.Pages
{
    public partial class Index
    {
#if DEBUG
        public const string ApiBaseUrl = "http://localhost:7071";
#else
        public const string ApiBaseUrl = "";
#endif

        public bool Ready { get; set; }

        public string UserName { get; set; } = string.Empty;

        public List<Message> Messages { get; set; } = new List<Message>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                UserName = await JSRuntime.InvokeAsync<string>("askUserName");
                Ready = true;
                StateHasChanged();
            }
        }
    }
}
