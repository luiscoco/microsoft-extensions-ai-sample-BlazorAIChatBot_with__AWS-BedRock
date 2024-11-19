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
