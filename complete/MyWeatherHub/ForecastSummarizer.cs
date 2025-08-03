using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace MyWeatherHub;

public class ForecastSummarizer(IChatClient chatClient)
{
	public async Task<string> SummarizeForecastAsync(string forecasts)
	{
		
		var prompt = $"""
			You are a weather assistant. Summarize the following forecast 
			as one of the following conditions: Sunny, Cloudy, Rainy, Snowy.  
			Only those four values are allowed.  Be as concise as possible.  
			I want a 1-word response with one of these options: Sunny, Cloudy, Rainy, Snowy.

			The forecast is: {forecasts}
			""";

		var response = await chatClient.GetResponseAsync(prompt);

		// look for one of the four values in the response
		if (!response?.Messages.Any() ?? true)
		{
			return "unknown";
		}

		var condition = response.Messages.First().Text switch
		{
			string s when s.Contains("Snowy", StringComparison.OrdinalIgnoreCase) => "Snowy",
			string s when s.Contains("Rainy", StringComparison.OrdinalIgnoreCase) => "Rainy",
			string s when s.Contains("Cloudy", StringComparison.OrdinalIgnoreCase) => "Cloudy",
			string s when s.Contains("Sunny", StringComparison.OrdinalIgnoreCase) => "Sunny",
			string s when s.Contains("Clear", StringComparison.OrdinalIgnoreCase) => "Clear",
			_ => null
		};

		if (condition is null) Console.WriteLine($"Condition reported is {response.Messages.First().Text}");

		return condition ?? "unknown";

	}
}
