# Using AI, Microsoft AI Extensions Library and OpenAI to Summarize User Comments

Either you are building an e-commerce application or a simple blog, **user comments** (about your products or blog posts) **can grow rapidly**, making it harder for users to get the gist of discussions at a glance. AI is a pretty good tool to solve the problem. By using AI, you can **summarize all the user comments** and show a single paragraph to your users, so they can easily understand the overall thought of users about the product or the blog post.

In this tutorial, we’ll walk through a real-life implementation of using AI to summarize multiple user comments in an application. I will implement the solution based on ABP's **[CMS Kit](https://abp.io/docs/latest/modules/cms-kit)** library, as it already features a **[commenting system](https://abp.io/docs/latest/modules/cms-kit/comments)** and a [demo application](https://cms-kit-demo.abpdemo.com/) that displays user comments on **[gallery images](https://cms-kit-demo.abpdemo.com/image-gallery)** (it has not a comment summary feature yet, we will implement it in this tutorial).

## A Screenshot

Here, an example screenshot from the application with the comment summary feature:

![comment-example](D:\Github\abp\docs\en\Community-Articles\2025-04-25-AI-Comment-Summarization\comment-example.png)

## Cloning the Repository

If you want to follow the development, you can clone the [CMS Kit Demo repository](https://github.com/abpframework/cms-kit-demo) to your computer and make it running by following the instructions on the [README file](https://github.com/abpframework/cms-kit-demo?tab=readme-ov-file#cms-kit-demo).

I suggest to you to play a little with [the application](https://cms-kit-demo.abpdemo.com/) (create a new user for yourself, add some comments to the images in the gallery), so you understand how it works.

## Preparing the Solution for AI

I will use [Microsoft AI Extensions Library](https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions) to use the AI features. It is an abstraction library that can work with multiple AI models and tools. I will use an OpenAI model in the demo.

The first step is to add the [Microsoft.Extensions.AI.OpenAI](http://nuget.org/packages/Microsoft.Extensions.AI.OpenAI) NuGet package to the project:

````bash
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
````

>The Microsoft AI Extensions Library was in preview at the time when I wrote this article. If it has a stable release now, you can remove the `--prerelease` parameter for the preceding command.

We will store the OpenAI key and model name in user secrets. So, locate the root path of the CMS Kit project (`src\CmsKitDemo` folder) and execute the following commands in order in a command-line terminal:

````bash
dotnet user-secrets init
dotnet user-secrets set OpenAIKey <your-openai-key>
dotnet user-secrets set ModelName <your-openai-model-name>
````

For this example, you need to have an [OpenAI API Key](https://platform.openai.com/). That's all. Now, we are ready to use the AI.

## Implementing the AI Summarization

Let's start from the most important point of this article: Comment summarization. I will create a class named `AiCommentSummarizer` to implement the summarization work. Here, the full content of that class:

````csharp
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI;
using Volo.Abp.DependencyInjection;

namespace CmsKitDemo.Utils;

public class AiCommentSummarizer : ITransientDependency
{
    private readonly IConfiguration _configuration;

    public AiCommentSummarizer(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<string> SummarizeAsync(string[] commentTexts)
    {
        // Get the model and key from the configuration
        var aiModel = _configuration["ModelName"];
        var apiKey = _configuration["OpenAIKey"];

        if (aiModel.IsNullOrEmpty() || apiKey.IsNullOrEmpty())
        {
            return "";
        }

        // Create the IChatClient
        var client = new OpenAIClient(apiKey)
            .GetChatClient(aiModel)
            .AsIChatClient();

        // Create a prompt (input for AI)
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine(
            @"There are comments from different users of our website about an image.
We want to summarize the comments into a single comment.
Return a single comment with a maximum of 512 characters. Comments are separated by a newline character and given below."
        );
        promptBuilder.AppendLine();
        
        foreach (var commentText in commentTexts)
        {
            promptBuilder.AppendLine("User comment:");
            promptBuilder.AppendLine(commentText);
            promptBuilder.AppendLine();
        }
        
        // Submit the prompt and get the response
        var response = await client.GetResponseAsync(
            promptBuilder.ToString(),
            new ChatOptions { MaxOutputTokens = 1024 }
        );

        return response.Text;
    }
}
````

That class is pretty simple and already decorated with comments:

* First, we are getting the API Key and an OpenAI model name from user secrets. I used `gpt-4.1` as the model name, but you can use another available model.
* Then we are obtaining an `IChatClient` reference for OpenAI. `IChatClient` interface is an abstraction that is provided by the [Microsoft AI Extensions Library](https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions) library, so we can implement rest of the code independently from OpenAI.
* Then we continue by building a proper prompt (input) for the AI operation.
* And finally we are using the AI to generate a response (the summary).

At this point, all the AI-related work has already been done. The rest of this article explains how to integrate that summarization feature with the [CMS Kit Demo application](https://cms-kit-demo.abpdemo.com/).

