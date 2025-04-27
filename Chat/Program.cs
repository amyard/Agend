using Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
// builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3"));
// builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.2"));


builder.Services.AddSingleton<IChatClientFactory, ChatClientFactory>();

var app = builder.Build();

// old implementation
// IChatClient chatClient = app.Services.GetRequiredService<IChatClient>();


// new implementation
var factory = builder.Services.BuildServiceProvider().GetRequiredService<IChatClientFactory>();
var llamaClient = factory.GetClient("llama");
var llavaClient = factory.GetClient("llava");


// update to newest version will break this
// await SimpleQuestions.Call(llamaClient);

// save somewhere in DB the json of chatHistory and read it before current code
// need type reply 50 words. and find solution to display response immediately
// await ChatWithHistory.Call(chatClient);


// SUMMARIZATION
// await Summarization.Call(chatClient);

// CATEGORIZATION
// await Categoriazation.Call(chatClient);

// FULL new for production
// await ChatHistoryFull.Call(chatClient, 1);

// image
Console.WriteLine("Start !!!");
// await ImageRecognition.Call(llavaClient);
await ImageRecognitionWithChat.Call(llavaClient);


Console.WriteLine("Hello, World!");





