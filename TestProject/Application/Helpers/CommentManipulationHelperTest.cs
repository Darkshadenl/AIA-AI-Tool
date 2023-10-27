using aia_api.Application.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestProject.Application.Helpers;

public class CommentManipulationHelperTest
{
    private Mock<ILogger<CommentManipulationHelper>> _logger;
    private CommentManipulationHelper _llmResponseController;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<CommentManipulationHelper>>();
        _llmResponseController = new CommentManipulationHelper(_logger.Object);
    }

    [Test]
    public void ProcessLlmResponse_ShouldReturnCodeWithNewComment_WithMultiLineComment()
    {
        // Arrange
        var expectedCode = """
                           class Calculate
                           {
                               public Calculate()
                               {
                                   CalculateSum(5, 5);
                                   CalculateProduct(5, 5);
                               }
                               
                               /// <summary>
                               /// This code calculates the sum of two numbers
                               /// </summary>
                               /// <param name="number1"></param>
                               /// <param name="number2"></param>
                               /// <returns>Sum of number1 and number2</returns>
                               public int CalculateSum(int number1, int number2)
                               {
                                   return number1 + number2;
                               }
                               
                               /// <summary>
                               /// Calculates the product of two given numbers.
                               /// </summary>
                               /// <param name="number1">The first number.</param>
                               /// <param name="number2">The second number.</param>
                               /// <returns>The product of number1 and number2.</returns>
                               public int CalculateProduct(int number1, int number2)
                               {
                                   return number1 * number2;
                               }
                           }
                           """;
        var newComment = """
                             /// <summary>
                             /// Calculates the product of two given numbers.
                             /// </summary>
                             /// <param name="number1">The first number.</param>
                             /// <param name="number2">The second number.</param>
                             /// <returns>The product of number1 and number2.</returns>
                             public int CalculateProduct(int number1, int number2)
                         """;
        var code = """
                  class Calculate
                  {
                      public Calculate()
                      {
                          CalculateSum(5, 5);
                          CalculateProduct(5, 5);
                      }
                      
                      /// <summary>
                      /// This code calculates the sum of two numbers
                      /// </summary>
                      /// <param name="number1"></param>
                      /// <param name="number2"></param>
                      /// <returns>Sum of number1 and number2</returns>
                      public int CalculateSum(int number1, int number2)
                      {
                          return number1 + number2;
                      }
                      
                      /// <summary>
                      /// Wrong comment here
                      /// </summary>
                      /// <param name="number1"></param>
                      /// <param name="number2"></param>
                      /// <returns>This is wrong</returns>
                      public int CalculateProduct(int number1, int number2)
                      {
                          return number1 * number2;
                      }
                  }
                  """;
        
        // Act
        string codeWithComments = _llmResponseController.ReplaceCommentInCode(newComment, code);
        
        // Assert
        Assert.That(codeWithComments, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void ProcessLlmResponse_ShouldReturnCodeWithNewComment_WithSingleLineComment()
    {
        // Arrange
        var expectedCode = """
                           class Calculate
                           {
                               public Calculate()
                               {
                                   CalculateSum(5, 5);
                                   CalculateProduct(5, 5);
                               }
                               
                               // This code calculates the sum of two numbers
                               public int CalculateSum(int number1, int number2)
                               {
                                   return number1 + number2;
                               }
                               
                               // Calculates the product of two given numbers.
                               public int CalculateProduct(int number1, int number2)
                               {
                                   return number1 * number2;
                               }
                           }
                           """;
        var newComment = """
                             // Calculates the product of two given numbers.
                             public int CalculateProduct(int number1, int number2)
                         """;
        var code = """
                  class Calculate
                  {
                      public Calculate()
                      {
                          CalculateSum(5, 5);
                          CalculateProduct(5, 5);
                      }
                      
                      // This code calculates the sum of two numbers
                      public int CalculateSum(int number1, int number2)
                      {
                          return number1 + number2;
                      }
                      
                      // Wrong comment here
                      public int CalculateProduct(int number1, int number2)
                      {
                          return number1 * number2;
                      }
                  }
                  """;
        
        // Act
        string codeWithComments = _llmResponseController.ReplaceCommentInCode(newComment, code);
        
        // Assert
        Assert.That(codeWithComments, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void ProcessLlmResponse_ShouldReturnOriginalCode_WithMultiLineComment()
    {
        // Arrange
        var expectedCode = """
                           class Calculate
                           {
                               public Calculate()
                               {
                                   CalculateSum(5, 5);
                                   CalculateProduct(5, 5);
                               }
                               
                               /// <summary>
                               /// This code calculates the sum of two numbers
                               /// </summary>
                               /// <param name="number1"></param>
                               /// <param name="number2"></param>
                               /// <returns>Sum of number1 and number2</returns>
                               public int CalculateSum(int number1, int number2)
                               {
                                   return number1 + number2;
                               }
                               
                               /// <summary>
                               /// Wrong comment here
                               /// </summary>
                               /// <param name="number1"></param>
                               /// <param name="number2"></param>
                               /// <returns>This is wrong</returns>
                               public int WrongMethodName(int number1, int number2)
                               {
                                   return number1 * number2;
                               }
                           }
                           """;
        var newComment = """
                             /// <summary>
                             /// Calculates the product of two given numbers.
                             /// </summary>
                             /// <param name="number1">The first number.</param>
                             /// <param name="number2">The second number.</param>
                             /// <returns>The product of number1 and number2.</returns>
                             public int CalculateProduct(int number1, int number2)
                         """;
        var code = """
                  class Calculate
                  {
                      public Calculate()
                      {
                          CalculateSum(5, 5);
                          CalculateProduct(5, 5);
                      }
                      
                      /// <summary>
                      /// This code calculates the sum of two numbers
                      /// </summary>
                      /// <param name="number1"></param>
                      /// <param name="number2"></param>
                      /// <returns>Sum of number1 and number2</returns>
                      public int CalculateSum(int number1, int number2)
                      {
                          return number1 + number2;
                      }
                      
                      /// <summary>
                      /// Wrong comment here
                      /// </summary>
                      /// <param name="number1"></param>
                      /// <param name="number2"></param>
                      /// <returns>This is wrong</returns>
                      public int WrongMethodName(int number1, int number2)
                      {
                          return number1 * number2;
                      }
                  }
                  """;

        // Act
        string codeWithComments = _llmResponseController.ReplaceCommentInCode(newComment, code);
        
        // Assert
        Assert.That(codeWithComments, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void ProcessLlmResponse_ShouldReturnOriginalCode_WithSingleLineComment()
    {
        // Arrange
        var expectedCode = """
                           class Calculate
                           {
                               public Calculate()
                               {
                                   CalculateSum(5, 5);
                                   CalculateProduct(5, 5);
                               }
                               
                               // This code calculates the sum of two numbers
                               public int CalculateSum(int number1, int number2)
                               {
                                   return number1 + number2;
                               }
                               
                               // Wrong comment here
                               public int WrongMethodName(int number1, int number2)
                               {
                                   return number1 * number2;
                               }
                           }
                           """;
        var newComment = """
                             // Calculates the product of two given numbers.
                             public int CalculateProduct(int number1, int number2)
                         """;
        var code = """
                  class Calculate
                  {
                      public Calculate()
                      {
                          CalculateSum(5, 5);
                          CalculateProduct(5, 5);
                      }
                      
                      // This code calculates the sum of two numbers
                      public int CalculateSum(int number1, int number2)
                      {
                          return number1 + number2;
                      }
                      
                      // Wrong comment here
                      public int WrongMethodName(int number1, int number2)
                      {
                          return number1 * number2;
                      }
                  }
                  """;
        
        string codeWithComments = _llmResponseController.ReplaceCommentInCode(newComment, code);
        Assert.That(codeWithComments, Is.EqualTo(expectedCode));
    }

    [Test]
    public void ProcessLlmResponse_ShouldCombineTokens()
    {
        // Arrange
        var expectedResult = "This is a token.";
        var tokens = new string[] { "This", " ", "is", " ", "a", " ", "to", "ken", "." };
        
        // Act
        var combinedTokens = _llmResponseController.CombineTokens(tokens);
        
        // Assert
        Assert.That(combinedTokens, Is.EqualTo(expectedResult));
    }
}