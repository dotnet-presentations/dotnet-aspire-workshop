using Microsoft.Extensions.AI;

namespace MyWeatherHub;

public class ForecastSummarizer(IChatClient chatClient)
{
	public async Task<string> SummarizeForecastAsync(string forecasts)
	{
		
		var prompt = $"""
			You are a weather assistant. Summarize the following forecast 
			as one of the following conditions: Sunny, Cloudy, Rainy, Snowy.  
			Only those four values are allowed. Be as concise as possible.  
			I want a 1-word response with one of these options: Sunny, Cloudy, Rainy, Snowy.

			The forecast is: {forecasts}
			""";

		var response = await chatClient.GetResponseAsync(prompt);

		// Look for one of the four values in the response
		if (string.IsNullOrEmpty(response.Text))
		{
			return "Cloudy"; // Default fallback
		}

		var condition = response.Text switch
		{
			string s when s.Contains("Snowy", StringComparison.OrdinalIgnoreCase) => "Snowy",
			string s when s.Contains("Rainy", StringComparison.OrdinalIgnoreCase) => "Rainy", 
			string s when s.Contains("Cloudy", StringComparison.OrdinalIgnoreCase) => "Cloudy",
			string s when s.Contains("Sunny", StringComparison.OrdinalIgnoreCase) => "Sunny",
			string s when s.Contains("Clear", StringComparison.OrdinalIgnoreCase) => "Sunny",
			_ => "Cloudy" // Default fallback
		};

		return condition;
	}
}
