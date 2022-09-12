# Event driven applications in Azure

This demo uses the following technologies:

- Web sockets
- Azure SignalR Service
- Azure Static Web App
- Azure Functions

> This demo uses HTML / JavaScript for the frontend, and .NET for the Azure Functions backend. However you can also use JavaScript for the backend, and you can also use Blazor (.NET Web Assembly) for the frontend.

## Trying the demo

See the [SignalR simple chat demo](http://gslb.ch/signalr-simple-chat-demo) deployed here.

## Understanding the demo

Open the [Solution](./ChatServer.sln) in Visual Studio. This shows 2 projects

- ChatServer: This is the Azure Functions backend. It uses .NET but you can create a similar functionality with JavaScript. It has 2 functions
  - [Negotiate.cs](./ChatServer/Negotiate.cs): This HTTP Trigger (GET) function connects to the Azure SignalR service and gets the connection information (URL and token) that it returns to the caller.
    - [Lines 18-19](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatServer/Negotiate.cs#L18) use an Azure Function binding to connect to the SignalR service automatically *(See the note below about the connection string)*.
    - On [line 22](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatServer/Negotiate.cs#L22) we simply return the connection info to the caller. This will be used [in the client](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L96) later.

  - [Talk.cs](./ChatServer/Talk.cs): This HTTP Trigger (POST) function receives the message that it needs to send to all the recipients. It then wraps it into a SignalRMessage that it add into the SignalR queue. This sends the message to the Azure SignalR service which then dispatches it to all the registered clients.
    - On [Line 24-25](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatServer/Talk.cs#L24) we define a Function binding which connects us to the SignalR service *(See the note below about the connection string)*.
    - [Lines 32-38](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatServer/Talk.cs#L32) read the message from the request body. See the `name` and `text` here, which are sent from the [web client](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L103).
    - [Lines 40-45](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatServer/Talk.cs#L40) add the SignalR message to the queue, which will then automatically send it to the SignalR service, and dispatch it to all the registered clients.

> Note that we will need the service's connection string for this to work, which we will configure when we deploy the application to Azure further down.

- ChatClientWeb: This project is only used locally for tests. When we deploy to Azure, we will only deploy the index.html file.
  - [index.html](./ChatClientWeb/index.html): This file is the whole client, it contains JavaScript and HTMl code, and uses Vue.js for animations and view-model driven display. The code performs the following functions:
    - [Line 8](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L8) defines the base address for the functions. When we deploy we will change this.
    - [Lines 28-35](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L28) display a Form to collect the messages from the user. This is only displayed if the [ready](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L75) property is set to `true` in the view model.
      - On [Line 32](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L32), the textbox is used to collect the new messages. Note that it is databound to the [newMessage](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L72) property on the view model.
      - On [Line 31](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L31) the form is actuating the [sendMessage function](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L101) via the [sendNewMessage method](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L82). This happens when the `Enter` key is pressed.
    - [Lines 41-63](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L41) show a table of all the messages. When a message is added to the [messages](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L74) property in the view model, Vue.js will automatically display a new row.
    - [Line 51](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L51) and [Line 56](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L56) show the message's properties being displayed, namely `name` and `text`.
    - [Lines 66-68](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L66) include JavaScript frameworks to the code:
      - [Vue.js](https://vuejs.org/)
      - [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-6.0)
      - [Axios](https://axios-http.com/), a lightweight HTTP client for JavaScript.
    - [Lines 71-76](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L71) define the view model for the application, with the `newMessage` entered by the user, the user's `name`, the `messages` collection which will drive the display, and the `ready` property used to decide what parts of the UI are shown.
    - [Lines 78-87](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L78) show the Vue app, including the `sendNewMessage` method called when the `Enter` key is pressed on the form.
    - [Lines 89-94](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L89) are used to construct the options for the Axios HTTP client.
    - [Lines 96-99](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L96) are called when the page is loaded, and they call the [Negotiate Azure function](./ChatServer/Negotiate.cs) on the ChatServer. In return, they get the SignalR connection info.
    - [Lines 101-106](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L101) call the [Talk Azure function](./ChatServer/Talk.cs) on the ChatServer and pass a message to the function. The function will then wrap this and send to the SignalR hub.
    - [Lines 110-113](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L110) define the function that will be called when a new message is received. This function will be used to [configure the connection further down](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L134).
    - On [Line 115](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L115), the execution of the code starts. We ask the user's name using an old fashioned `Prompt`.
    - On [Line 122](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L122), we call the [getConnectionInfo function](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L96) which uses the Axios HTTP client to get the connection info from the SignalR service via the [Negotiate Azure function](./ChatServer/Negotiate.cs).
    - When the response from the Negotiate Azure function arrives, we use the `AccessToken` property to build a "token factory" that we will need to [build the SignalR connection](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L130).
    - On [Lines 129-132](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L129) we use the SignalR `HubConnectionBuilder` to build the connection, using the [https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L130] `url` and the `accessToken` that we obtained via the `getConnectionInfo` call earlier.
    - On [Line 134](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L134) we configure the `connection` to dispatch the messages marked with `newMessage` to the `getNewMessage` function defined earlier. Note that the `newMessage` target was set in the [Talk Azure function](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatServer/Talk.cs#L43).
    - Finally, on [Line 140](https://github.com/lbugnion/sample-functions-signalr/blob/100e78245052370e169d83e158196d9bc82c7c20/ChatClientWeb/index.html#L140) we start the connection.

> [The rest can be seen in the live demo](http://gslb.ch/signalr-simple-chat-demo).

## Deploying to Azure

Follow the steps:

- In the Azure portal, create a new SignalR service.
  - Go to the [Azure portal](https://portal.azure.com/).
  - Click on `Create a Resource`.

![]()

  - 