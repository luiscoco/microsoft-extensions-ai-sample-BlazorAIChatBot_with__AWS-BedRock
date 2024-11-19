# Integrating AI ChatBot with Blazor Web App Using .NET 9 AWS_Bedrock Anthropic Claude-3 Model

## 1. How to start using AWS_Bedrock Service

### 1.1. What is AWS_Bedrock?

**Amazon Bedrock** is a fully managed service (is serverless) that offers a choice of high-performing **Foundation Models (FMs)** from leading AI companies like **AI21** Labs, **Anthropic(Claude)**, **Cohere**, **Meta(Llama 2)**, **Mistral AI**, **Stability AI**, and **Amazon(Titan)** through a single API

Build Generative AI Applications with your data. Using Amazon Bedrock, you can privately customize top FMs with your data using techniques such as Fine-Tuning and Retrieval Augmented Generation (RAG), and build Agents that execute tasks using your enterprise systems and data sources

Agents for Amazon Bedrock plan and execute multistep tasks using company systems and data source 

### 1.2. How to create AWS_Service with your AWS Console

We first **login** in **AWS Console**

We **search** for **AWS_Bedrock** Service

![image](https://github.com/user-attachments/assets/a3ca4a00-0964-456f-9135-3e602bc25b95)

We navigate to AWS_Bedrock

![image](https://github.com/user-attachments/assets/3aa95973-4f38-4389-a08b-0e69f6195687)

We request the **Model access**

![image](https://github.com/user-attachments/assets/9640006c-4a9f-4229-a797-eddd3962cc97)

## 2. We get Access_Key and Secret_access_Key in AWS Console

We right click on the user name and select the menu option **Security credentials**

![image](https://github.com/user-attachments/assets/0ce586db-9580-4f9d-a295-af9192702aa0)

We scroll down and in **Access keys** we press the **Create access key** button

![image](https://github.com/user-attachments/assets/4dd1dce0-c1ed-47a8-b72a-3706bbd5c9b3)

We select the first use case radio button option "Command Line Interface (CLI)", select the checkbox "I understand the above recommendation and want to proceed to create an access key." and press the Next button 

![image](https://github.com/user-attachments/assets/4c58cadf-b4f1-4dfe-95a6-ece9f906cf40)

We provide a description tag value and press the Create Access Key button 

![image](https://github.com/user-attachments/assets/2a4f61f9-2f57-4abd-a1ae-76b31d410695)

We copy the Access Key and Secret access key values and press the Done button

![image](https://github.com/user-attachments/assets/c3acfb64-c899-4aba-b167-04160fdf515f)

## 3. Create a Blazor Web App (.NET 9)

We run Visual Studio 2022 Community Edition and we Create a new Project

![image](https://github.com/user-attachments/assets/50ab5224-a631-4e07-95ab-856640c91f83)

We select the Blazor Web App project template

![image](https://github.com/user-attachments/assets/91f9c737-5891-403d-8178-7e8fd100a8af)

We input the **project name and location** and we press the Next button



We select the **.NET 9** framework and leave the other options with the default values, and we press the Create button

![image](https://github.com/user-attachments/assets/da2e303b-1947-43e9-bef7-0a02dc670f15)

We verify the project folders and files structure

![image](https://github.com/user-attachments/assets/8a18644b-abff-4181-9272-b0e27f0e154d)

## 4. Load the Nuget Packages for AWS_Bedrock Service



## 5. Modify the middleware(Program.cs)

We first have to register the **Log Service**

```csharp
builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});
```

We register the **Chat Client Service** for **AWS_Bedrock**

```csharp
builder.Services.AddSingleton<AmazonBedrockRuntimeClient>(static serviceProvider =>
{

var credentials = new BasicAWSCredentials("Access_Key", "Secret_access_Key");
var config = new AmazonBedrockRuntimeConfig { RegionEndpoint = RegionEndpoint.USEast1 };
var client = new AmazonBedrockRuntimeClient(credentials, config);

    return client;
});
```

**IMPORTANT NOTE**: copy the ** Key** form Section 1 and paste it in this code line

```csharp
var credentials = new BasicAWSCredentials("Access_Key", "Secret_access_Key");
```

We also have to register the **Chat Messages Service**

```csharp
builder.Services.AddSingleton<List<Message>>(static serviceProvider =>
{
    return new List<Message>()
    {
    };
});
```

We verify the whole **Program.cs** file

```csharp
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Amazon;
using BlazorAIChatBot_with_AWS_Bedrock.Components;
using BlazorAIChatBotOpenAI.Components;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using Amazon.BedrockRuntime.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});

// Register the chat client for AWS-Bedrock
builder.Services.AddSingleton<AmazonBedrockRuntimeClient>(static serviceProvider =>
{

var credentials = new BasicAWSCredentials("", "");
var config = new AmazonBedrockRuntimeConfig { RegionEndpoint = RegionEndpoint.USEast1 };
var client = new AmazonBedrockRuntimeClient(credentials, config);

    return client;
});

// Register default chat messages
builder.Services.AddSingleton<List<Message>>(static serviceProvider =>
{
    return new List<Message>()
    {
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
```

## 6. Add the Chatbot razor component

We create a new folder **Chatbot** inside the **Components** folder

![image](https://github.com/user-attachments/assets/577166ba-2a16-4e71-ac29-8d0cba624895)

Then we are going to create the classes files and razor components

We first create the **ChatState.cs**

We input the code in the **ChatState.cs** file:

```csharp
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Security.Claims;
using System.Text;

namespace BlazorAIChatBotOpenAI.Components.Chatbot;

public class ChatState
{
    private readonly ILogger _logger;
    private readonly AmazonBedrockRuntimeClient? _chatClient;
    private List<Message>? _chatMessages;
    private ConverseRequest _request;

    public List<Message>? ChatMessages { get => _chatMessages; set => _chatMessages = value; }

    public ChatState(ClaimsPrincipal user, AmazonBedrockRuntimeClient chatClient, List<Message>? chatMessages, ILogger logger)
    {
        _logger = logger;
        _chatClient = chatClient;
        ChatMessages = chatMessages;

        // Initialize the ConverseRequest here
        _request = new ConverseRequest
        {
            ModelId = "anthropic.claude-3-haiku-20240307-v1:0",
            Messages = ChatMessages,
            InferenceConfig = new InferenceConfiguration()
            {
                MaxTokens = 512,
                Temperature = 0.5F,
                TopP = 0.9F
            }
        };
    }

    public async Task AddUserMessageAsync(string userText, Action onMessageAdded)
    {
        ChatMessages.Add(
            new Message
            {
                Role = ConversationRole.User,
                Content = new List<ContentBlock> { new ContentBlock { Text = userText } }
            }
        );
        onMessageAdded();

        try
        {
            _logger.LogInformation("Sending message to chat client.");
            _logger.LogInformation($"User Text: {userText}");

            // Update the Messages property of the request object before sending
            _request.Messages = ChatMessages;

            var result = await _chatClient.ConverseAsync(_request);

            ChatMessages.Add(
                new Message
                {
                    Role = ConversationRole.Assistant,
                    Content = new List<ContentBlock> { new ContentBlock { Text = result?.Output?.Message?.Content?[0]?.Text } }
                }
            );

            _logger.LogInformation($"Assistant Response: {result?.Output?.Message?.Content?[0]?.Text}");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error getting chat completions.");
            }

            ChatMessages.Add(
                new Message
                {
                    Role = ConversationRole.Assistant,
                    Content = new List<ContentBlock> { new ContentBlock { Text = $"My apologies, but I encountered an unexpected error.\n\n<p style=\"color: red\">{e}</p>" } }
                }
            );
        }
        onMessageAdded();
    }
}
```

We also have to create the **MessageProcessor.cs** file 

```csharp
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace BlazorAIChatBotOpenAI.Components.Chatbot;

public static partial class MessageProcessor
{
    public static MarkupString AllowImages(string message)
    {
        // Having to process markdown and deal with HTML encoding isn't ideal. If the language model could return
        // search results in some defined format like JSON we could simply loop over it in .razor code. This is
        // fine for now though.

        var result = new StringBuilder();
        var prevEnd = 0;
        message = message.Replace("&lt;", "<").Replace("&gt;", ">");

        foreach (Match match in FindMarkdownImages().Matches(message))
        {
            var contentToHere = message.Substring(prevEnd, match.Index - prevEnd);
            result.Append(HtmlEncoder.Default.Encode(contentToHere));
            result.Append($"<img title=\"{(HtmlEncoder.Default.Encode(match.Groups[1].Value))}\" src=\"{(HtmlEncoder.Default.Encode(match.Groups[2].Value))}\" />");

            prevEnd = match.Index + match.Length;
        }
        result.Append(HtmlEncoder.Default.Encode(message.Substring(prevEnd)));

        return new MarkupString(result.ToString());
    }

    public static MarkupString ProcessMessageToHTML(string message)
    {
        return new MarkupString(message);
    }

    [GeneratedRegex(@"\!?\[([^\]]+)\]\s*\(([^\)]+)\)")]
    private static partial Regex FindMarkdownImages();
}
```

Now we create the razor components:

We create the ShowChatbot button

**ShowChatbotButton.razor**

```razor
@inject NavigationManager Nav

<a class="show-chatbot" href="@Nav.GetUriWithQueryParameter("chat", true)" title="Show chatbot"></a>

@if (ShowChat)
{
    <Chatbot />
}

@code {
    [SupplyParameterFromQuery(Name = "chat")]
    public bool ShowChat { get; set; }
}
```

And also we create the Chatbot razor component

**Chatbot.razor**

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
@using Amazon.BedrockRuntime
@using Amazon.BedrockRuntime.Model
@using Microsoft.AspNetCore.Components.Authorization
@using BlazorAIChatBotOpenAI.Components.Chatbot
@using System.ComponentModel

@inject IJSRuntime JS
@inject NavigationManager Nav

@inject AuthenticationStateProvider AuthenticationStateProvider
@inject ILogger Logger
@inject IConfiguration Configuration
@inject IServiceProvider ServiceProvider

<div class="floating-pane">
    <a href="@Nav.GetUriWithQueryParameter("chat", (string?)null)" class="hide-chatbot" title="Close Chat"><span>✖</span></a>

    <div class="chatbot-chat" @ref="chat">
        @if (chatState is not null)
        {
            foreach (var message in chatState.ChatMessages.Where(m => m.Role == ConversationRole.Assistant || m.Role == ConversationRole.User))
            {
            if (!string.IsNullOrEmpty(message.Content[0].Text.ToString()))
                {
                    <p @key="@message" class="message message-@message.Role">@MessageProcessor.AllowImages(message.Content[0].Text.ToString()!)</p>                    
                }
            }
        }
        else if (missingConfiguration)
        {
            <p class="message message-assistant"><strong>The chatbot is missing required configuration.</strong> Please review your app settings.</p>
        }

        @if (thinking)
        {
            <p class="thinking">"[claude3]" is Thinking...</p>
        }

    </div>

    <form class="chatbot-input" @onsubmit="SendMessageAsync">
        <textarea placeholder="Start chatting..." @ref="@textbox" @bind="messageToSend"></textarea>
        <button type="submit" title="Send" disabled="@(chatState is null)">Send</button>
    </form>
</div>

@code {
    bool missingConfiguration;
    ChatState? chatState;
    ElementReference textbox;
    ElementReference chat;
    string? messageToSend;
    bool thinking;
    IJSObjectReference? jsModule;

    protected override async Task OnInitializedAsync()
    {
        AmazonBedrockRuntimeClient? chatClient = ServiceProvider.GetService<AmazonBedrockRuntimeClient>();
        List<Message>? chatMessages = ServiceProvider.GetService<List<Message>>();
        if (chatClient is not null)
        {
            AuthenticationState auth = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            chatState = new ChatState(auth.User, chatClient, chatMessages, Logger);
        }
        else
        {
            missingConfiguration = true;
        }
    }

    private async Task SendMessageAsync()
    {
        var messageCopy = messageToSend?.Trim();
        messageToSend = null;

        if (chatState is not null && !string.IsNullOrEmpty(messageCopy))
        {
            thinking = true;
            
            await chatState.AddUserMessageAsync(messageCopy, onMessageAdded: StateHasChanged);
            
            thinking = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Chatbot/Chatbot.razor.js");
        await jsModule.InvokeVoidAsync("scrollToEnd", chat);

        if (firstRender)
        {
            await textbox.FocusAsync();
            await jsModule.InvokeVoidAsync("submitOnEnter", textbox);
        }
    }
}
```

## 7. Modify the Home.razor component

We have to invoke the Show Chatbot button from the home page, for this purpose we add the following code:

**Home.razor**

```razor
@page "/"

@using BlazorAIChatBotOpenAI.Components.Chatbot
@using BlazorAIChatBot_with_AzureOpenAI.Components.Chatbot

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

Welcome to your new app.

<ShowChatbotButton />
```

## 8. Run the application a see the results

![image](https://github.com/user-attachments/assets/ee68ce98-8a9a-4757-be5c-6583dac8b979)

![image](https://github.com/user-attachments/assets/a36c02dd-f7ce-4a31-9b1a-7810a90de99e)

We write the message and press the Send button

![image](https://github.com/user-attachments/assets/015bbb4f-35c4-4342-a987-f785778ea28d)

![image](https://github.com/user-attachments/assets/23dd2ef5-9d22-4243-9442-e7b710454a24)
