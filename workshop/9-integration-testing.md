# Integration Testing with .NET Aspire

## Introduction

In this module, we will cover integration testing using `Aspire.Hosting.Testing` with `MSTest`. Integration testing is crucial for ensuring that different parts of your application work together as expected. We will also explain the difference between unit testing and integration testing in the context of distributed applications with .NET Aspire.

## Unit Testing vs. Integration Testing

### Unit Testing

Unit testing focuses on testing individual components or units of code in isolation. The goal is to verify that each unit of code behaves as expected. Unit tests are typically fast and do not depend on external systems or resources.

### Integration Testing

Integration testing, on the other hand, focuses on testing the interactions between different components or systems. The goal is to ensure that the integrated components work together as expected. Integration tests may involve external systems, databases, APIs, and other resources.

In the context of distributed applications with .NET Aspire, integration testing is essential for verifying that different services and components can communicate and function correctly in a real-world environment.

## Setting Up Integration Testing

### Adding the Test Project

1. Create a new test project in your solution:
   - Right-click on the solution and select `Add` > `New Project`.
   - Select the `MSTest Test Project` template.
   - Name the project `IntegrationTests`.
   - Click `Next` > `Create`.

2. Install the required NuGet packages in the `IntegrationTests` project:

   ```bash
   dotnet add package Aspire.Hosting.Testing
   dotnet add package MSTest.TestAdapter
   dotnet add package MSTest.TestFramework
   ```

### Configuring the Test Project

1. Add references to the `Api` and `MyWeatherHub` projects in the `IntegrationTests` project:
   - Right-click on the `IntegrationTests` project and select `Add` > `Reference`.
   - Check the `Api` and `MyWeatherHub` projects and click `OK`.

2. Create a new file `IntegrationTestBase.cs` in the `IntegrationTests` project:

   ```csharp
   using Aspire.Hosting.Testing;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   [TestClass]
   public abstract class IntegrationTestBase
   {
       protected static AspireTestHost TestHost;

       [AssemblyInitialize]
       public static void AssemblyInitialize(TestContext context)
       {
           TestHost = AspireTestHost.CreateBuilder()
               .AddProject<Projects.Api>("api")
               .AddProject<Projects.MyWeatherHub>("myweatherhub")
               .Build();
       }

       [AssemblyCleanup]
       public static void AssemblyCleanup()
       {
           TestHost.Dispose();
       }
   }
   ```

## Writing Integration Tests

### API Integration Tests

1. Create a new file `ApiIntegrationTests.cs` in the `IntegrationTests` project:

   ```csharp
   using Microsoft.VisualStudio.TestTools.UnitTesting;
   using System.Net.Http;
   using System.Threading.Tasks;

   [TestClass]
   public class ApiIntegrationTests : IntegrationTestBase
   {
       private static HttpClient _client;

       [ClassInitialize]
       public static void ClassInitialize(TestContext context)
       {
           _client = TestHost.GetHttpClient("api");
       }

       [TestMethod]
       public async Task GetForecast_ReturnsSuccess()
       {
           var response = await _client.GetAsync("/forecast/AKZ018");
           response.EnsureSuccessStatusCode();
           var content = await response.Content.ReadAsStringAsync();
           Assert.IsNotNull(content);
       }
   }
   ```

### Web Application Integration Tests

1. Create a new file `WebAppIntegrationTests.cs` in the `IntegrationTests` project:

   ```csharp
   using Microsoft.VisualStudio.TestTools.UnitTesting;
   using System.Net.Http;
   using System.Threading.Tasks;

   [TestClass]
   public class WebAppIntegrationTests : IntegrationTestBase
   {
       private static HttpClient _client;

       [ClassInitialize]
       public static void ClassInitialize(TestContext context)
       {
           _client = TestHost.GetHttpClient("myweatherhub");
       }

       [TestMethod]
       public async Task HomePage_ReturnsSuccess()
       {
           var response = await _client.GetAsync("/");
           response.EnsureSuccessStatusCode();
           var content = await response.Content.ReadAsStringAsync();
           Assert.IsNotNull(content);
       }
   }
   ```

## Running the Tests

1. Run the tests using the Test Explorer in Visual Studio or the `dotnet test` command:

   ```bash
   dotnet test IntegrationTests
   ```

2. Verify that all tests pass successfully.

## Playwright for End-to-End Testing

Playwright is a powerful tool for end-to-end testing of web applications. It allows you to automate browser interactions and verify the behavior of your application from the user's perspective. Playwright supports multiple browsers, including Chromium, Firefox, and WebKit.

### Use Cases for Playwright

- **User Interface Testing**: Verify that the UI elements are rendered correctly and respond to user interactions.
- **Cross-Browser Testing**: Ensure that your application works consistently across different browsers.
- **End-to-End Testing**: Test the entire workflow of your application, from the frontend to the backend.

### Getting Started with Playwright

To get started with Playwright, refer to the official documentation: [Playwright for .NET](https://playwright.dev/dotnet/)

## Conclusion

In this module, we covered the basics of integration testing using `Aspire.Hosting.Testing` with `MSTest`. We also introduced Playwright for end-to-end testing. Integration testing is crucial for ensuring that different parts of your application work together as expected, especially in distributed applications with .NET Aspire.

**Next**: [Module #10: Advanced Topics](10-advanced-topics.md)
