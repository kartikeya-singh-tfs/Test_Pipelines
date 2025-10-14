using ThermoFisher.Opal.Api.Tests.TestUtilities;

namespace ThermoFisher.Opal.Api.Tests;

public class ModuleRegistryBuilderTests
{
    [Fact]
    public void RegisterModule_WithValidModule_ShouldSucceed()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module = new TestMinimalModule();

        // Act
        var result = builder.RegisterModule(module);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void RegisterModule_WithNullModule_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterModule(null!));
        Assert.Equal("module", exception.ParamName);
    }

    [Fact]
    public void RegisterModule_WithDuplicateModuleName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module1 = new TestMinimalModule();
        var module2 = new TestMinimalModule();

        builder.RegisterModule(module1);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterModule(module2)
        );
        Assert.Contains(module2.Name, exception.Message);
    }

    [Fact]
    public void RegisterModule_WithValidPathPrefix_ShouldSucceed()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module = new TestHttpEndpointModule();

        // Act
        var result = builder.RegisterModule(module, "test-prefix");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void RegisterModule_WithInvalidPathPrefix_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var httpModule = new TestHttpEndpointModule();

        // Test various special characters that should be invalid in path prefixes
        var invalidPrefixes = new[]
        {
            "test path", // Contains space
            "test:port", // Contains colon
            "   ", // Contains only whitespace
            "", // Empty string
            null, // Null value
        };

        foreach (var invalidPrefix in invalidPrefixes)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                builder.RegisterModule(new TestHttpEndpointModule(), invalidPrefix!)
            );
        }
    }

    [Fact]
    public void RegisterModule_WithDuplicatePathPrefix_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module1 = new TestHttpEndpointModule();
        var module2 = new TestFullFeaturedModule();

        builder.RegisterModule(module1, "test-prefix");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterModule(module2, "test-prefix")
        );
        Assert.Contains("test-prefix", exception.Message);
    }

    [Fact]
    public void RegisterModule_HttpEndpointModuleWithoutPathPrefix_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module = new TestHttpEndpointModule();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterModule(module)
        );
        Assert.Contains("path prefix", exception.Message);
    }

    [Fact]
    public void RegisterModule_NonHttpEndpointModuleWithPathPrefix_ShouldSucceed()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module = new TestServiceModule();

        // Act
        var result = builder.RegisterModule(module, "test-prefix");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void RegisterModule_Generic_WithValidType_ShouldSucceed()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        // Act
        var result = builder.RegisterModule<TestMinimalModule>();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void RegisterModule_Generic_WithPathPrefix_ShouldSucceed()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        // Act
        var result = builder.RegisterModule<TestHttpEndpointModule>("test-prefix");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void RegisterModule_Generic_HttpEndpointModuleWithoutPathPrefix_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterModule<TestHttpEndpointModule>()
        );
        Assert.Contains("path prefix", exception.Message);
    }

    [Fact]
    public void RegisterModule_AfterBuildServiceConfigurator_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module = new TestHttpEndpointModule();

        builder.BuildServiceConfigurator();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterModule(module, "test-prefix")
        );
    }

    [Fact]
    public void BuildServiceConfigurator_MultipleCalls_ShouldReturnDifferentInstances()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        builder.RegisterModule(new TestMinimalModule());

        // Act
        var configurator1 = builder.BuildServiceConfigurator();
        var configurator2 = builder.BuildServiceConfigurator();

        // Assert
        Assert.NotSame(configurator1, configurator2);
    }

    [Fact]
    public void RegisterModule_CaseInsensitivePathPrefix_ShouldThrowForSimilarPrefixes()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var module1 = new TestHttpEndpointModule();
        var module2 = new TestFullFeaturedModule();

        builder.RegisterModule(module1, "test-prefix");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.RegisterModule(module2, "Test-prefix")
        );
    }
}
