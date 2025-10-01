namespace Aspire.Hosting;

public static class ApiCommandExtenions
{

	public static IResourceBuilder<ProjectResource> WithApiCacheInvalidation(
			this IResourceBuilder<ProjectResource> builder,
			IResourceBuilder<ParameterResource> invalidationKey)
	{

		builder.WithEnvironment("ApiCacheInvalidationKey", invalidationKey)
			.WithHttpCommand(
				path: "/cache/invalidate",
				displayName: "Invalidate Cache",
				commandOptions: new HttpCommandOptions
				{
					Description = "Invalidates the API cache by removing all output cache.",
					PrepareRequest = (context) =>
					{
						var key = invalidationKey.Resource.Value;
						context.Request.Headers.Add("X-CacheInvalidation-Key",
							$"Key: {key}");
						return Task.CompletedTask;
					},
					Method = HttpMethod.Post,
					IconName = "DocumentLightning",
					IconVariant = IconVariant.Filled,
					ConfirmationMessage = "Are you sure you want to invalidate the API cache?",
				});

		return builder;
	}

}