using System;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace MarcRocNy.Common.Hypotheses;

public class SwitchOverEnumShould
{
    public enum ResultState
    {
        Success,
        Warning,
        ErrorOne,
        ErrorTwo,
    };

    [Fact]
    public void InexhaustveStatementCompilesAndRuns()
    {
        ResultState state = ResultState.ErrorTwo;

        string foo = "";

        switch (state)
        {
            case ResultState.Success: foo = "yes!"; break;
            case ResultState.Warning: foo = "hmm"; break;
            case ResultState.ErrorOne: foo = "nope";  break;
            //case ResultState.ErrorTwo: foo = "Error"; break;
        }

        foo.Should().BeEmpty();
    }

    [Fact]
    public void InexhaustiveExpressionWarnsCS8509AndThrows()
    {
        ResultState state = ResultState.ErrorTwo;

        // this could be escalated to an error
        Action act = () =>
        {
            string foo = state switch
            {
                ResultState.Success => "yay!",
                ResultState.Warning => "hmm",
                ResultState.ErrorOne => "dangit!",
            };
        };

        act.Should().Throw<SwitchExpressionException>();
    }

}
