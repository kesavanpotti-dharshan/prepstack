using Prepstack.Domain.Common;
using Shouldly;

namespace Prepstack.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_SetsValueAndIsSuccess()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_AccessingValue_Throws()
    {
        var result = Result<int>.Failure(Error.Validation("code", "message"));

        result.IsSuccess.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ProducesFailure()
    {
        Result<int> result = Error.NotFound("code", "message");

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.NotFound);
    }
}
