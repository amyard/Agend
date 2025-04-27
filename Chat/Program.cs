using Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
// builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3"));
builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.2"));

var app = builder.Build();
IChatClient chatClient = app.Services.GetRequiredService<IChatClient>();

// update to newest version will break this
// await SimpleQuestions.Call(chatClient);

// save somewhere in DB the json of chatHistory and read it before current code
// need type reply 50 words. and find solution to display response immediately
// await ChatWithHistory.Call(chatClient);


// SUMMARIZATION
// await Summarization.Call(chatClient);

// CATEGORIZATION
// await Categoriazation.Call(chatClient);

// FULL
await ChatHistoryFull.Call(chatClient, 1);


Console.WriteLine("Hello, World!");




